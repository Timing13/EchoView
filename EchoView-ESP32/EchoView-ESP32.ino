#include <WebSocketsServer.h>
#include <sstream>
#include "header.h"

#define MIC_PIN 2           // A1 is GPIO2
#define NUM_SAMPLES (4096)  // Number of samples for averaging

int currentClientNumber;

void setup()
{
    Serial.begin(115200);
    while (!Serial)
    {
        // Wait for serial connection
    }

    // Print column header for Serial Plotter
    Serial.println("Mic_Avg");

    connect2Network();
}

void loop()
{
    long sum = 0;  // Use long to prevent overflow

    // Take 100 samples as quickly as possible
    for (int i = 0; i < NUM_SAMPLES; i++)
    {
        sum += analogRead(MIC_PIN);
    }

    // Compute the average
    int avgValue = sum / NUM_SAMPLES;

    // Print the average value for plotting
    Serial.println(avgValue);

    //delay(50);  // Adjust delay for smooth plotting

    if (WiFi.status() != WL_CONNECTED)
    {
        Serial.println("Lost connection to network! Attempting to reconnect...");
        connect2Network();
        return;
    }

    webSocket.loop();
    if (currentClientNumber != webSocket.connectedClients())
    {
        if (currentClientNumber < webSocket.connectedClients())
            Serial.println("A new client has connected!");
        else
            Serial.println("A client has disconnected");
        Serial.print("Number of clients currently connected: ");
        Serial.println(webSocket.connectedClients());

        currentClientNumber = webSocket.connectedClients();
    }

    JSONtxt = String("{\"MicrophoneValue\":");
    JSONtxt.concat(avgValue);
    JSONtxt.concat("}");
    webSocket.broadcastTXT(JSONtxt);
}
