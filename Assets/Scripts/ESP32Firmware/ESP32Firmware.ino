#include "Wire.h"
#include "I2Cdev.h"
#include "MPU6050_6Axis_MotionApps612.h"
#include "BluetoothSerial.h"

// Constants
const int NUM_POTS = 5;
const float DEADZONE_THRESHOLD = 0.02f;

// MPU6050 object
MPU6050 mpu(0x68);

// Bluetooth
BluetoothSerial SerialBT;

// Potentiometer pins and values
const int potPins[NUM_POTS] = {36, 39, 34, 35, 32};
int potValues[NUM_POTS] = {0};

// Deadzone logic state
Quaternion lastKnownOrientation = {1.0f, 0.0f, 0.0f, 0.0f};
unsigned long lastMovementTime = 0;

// Utility function to compare quaternions
bool isInDeadzone(const Quaternion& a, const Quaternion& b, float threshold) {
    return abs(a.w - b.w) < threshold &&
           abs(a.x - b.x) < threshold &&
           abs(a.y - b.y) < threshold &&
           abs(a.z - b.z) < threshold;
}

// Smart logging function
void logToConnectedInterface(const String& message) {
    if (SerialBT.hasClient()) {
        SerialBT.println(message);
    } else {
        Serial.println(message);
    }
}
void logToConnectedInterface(const char* message) {
    if (SerialBT.hasClient()) {
        SerialBT.println(message);
    } else {
        Serial.println(message);
    }
}

// --- NEW "BRUTE-FORCE" SETUP FUNCTION ---
void setup() {
    Serial.begin(115200);
    // Add a long initial delay to allow all hardware to stabilize after power-on.
    delay(2000); 
    Serial.println("Starting setup...");

    Wire.begin();
    Wire.setClock(100000);
    delay(100); // Small delay after I2C bus starts

    SerialBT.begin("ESP32-HandTracker");
    Serial.println("{\"key\": \"/log\", \"value\": \"Bluetooth started.\", \"level\": \"INFO\"}");

    Serial.println("{\"key\": \"/log\", \"value\": \"Initializing MPU6050...\", \"level\": \"DEBUG\"}");
    mpu.initialize();
    delay(100); // Small delay after MPU init

    Serial.println("{\"key\": \"/log\", \"value\": \"Initializing DMP...\", \"level\": \"DEBUG\"}");
    uint8_t dmpInitStatus = mpu.dmpInitialize();

    // Check if DMP initialization was successful. This is our main connection test now.
    if (dmpInitStatus == 0) {
        Serial.println("{\"key\": \"/log\", \"value\": \"DMP Initialization successful.\", \"level\": \"INFO\"}");

        // The testConnection() call is removed entirely.
        
        // Set offsets to 0 for a known starting state.
        mpu.setXGyroOffset(0);
        mpu.setYGyroOffset(0);
        mpu.setZGyroOffset(0);
        mpu.setXAccelOffset(0);
        mpu.setYAccelOffset(0);
        mpu.setZAccelOffset(0);

        // Enable the DMP
        mpu.setDMPEnabled(true);
        Serial.println("{\"key\": \"/log\", \"value\": \"DMP enabled and MPU6050 ready.\", \"level\": \"INFO\"}");
        lastMovementTime = millis();

    } else {
        // If DMP fails, we cannot continue.
        char errorMsg[150];
        snprintf(errorMsg, sizeof(errorMsg),
                 "{\"key\": \"/log\", \"value\": \"CRITICAL: DMP Initialization FAILED (code %d). Halting.\", \"level\": \"ERROR\"}",
                 dmpInitStatus);
        Serial.println(errorMsg);
        while (1) delay(100); // Halt
    }
}

// --- LOOP FUNCTION - Unchanged ---
void loop() {
    if (!mpu.getDMPEnabled()) {
        return;
    }
    
    if (!Serial && !SerialBT.hasClient()) {
        delay(100); 
        return;
    }

    uint8_t fifoBuffer[64]; 
    if (mpu.dmpGetCurrentFIFOPacket(fifoBuffer)) {
        Quaternion q_from_sensor;
        Quaternion q_to_send;
        
        mpu.dmpGetQuaternion(&q_from_sensor, fifoBuffer);

        if (isInDeadzone(q_from_sensor, lastKnownOrientation, DEADZONE_THRESHOLD)) {
            q_to_send = lastKnownOrientation;
        } else {
            lastMovementTime = millis();
            lastKnownOrientation = q_from_sensor;
            q_to_send = q_from_sensor;
        }

        char orientationBuffer[180];
        snprintf(orientationBuffer, sizeof(orientationBuffer),
                 "{\"key\": \"/orientation\", \"value\": [%.6f, %.6f, %.6f, %.6f]}",
                 q_to_send.w, q_to_send.x, q_to_send.y, q_to_send.z);
        logToConnectedInterface(orientationBuffer);
    }

    String potData = "{\"key\": \"/pots\", \"value\": [";
    for (int i = 0; i < NUM_POTS; i++) {
        potValues[i] = analogRead(potPins[i]);
        potData += String(potValues[i]);
        if (i < NUM_POTS - 1) {
            potData += ", ";
        }
    }
    potData += "]}";
    logToConnectedInterface(potData);

    delay(10); 
}