#include <SPI.h>
#include <Adafruit_GFX.h>
#include <Adafruit_ILI9341.h>
#include <ESP8266WiFi.h>
#include <PubSubClient.h>
#include <ArduinoJson.h>
#include <Servo.h>

#ifdef ESP8266
   #define STMPE_CS 16
   #define TFT_CS   0
   #define TFT_DC   15
   #define SD_CS    2
#endif

#define b_next 4
#define b_prev 5

#include "arduino_secrets.h" 

const char* ssid     = SECRET_SSID;
const char* password = SECRET_PASS;
const char* mqttuser = SECRET_MQTTUSER;
const char* mqttpass = SECRET_MQTTPASS;

const char* mqtt_server = "mqtt.cetools.org";
const char* topic_GF = "UCL/OPSEBO/004/Room/CDS/Value";
const char* topic_1F = "UCL/OPSEBO/107/Room/CDS/Value";
const char* topic_2F = "UCL/OPSEBO/201/NextToDoorTo206/CDS/Value";
const char* topic_3F = "UCL/OPSEBO/313/Room/CDS/Value";
const char *topics[4] = {topic_GF, topic_1F, topic_2F, topic_3F};
const char *titles[4] = {"004", "107", "201", "313"};
WiFiClient espClient;
PubSubClient client(espClient);
long lastMsg = 0;
int values[4] = {0, 0, 0, 0};
int angle = 0;
int count_tft = 0;
int topic_count = 0;
boolean isPressed = false;
boolean initial = true;

Adafruit_ILI9341 tft = Adafruit_ILI9341(TFT_CS, TFT_DC);
Servo myservo; 

void setup() {
  Serial.begin(115200);
  pinMode(b_next, INPUT);
  pinMode(b_prev, INPUT);
  delay(10);
  myservo.attach(2, 500, 1750);
  tft.begin();
  tft.setRotation(1);
  tft.fillScreen(ILI9341_BLACK);
  tft.setCursor(70, 110);
  tft.setTextColor(ILI9341_GREEN);    
  tft.setTextSize(3);
  tft.println("Connecting");

  startWifi();
  tft.fillScreen(ILI9341_BLACK);
  tft.setCursor(60, 0);
  tft.setTextColor(ILI9341_RED);    
  tft.setTextSize(3);
  tft.println("Current Room:");
  
  tft.setTextColor(ILI9341_GREEN);    
  tft.setTextSize(2);
  tft.setCursor(0, 120);
  tft.println("precision value:");
  tft.setCursor(250, 122);
  tft.setTextColor(ILI9341_WHITE);    
  tft.setTextSize(2);
  tft.println("ppm");

  client.setServer(mqtt_server, 1884);
  client.setCallback(callback);
  }

void loop() {
  // put your main code here, to run repeatedly:
  if (!client.connected()) {
    //TODO: add a connecting display
    reconnect();
  }
  client.loop();

  //TODO: add button actions
  int next = digitalRead(b_next);
  int prev = digitalRead(b_prev);

  if(next == HIGH){
    topic_count += 1;
    topic_count = topic_count % 4;
    isPressed = true;
  }
  if(prev == HIGH){
    topic_count -= 1;
    if (topic_count < 0){
      topic_count = 3;
    }
    isPressed = true;
  }

  if(initial){
    tft.setCursor(90, 50);
    tft.setTextColor(ILI9341_WHITE);
    tft.setTextSize(3);
    tft.print("Room ");
    tft.println(titles[topic_count]);
    showText(topic_count);
    initial = false;
  }
  
  while(isPressed){
    tft.setCursor(90, 50);
    tft.setTextColor(ILI9341_WHITE);
    tft.setTextSize(3);
    tft.print("Room ");
    tft.fillRect(180, 50, 300, 40, ILI9341_BLACK); 
    tft.println(titles[topic_count]);
    showText(topic_count);
    servoAction(values[topic_count]);
    isPressed = false;
  } 
  servoAction(values[topic_count]);
  tft.setRotation(1);
  if(count_tft == 100){
    showText(topic_count);
    count_tft = 0;
  }
  delay(100);
  count_tft += 1;
}

void showText(int topic_count) {
  tft.fillRect(200, 122, 45, 30, ILI9341_BLACK); 
  tft.setCursor(200, 122);
  tft.setTextColor(ILI9341_WHITE);
  tft.setTextSize(2);
  tft.println(values[topic_count]);
}

void startWifi() {
  // We start by connecting to a WiFi network
  Serial.println();
  Serial.print("Connecting to ");
  Serial.println(ssid);
  WiFi.begin(ssid, password);

  // check to see if connected and wait until you are
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("");
  Serial.println("WiFi connected");
  Serial.print("IP address: ");
  Serial.println(WiFi.localIP()); 
}

void reconnect() {
  // Loop until we're reconnected
  while (!client.connected()) {
    Serial.print("Attempting MQTT connection...");
    // Create a random client ID
    String clientId = "ESP-yikunli";
    clientId += String(random(0xffff), HEX);
    
    // Attempt to connect with clientID, username and password
    if (client.connect(clientId.c_str(), mqttuser, mqttpass)) {
      client.subscribe(topics[0]);
      client.subscribe(topics[1]);
      client.subscribe(topics[2]);
      client.subscribe(topics[3]);
      Serial.println("connected");
    } else {
      Serial.print("failed, rc=");
      Serial.print(client.state());
      Serial.println(" try again in 5 seconds");
      // Wait 5 seconds before retrying
      delay(5000);
    }
  }
}

void callback(char* topic, byte* payload, unsigned int length) {
  char msg[1024];
  memcpy(msg, payload, length);
  msg[length] = '\0';

  StaticJsonDocument<1024> doc;
  DeserializationError error = deserializeJson(doc, msg);

  if(strcmp(topic, topics[0]) == 0){
    Serial.println("From topic 0");
    values[0] = doc["/UCL Virtual ES/IOT-mqtt/1PS/004/Room/CDS/Value"];
  }
  if(strcmp(topic, topics[1]) == 0){
    Serial.println("From topic 1");
    values[1] = doc["/UCL Virtual ES/IOT-mqtt/1PS/107/Room/CDS/Value"];
  }
  if(strcmp(topic, topics[2]) == 0){
    Serial.println("From topic 2");
    values[2] = doc["/UCL Virtual ES/IOT-mqtt/1PS/201/NextToDoorTo206/CDS/Value"];
  }
  if(strcmp(topic, topics[3]) == 0){
    Serial.println("From topic 3");
    values[3] = doc["/UCL Virtual ES/IOT-mqtt/1PS/313/Room/CDS/Value"];
  }

  Serial.print("Message arrived [");
  Serial.print(topic);
  Serial.print("] ");
  for (int i = 0; i < length; i++) {
    Serial.print((char)payload[i]);
  }
}

void servoAction(int value){
  if(value < 400 && value >= 300){
    int angle_temp = (value - 300) / 100 * 30;
    if(angle > angle_temp){
      for(angle; angle > angle_temp; angle -= 1){
        myservo.write(angle);
        delay(15);
      }
    }else if(angle < angle_temp){
      for(angle; angle < angle_temp; angle += 1){
        myservo.write(angle);
        delay(15);
      }
    }
  }else if(value >= 400 && value <= 600){
    int angle_temp = (value - 400) / 200 * 120 + 30;
    if(angle > angle_temp){
      for(angle; angle > angle_temp; angle -= 1){
        myservo.write(angle);
        delay(15);
      }
    }else if(angle < angle_temp){
      for(angle; angle < angle_temp; angle += 1){
        myservo.write(angle);
        delay(15);
      }
    }
  }else if(value > 600 && value < 2000){
    int angle_temp = (value - 600) / 1400 * 30 + 150;
    if(angle > angle_temp){
      for(angle; angle > angle_temp; angle -= 1){
        myservo.write(angle);
        delay(15);
      }
    }else if(angle < angle_temp){
      for(angle; angle < angle_temp; angle += 1){
        myservo.write(angle);
        delay(15);
      }
    }
  }
}
