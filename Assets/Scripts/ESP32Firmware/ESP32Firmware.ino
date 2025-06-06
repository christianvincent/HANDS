#include "Wire.h"
#include "I2Cdev.h"
#include "MPU6050_6Axis_MotionApps612.h"
#include "BluetoothSerial.h"

// Constants
const int NUM_POTS = 5;
const float DEADZONE_THRESHOLD = 0.02f;   // How much the sensor can move before we consider it "moving"
// const unsigned long RESET_DELAY_MS = 1500; // No longer needed, as auto-reset is disabled

// MPU6050 object
MPU6050 mpu(0x68);

// Bluetooth
BluetoothSerial SerialBT;

// Potentiometer pins and values
const int potPins[NUM_POTS] = {36, 39, 34, 35, 32};
int potValues[NUM_POTS] = {0};

// Deadzone logic state
Quaternion lastKnownOrientation = {1.0f, 0.0f, 0.0f, 0.0f}; // Last orientation when hand was moving
unsigned long lastMovementTime = 0; // Still useful for debugging or other potential features

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

// --- SETUP FUNCTION - UNCHANGED ---
void setup() {
    Wire.begin();
    Wire.setClock(100000);

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
        logAll("{\"key\": \"/log\", \"value\": \"Using default offsets. Unity-side calibration is recommended.\", \"level\": \"WARNING\"}");
        mpu.setXGyroOffset(0);
        mpu.setYGyroOffset(0);
        mpu.setZGyroOffset(0);
        mpu.setXAccelOffset(0);
        mpu.setYAccelOffset(0);
        mpu.setZAccelOffset(0);

        mpu.setDMPEnabled(true);
        logAll("{\"key\": \"/log\", \"value\": \"DMP enabled and MPU6050 ready.\", \"level\": \"INFO\"}");
        lastMovementTime = millis();
    } else {
        char errorMsg[150];
        snprintf(errorMsg, sizeof(errorMsg),
                 "{\"key\": \"/log\", \"value\": \"DMP Initialization failed (code %d). Check MPU6050.\", \"level\": \"ERROR\"}",
                 dmpInitStatus);
        logAll(errorMsg);
        while (1) delay(100);
    }
}

// --- LOOP FUNCTION - WITH AUTO-RESET DISABLED ---
void loop() {
    if (!mpu.getDMPEnabled() || !SerialBT.hasClient()) {
        delay(100); 
        return;
    }

    uint8_t fifoBuffer[64];
    if (mpu.dmpGetCurrentFIFOPacket(fifoBuffer)) {
        Quaternion q_from_sensor;
        Quaternion q_to_send;
        
        mpu.dmpGetQuaternion(&q_from_sensor, fifoBuffer);

        // --- Deadzone Logic (Auto-Reset Disabled) ---
        if (isInDeadzone(q_from_sensor, lastKnownOrientation, DEADZONE_THRESHOLD)) {
            // Hand is considered still. Use the last known good orientation
            // to eliminate any small jittering.
            q_to_send = lastKnownOrientation;
        } else {
            // Hand is moving. Update the state.
            lastMovementTime = millis();
            lastKnownOrientation = q_from_sensor;
            q_to_send = q_from_sensor;
        }
        // --- End of Logic ---

        // Send orientation data
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

    delay(10); // Maintain a consistent loop rate
}