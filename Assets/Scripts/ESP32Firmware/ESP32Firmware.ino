#include "Wire.h"
#include "I2Cdev.h"
#include "MPU6050_6Axis_MotionApps612.h" // Ensure you have this library
#include "BluetoothSerial.h"

// Constants
const int NUM_POTS = 5;                    // Number of potentiometers
const float DEADZONE_THRESHOLD = 0.02f;    // **REDUCED** Deadzone for quaternion comparison (more sensitive)
const unsigned long RESET_DELAY_MS = 850;  // Delay for orientation auto-reset (currently disabled below)

// MPU6050 object
MPU6050 mpu(0x68); // Default MPU6050 address

// Bluetooth
BluetoothSerial SerialBT;

// Potentiometer pins and values
const int potPins[NUM_POTS] = {36, 39, 34, 35, 32}; // ESP32 ADC1 pins
int potValues[NUM_POTS] = {0};

// Deadzone and reset logic state
Quaternion lastQ = {1.0f, 0.0f, 0.0f, 0.0f}; // Last known quaternion
unsigned long lastMovementTime = 0;            // Timestamp of the last significant movement

// Utility function to compare quaternions within a deadzone
bool isInDeadzone(const Quaternion& a, const Quaternion& b, float threshold) {
    return abs(a.w - b.w) < threshold &&
           abs(a.x - b.x) < threshold &&
           abs(a.y - b.y) < threshold &&
           abs(a.z - b.z) < threshold;
}

// Log helper for USB Serial and Bluetooth Serial
void logAll(const char* message) {
    Serial.println(message);
    if (SerialBT.hasClient()) {
        SerialBT.println(message);
    }
}

void logAll(const String& message) {
    Serial.println(message);
    if (SerialBT.hasClient()) {
        SerialBT.println(message);
    }
}

void setup() {
    Wire.begin();
    Wire.setClock(100000); // Or 400000 if stable for your setup

    Serial.begin(115200);

    SerialBT.begin("ESP32-HandTracker");
    logAll("{\"key\": \"/log\", \"value\": \"Bluetooth ESP32-HandTracker started.\", \"level\": \"INFO\"}");

    logAll("{\"key\": \"/log\", \"value\": \"Initializing MPU6050...\", \"level\": \"DEBUG\"}");
    mpu.initialize();

    if (!mpu.testConnection()) {
        logAll("{\"key\": \"/log\", \"value\": \"MPU6050 connection failed. Check wiring.\", \"level\": \"ERROR\"}");
    } else {
        logAll("{\"key\": \"/log\", \"value\": \"MPU6050 connection successful.\", \"level\": \"INFO\"}");
    }

    logAll("{\"key\": \"/log\", \"value\": \"Initializing DMP...\", \"level\": \"DEBUG\"}");
    uint8_t dmpInitStatus = mpu.dmpInitialize();

    if (dmpInitStatus == 0) {
        // *** CRITICAL: MPU6050 CALIBRATION NEEDED ***
        // The default offsets (0) are unlikely to be correct for your sensor.
        // You NEED to implement a calibration routine here to determine the correct
        // gyro and accelerometer offsets for your specific MPU6050.
        // Without proper calibration, you will experience drift and inaccurate orientation.
        // Look for calibration sketch examples provided with the MPU6050 library.
        // Example: Run a calibration sketch once, note the offsets, then hardcode them here,
        // or implement a calibration sequence that runs on every startup.
        logAll("{\"key\": \"/log\", \"value\": \"MPU6050 CALIBRATION REQUIRED for accurate orientation! Using default (likely incorrect) offsets.\", \"level\": \"WARNING\"}");
        mpu.setXGyroOffset(0); // Replace 0 with your calibrated offset
        mpu.setYGyroOffset(0); // Replace 0 with your calibrated offset
        mpu.setZGyroOffset(0); // Replace 0 with your calibrated offset
        mpu.setXAccelOffset(0); // Replace 0 with your calibrated offset (less critical for DMP quaternion)
        mpu.setYAccelOffset(0); // Replace 0 with your calibrated offset
        mpu.setZAccelOffset(0); // Replace 0 with your calibrated offset


        mpu.setDMPEnabled(true);
        logAll("{\"key\": \"/log\", \"value\": \"DMP enabled and MPU6050 ready.\", \"level\": \"INFO\"}");
        lastMovementTime = millis();
    } else {
        char errorMsg[150];
        snprintf(errorMsg, sizeof(errorMsg),
                 "{\"key\": \"/log\", \"value\": \"DMP Initialization failed (code %d). Check MPU6050.\", \"level\": \"ERROR\"}",
                 dmpInitStatus);
        logAll(errorMsg);
        logAll("{\"key\": \"/log\", \"value\": \"Halting execution due to DMP init failure.\", \"level\": \"CRITICAL\"}");
        while (1) delay(100);
    }
}

void loop() {
    if (!mpu.getDMPEnabled()) {
        return;
    }

    uint8_t fifoBuffer[64];
    Quaternion q_from_sensor; // Raw sensor reading
    Quaternion q_to_send;     // Quaternion to actually send (might be modified by deadzone/reset)

    if (mpu.dmpGetCurrentFIFOPacket(fifoBuffer)) {
        mpu.dmpGetQuaternion(&q_from_sensor, fifoBuffer);
        q_to_send = q_from_sensor; // Start with the current sensor reading

        // Deadzone logic
        if (isInDeadzone(q_from_sensor, lastQ, DEADZONE_THRESHOLD)) {
            // If in deadzone, you might choose to send the last 'good' quaternion (lastQ)
            // to reduce jitter, or send the current q_from_sensor.
            // For now, we'll send q_from_sensor, but the auto-reset is disabled.
            q_to_send = q_from_sensor; // or q_to_send = lastQ; for less jitter when "still"

            // Auto-reset logic (CURRENTLY COMMENTED OUT TO DISABLE IT)
            /*
            if (millis() - lastMovementTime > RESET_DELAY_MS) {
                // If stationary for too long, reset orientation to identity
                q_to_send.w = 1.0f; q_to_send.x = 0.0f; q_to_send.y = 0.0f; q_to_send.z = 0.0f;
                lastQ = q_to_send; // Update lastQ to the reset state
                // logAll("{\"key\": \"/log\", \"value\": \"Orientation auto-reset due to inactivity.\", \"level\": \"INFO\"}");
            }
            */
        } else {
            // Movement detected, update last known state and time
            lastMovementTime = millis();
            lastQ = q_from_sensor; // Update lastQ with the new "moving" orientation
            // q_to_send is already q_from_sensor
        }

        // Send quaternion data
        char orientationBuffer[180];
        snprintf(orientationBuffer, sizeof(orientationBuffer),
                 "{\"key\": \"/orientation\", \"value\": [%.6f, %.6f, %.6f, %.6f]}",
                 q_to_send.w, q_to_send.x, q_to_send.y, q_to_send.z);
        logAll(orientationBuffer);
    }

    // Read and send potentiometer values
    String potData = "{\"key\": \"/pots\", \"value\": [";
    for (int i = 0; i < NUM_POTS; i++) {
        potValues[i] = analogRead(potPins[i]);
        potData += String(potValues[i]);
        if (i < NUM_POTS - 1) {
            potData += ", ";
        }
    }
    potData += "]}";
    logAll(potData);

    delay(10); // Small delay
}