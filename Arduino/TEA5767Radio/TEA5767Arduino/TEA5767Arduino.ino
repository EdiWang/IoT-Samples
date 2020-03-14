#include "TEA5767.h"
#include <LiquidCrystal.h> 
#include <Wire.h>
#include "EEPROMex.h"

//Constants:
TEA5767 Radio; //Pinout SLC and SDA - Arduino uno pins A5 and A4
LiquidCrystal lcd(12, 11, 5, 4, 3, 2);

//Variables:
double old_frequency;
double frequency = 103.7;
int search_mode = 0;
int search_direction;
unsigned long last_pressed;
unsigned char buf[5];
int stereo;
int signal_level;
double current_freq;
unsigned long current_millis = millis();
int fUP = 7;
int fDOWN = 6;
int count = 0;
int flag = 0;

byte sig1[8] = {
  B00000,
  B00001,
  B00001,
  B00101,
  B00101,
  B10101,
  B10101,
};

byte sig2[8] = {
  B11111,
  B00000,
  B01111,
  B00000,
  B00111,
  B00000,
  B00011,
};

void setup () {
  //Init
  Serial.begin(9600);
  current_freq = 87.9;
  current_freq = EEPROM.readFloat(0);
  
  lcd.createChar(0, sig1);
  lcd.createChar(1, sig2);
  lcd.begin(16,2);

  lcd.clear();
  lcd.setCursor(0,0);
  lcd.print("Arduino FM Radio");
  lcd.setCursor(0,1);
  lcd.print("by Edi Wang");
  

  delay(2500);

  for (int l = 0; l < 16; l++) {
    lcd.scrollDisplayRight();
    delay(90);
  }
  
  lcd.clear();
  
  Radio.init();
  Radio.set_frequency(current_freq);
}

void loop () {
  if (Radio.read_status(buf) == 1) {
     current_freq = floor (Radio.frequency_available (buf) / 100000 + .5) / 10;
     stereo = Radio.stereo(buf);
     signal_level = Radio.signal_level(buf);

     if (flag == 0) {
       lcd.setCursor(0,0);
       lcd.print("PLAYING FM  ");
       EEPROM.updateFloat(0, current_freq);
       lcd.setCursor(11,0);
       lcd.print(current_freq);
     }
     
     lcd.setCursor(0,1);
     if (stereo){
         lcd.print("Stereo"); 
     } 
     else{
         lcd.print("Mono  ");
     } 

     lcd.setCursor(10,1);
     lcd.write(byte(1));

     if(signal_level < 10) {
        lcd.setCursor(12,1);
        lcd.print(signal_level);
     }
     else{
        lcd.setCursor(11,1);
        lcd.print(signal_level);
     }
     lcd.print("/15");
  
     flag = 1;
  }
   
  // When button pressed, search for new station
  if (search_mode == 1) {
      if (Radio.process_search (buf, search_direction) == 1) {
          search_mode = 0;
      }
  }

  if (digitalRead(fUP)) {
    count++;
    if(count > 0 && count <= 6) {
      lcd.clear();
      lcd.print("Searching Up..."); 

      last_pressed = current_millis;
      search_mode = 1;
      search_direction = TEA5767_SEARCH_DIR_UP;
      Radio.search_up(buf);
    }
    flag = 0;
    count = 0;
  }
  
  if (digitalRead(fDOWN)) {
    count++;
    if(count > 0 && count <= 6) {
      lcd.clear();
      lcd.print("Searching Down..."); 

      last_pressed = current_millis;
      search_mode = 1;
      search_direction = TEA5767_SEARCH_DIR_DOWN;
      Radio.search_down(buf);
    }
    flag = 0;
    count = 0;
  } 
  delay(500);
}
