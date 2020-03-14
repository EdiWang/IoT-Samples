#include <Wire.h>
#include "LiquidCrystal_I2C.h"

LiquidCrystal_I2C lcd(0x3F, 16, 2);
int BH1750_address = 0x23;
byte buff[2];

void setup()
{
    Wire.begin();
    lcd.begin();
    lcd.backlight();
    BH1750_Init(BH1750_address);
}

void loop()
{
    float valf = 0;
    if (BH1750_Read(BH1750_address) == 2)
    {
        lcd.setCursor(0, 0);
        valf = ((buff[0] << 8) | buff[1]) / 1.2;
        if (valf < 0)
        {
            lcd.print("> 65535");
        }
        else
        {
            lcd.print((int)valf, DEC);
        }
        lcd.print(" LX");
    }
    delay(500);
    lcd.clear();
}

void BH1750_Init(int address)
{
    Wire.beginTransmission(address);
    Wire.write(0x10);
    Wire.endTransmission();
}

byte BH1750_Read(int address)
{

    byte i = 0;
    Wire.beginTransmission(address);
    Wire.requestFrom(address, 2);
    while (Wire.available())
    {
        buff[i] = Wire.read();
        i++;
    }
    Wire.endTransmission();
    return i;
}