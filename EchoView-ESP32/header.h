#include <WiFi.h>

const char* ssid = "dsv-extrality-lab";
const char* password = "";

WebSocketsServer webSocket = WebSocketsServer(81);
String JSONtxt, oldJSON;

//=====================================================
//function process event: new data received from client
//=====================================================
void webSocketEvent(uint8_t num, WStype_t type, uint8_t *payload, size_t welength)
{
    String payloadString = (const char *) payload;
    Serial.print("payloadString= ");
    Serial.println(payloadString);

    if(type == WStype_TEXT) //receive text from client
    {
        byte separator=payloadString.indexOf('=');
        String var = payloadString.substring(0,separator);
        Serial.print("var= ");
        Serial.println(var);
        String val = payloadString.substring(separator+1);
        Serial.print("val= ");
        Serial.println(val);
        Serial.println(" ");
    }
}

void connect2Network() 
{
    WiFi.begin(ssid, password);
    Serial.println("Printing this device's details bellow...");
    Serial.print("MacAddress: ");
    Serial.println(WiFi.macAddress());
    Serial.print("Device Name: ");
    Serial.println(WiFi.getHostname());
    Serial.print("Attempting to connect to " + String(ssid) + " network...");

    while(WiFi.status() != WL_CONNECTED)
    {
        Serial.print(".");
        delay(500);
    }

    WiFi.mode(WIFI_STA);
    Serial.println();
    Serial.println("Successfully connected to network!");
    Serial.print("Local IP: ");
    Serial.println(WiFi.localIP());
    webSocket.begin();
    webSocket.onEvent(webSocketEvent);
}

void sendMessage2Clients()
{
    if(oldJSON != JSONtxt)
        webSocket.broadcastTXT(JSONtxt);
    oldJSON = JSONtxt;
}