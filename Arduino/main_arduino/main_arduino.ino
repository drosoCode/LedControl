 #include <Adafruit_NeoPixel.h>
 #include <SoftwareSerial.h>

#define NB_LEDS 20

Adafruit_NeoPixel strips[6] = {
  Adafruit_NeoPixel(78, 3, NEO_GRB + NEO_KHZ800),
  Adafruit_NeoPixel(NB_LEDS, 5, NEO_GRB + NEO_KHZ800),
  Adafruit_NeoPixel(NB_LEDS, 6, NEO_GRB + NEO_KHZ800),
  Adafruit_NeoPixel(NB_LEDS, 9, NEO_GRB + NEO_KHZ800),
  Adafruit_NeoPixel(NB_LEDS, 10, NEO_GRB + NEO_KHZ800),
  Adafruit_NeoPixel(NB_LEDS, 11, NEO_GRB + NEO_KHZ800)
};
SoftwareSerial arduinoComm(7, 8);

char serialBuffer[100];
char serialIndex = 0;

void setup() {
  Serial.begin(9600);
  arduinoComm.begin(9600);
  for (int i = 0; i<6; i++) {
    strips[i].begin();
    strips[i].setBrightness(255);
  }
}

void loop() {
  while (Serial.available())
  {
    char c = Serial.read();
    serialBuffer[serialIndex] = c;
    serialIndex++;
    if(c == '#')
    {
      //end of command
      serialBuffer[serialIndex] = '\0';
      executeCommand();
      memset(serialBuffer, 0, sizeof(serialBuffer));
      serialIndex = 0;
    }
  }
}


uint32_t getColor(char *color) {
  char red[3];
  memcpy(red, color /* Offset */, 2 /* Length */);
  red[2] = 0;
  char green[3];
  memcpy(green, color + 2 /* Offset */, 2 /* Length */);
  green[2] = 0;
  char blue[3];
  memcpy(blue, color + 4 /* Offset */, 2 /* Length */);
  blue[2] = 0;

  return strips[0].Color(
    (int) strtol(red, NULL, 16),
    (int) strtol(green, NULL, 16),
    (int) strtol(blue, NULL, 16)
   );
}

// -1;0:4;00ff00;255#-1;4:7;ffff00;255#-1;7:8;ff0000;255#-1;!#
// -1;0:20;ff0000;255#-1;!#
// -1;0:20;00ffff;255#-1;3;ff0000;255#-1;!#
// 2;0:20;abcdef;255#
// device;leds;color;brightness#
// 2;!# to display

void executeCommand() {
  char dat_device[3];
  char dat_leds[6];
  char dat_color[7];
  char dat_brightness[4];
  bool showStrip = false;

  int fillCount = 0;
  int fillLen = 0;
  for(int i = 0; i < strlen(serialBuffer); i++) {
    if (serialBuffer[i] == ';') {
      if (fillCount == 0) {
        dat_device[fillLen] = '\0';
      } else if (fillCount == 1) {
        dat_leds[fillLen] = '\0';
      } else if (fillCount == 2) {
        dat_color[fillLen] = '\0';
      }
      fillCount++;
      fillLen = 0;
    }
    else if (serialBuffer[i] == '#') {
      dat_brightness[fillLen] = '\0';
      break;
    }
    else {
      if (fillCount == 0) {
        dat_device[fillLen] = serialBuffer[i];
      } else if (fillCount == 1) {
        if (fillLen == 0 && serialBuffer[i] == '!') {
          showStrip = true;
          break;
        } else {
          dat_leds[fillLen] = serialBuffer[i];
        }
      } else if (fillCount == 2) {
        dat_color[fillLen] = serialBuffer[i];
      } else if (fillCount == 3) {
        dat_brightness[fillLen] = serialBuffer[i];
      }
      fillLen++;
    }
  }  
  int device = atoi(dat_device);
    
  if (device > 5 || device == -1) {
    arduinoComm.print(serialBuffer);
  }
  if (device <= 5) {
    if (showStrip) {
      if (device == -1) {
        for (int i = 0; i<6; i++) {
          strips[i].show();
        }
      }
      else {
        strips[device].show();
      }
    }
    else {
      int start = NULL;
      int count = NULL;
      if (strstr(dat_leds, ":") != NULL) {
        start = atoi(strtok(dat_leds, ":"));
        count = atoi(strtok(NULL, ":")) - start;
      }
      uint32_t color = getColor(dat_color);
      int brightness = atoi(dat_brightness);
  
      if (device == -1) {
        for (int i = 0; i<6; i++) {
          if (count != NULL) {
             strips[i].fill(color, start, count);
          }
          else {
            strips[i].setPixelColor(atoi(dat_leds), color);
          }
          strips[i].setBrightness(brightness);
        }
      }
      else {
          if (count != NULL) {
             strips[device].fill(color, start, count);
          } 
          else {
            strips[device].setPixelColor(atoi(dat_leds), color);
          }
          strips[device].setBrightness(brightness);
      }
    }
  }
}
