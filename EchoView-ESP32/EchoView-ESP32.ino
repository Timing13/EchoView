#include <WebSocketsServer.h>
#include <sstream>
#include "header.h"

//#define MIC_PIN 2           // A1 is GPIO2
#define MICROPHONES 4         // Number of microphones

//Constants to define microphones direction
#define NORTH 0
#define EAST 1
#define WEST 2
#define SOUTH 3
#define NUM_SAMPLES (100)  // Number of samples for averaging

int micPins[MICROPHONES];   // Array to match the microphones with their physical pin

int currentClientNumber, micAvg = 0;
int threshold = 200;
int sourceDir = 4; // Default value
int sourceAmp = -1; // Default value

String directions[MICROPHONES+1] = {"NORTH", "EAST", "WEST", "SOUTH", "UNDEFINED"}; 

void setup()
{
    Serial.begin(115200);
    while (!Serial)
    {
        // Wait for serial connection
    }

    // micPins initialization
    micPins[NORTH] = 1;       // SDA - GPIO01
    micPins[EAST] = 6;        // GPIO06
    micPins[WEST] = 4;        // GPIO04
    micPins[SOUTH] = 2;       // SCL - GPIO02

    connect2Network();
}

void loop()
{
    long sum[MICROPHONES];             // Use long to prevent overflow
    int  readValues[MICROPHONES];
    int  currentMicAvg = 0;            //Used for normalizing

    for(int dir = 0; dir < MICROPHONES; dir++){
      sum[dir] = 0;  // Initialization
      // Take 100 samples as quickly as possible and do the average
      for (int i = 0; i < NUM_SAMPLES; i++)
      {
          sum[dir] += analogRead(micPins[dir]);
      }
      readValues[dir] = sum[dir] / NUM_SAMPLES;     //Average of the 100 samples capture
    }

    //Compute the average of the values coming from the 4 microphones
     for (int dir = 0; dir < MICROPHONES; dir++){
        currentMicAvg += readValues[dir];           //Calculating the average value between the instant read values from all directions
    }
      currentMicAvg = currentMicAvg / MICROPHONES;
      micAvg = (micAvg + currentMicAvg) / 2;        //Global average of the values read by the microphones 

    //Subtract 'micAvg' to the read values the average value and normalize the intensity
    for (int dir = 0; dir < MICROPHONES; dir++){
      readValues[dir] = abs(readValues[dir] - micAvg);

      Serial.print(String(readValues[dir]));
      Serial.print(",");
    }
    Serial.println("");

    int sourceEvaluator = -1; // Default value

    // Selecting the Source Microphone
    for(int dir = 0; dir < MICROPHONES; dir++){
      if(readValues[dir] > sourceEvaluator){
        sourceEvaluator = readValues[dir];
        if(sourceEvaluator > threshold){
          sourceDir = dir;
          sourceAmp = sourceEvaluator;
        }
        
      }
    }
    Serial.print("Source: ");
    Serial.println(directions[sourceDir]);
    Serial.print("Amplitude: ");
    Serial.println(sourceAmp);

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

    messageTXT = String(directions[sourceDir]);
    messageTXT.concat("$");
    messageTXT.concat(String(sourceAvg));
    webSocket.broadcastTXT(messageTXT);
}
