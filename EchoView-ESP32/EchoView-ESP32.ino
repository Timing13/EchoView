#include <WebSocketsServer.h>
#include <sstream>
#include "header.h"

//#define MIC_PIN 2           // A1 is GPIO2
#define MICROPHONES 1         // Number of microphones

//Constants to define microphones direction
#define NORTH 0
#define EAST 1
#define WEST 2
#define SOUTH 3
#define NUM_SAMPLES (4096)  // Number of samples for averaging

int micPins[MICROPHONES];   // Array to match the microphones with their physical pin

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

    // micPins initialization
    micPins[NORTH] = 1;       // SDA - GPIO01
    micPins[EAST] = 2;        // SCL - GPIO02
    micPins[WEST] = 4;        // GPIO04
    micPins[SOUTH] = 6;       // GPIO06

    connect2Network();
}

void loop()
{
    long sum[MICROPHONES];  // Use long to prevent overflow
    int  avgValues[MICROPHONES];

    for(int dir = 0; dir < MICROPHONES; dir++){
      sum[dir] = 0;  // Initialization
      // Take 100 samples as quickly as possible
      for (int i = 0; i < NUM_SAMPLES; i++)
      {
          sum[dir] += analogRead(micPins[dir]);
      }
      avgValues[dir] = sum[dir] / NUM_SAMPLES;                       // Compute the average
      //Serial.println(String("Microphone ") + dir + ": " + avgValues[dir]);   // Print the average value for plotting
      Serial.println(String(analogRead(micPins[dir])));
    }

    // Selecting the Source Microphone
    int sourceDir = -1; // Default value
    int sourceAvg = -1; // Default value
    for(int dir = 0; dir < MICROPHONES; dir++){
      if(avgValues[dir] > sourceAvg){
        sourceAvg = avgValues[dir];
        sourceDir = dir;
      }
    }

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

    messageTXT = String(sourceDir);
    messageTXT.concat("$");
    messageTXT.concat(String(sourceAvg));
    webSocket.broadcastTXT(messageTXT);
}
