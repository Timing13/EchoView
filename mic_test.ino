#define MIC_PIN 2        // A1 is GPIO2
#define NUM_SAMPLES (4096)  // Number of samples for averaging

void setup() {
    Serial.begin(115200);
    while (!Serial) {
        // Wait for serial connection
    }

    // Print column header for Serial Plotter
    Serial.println("Mic_Avg");
}

void loop() {
    long sum = 0;  // Use long to prevent overflow

    // Take 100 samples as quickly as possible
    for (int i = 0; i < NUM_SAMPLES; i++) {
        sum += analogRead(MIC_PIN);
    }

    // Compute the average
    int avgValue = sum / NUM_SAMPLES;

    // Print the average value for plotting
    Serial.println(avgValue);

    //delay(50);  // Adjust delay for smooth plotting
}
