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
const int potPins[NUM_POTS] = {36, 39, 34, 35, 32}; // Your specified pin order
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
void logToConnectedInterface(const char* message) {
    if (SerialBT.hasClient()) {
        SerialBT.println(message);
    } else if (Serial) { // Check if USB serial is available/connected
        Serial.println(message);
    }
}

// --- SETUP FUNCTION - Unchanged from last stable version ---
void setup() {
    Serial.begin(115200);
    delay(2000); 
    Serial.println("Starting setup...");

    Wire.begin();
    Wire.setClock(100000);
    delay(100);

    SerialBT.begin("ESP32-HandTracker-left");
    Serial.println("{\"key\": \"/log\", \"value\": \"Bluetooth started.\", \"level\": \"INFO\"}");

    Serial.println("{\"key\": \"/log\", \"value\": \"Initializing MPU6050...\", \"level\": \"DEBUG\"}");
    mpu.initialize();
    delay(100);

    Serial.println("{\"key\": \"/log\", \"value\": \"Initializing DMP...\", \"level\": \"DEBUG\"}");
    uint8_t dmpInitStatus = mpu.dmpInitialize();

    if (dmpInitStatus == 0) {
        Serial.println("{\"key\": \"/log\", \"value\": \"DMP Initialization successful.\", \"level\": \"INFO\"}");
        mpu.setXGyroOffset(0); mpu.setYGyroOffset(0); mpu.setZGyroOffset(0);
        mpu.setXAccelOffset(0); mpu.setYAccelOffset(0); mpu.setZAccelOffset(0);
        mpu.setDMPEnabled(true);
        Serial.println("{\"key\": \"/log\", \"value\": \"DMP enabled and MPU6050 ready.\", \"level\": \"INFO\"}");
        lastMovementTime = millis();
    } else {
        char errorMsg[150];
        snprintf(errorMsg, sizeof(errorMsg), "{\"key\": \"/log\", \"value\": \"CRITICAL: DMP Initialization FAILED (code %d). Halting.\", \"level\": \"ERROR\"}", dmpInitStatus);
        Serial.println(errorMsg);
        while (1) delay(100);
    }
}

// --- LOOP FUNCTION - With new combined JSON packet ---
void loop() {
    if (!mpu.getDMPEnabled()) { return; }
    if (!Serial && !SerialBT.hasClient()) { delay(100); return; }

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

        // Read potentiometer values
        for (int i = 0; i < NUM_POTS; i++) {
            potValues[i] = analogRead(potPins[i]);
        }

        // --- Create a single, efficient JSON packet ---
        char dataPacket[256]; // Buffer for our combined JSON string
        snprintf(dataPacket, sizeof(dataPacket),
                 "{\"q\":[%.6f,%.6f,%.6f,%.6f],\"p\":[%d,%d,%d,%d,%d]}",
                 q_to_send.w, q_to_send.x, q_to_send.y, q_to_send.z,
                 potValues[0], potValues[1], potValues[2], potValues[3], potValues[4]
        );
        
        // Send the single packet
        logToConnectedInterface(dataPacket);
    }

    // --- New delay to ensure data rate is below baud rate ---
    delay(20); // ~50Hz, well within 115200 baud limit. Still very smooth for tracking.
}