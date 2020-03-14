#include "dht.h"
#include "LiquidCrystal_I2C.h"

dht DHT;
LiquidCrystal_I2C lcd(0x3F, 16, 2);

#define DHT11_PIN 7

void setup(){
  lcd.begin();
  lcd.backlight();
}

void loop()
{
  int chk = DHT.read11(DHT11_PIN);
  lcd.setCursor(0,0); 
  lcd.print("Temp: ");
  lcd.print(DHT.temperature);
  lcd.print((char)223);
  lcd.print("C");
  lcd.setCursor(0,1);
  lcd.print("Humidity: ");
  lcd.print(DHT.humidity);
  lcd.print("%");
  delay(1000);
}

