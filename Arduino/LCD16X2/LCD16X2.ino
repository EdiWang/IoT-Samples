#include <LiquidCrystal.h> //Import the LCD library
//Include^
//Init
LiquidCrystal lcd(12, 11, 5, 4, 3, 2); /*Initialize the LCD and
                                        tell it which pins is
                                        to be used for communicating*/

//Global Var
#define contra 9 //Define the pin that controls the contrast of the screen
#define bri 10 //Define the pin the controls the brightness of the screen
//Both pins are PWM so you can analogWrite to them and have them output a variable value


void setup() {//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
  // put your setup code here, to run once:
  lcd.begin(16, 2); //Tell the LCD that it is a 16x2 LCD
  pinMode(contra, OUTPUT); //set pin 9 to OUTPUT
  pinMode(bri, OUTPUT); //Set pin 10 to OUTPUT
  //pinMode-ing OUTPUT makes the specified pin output power
  digitalWrite(contra, LOW); /*outputs no power to the contrast pin.
                            this lets you see the words*/
  analogWrite(bri, 255); //Outputs full power to the screen brightness LED
}//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

void loop() {//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
  // put your main code here, to run repeatedly:
  lcd.print(" Hello, World!!"); //Output " Hello, World!!" on the first line of the LCD
  lcd.setCursor(0, 1); /*Set the (invisible) cursor on the first place second row of the LCD.
                        Cursor values are 0-indexed, 0 would the be the first place.
                        The cursor coordinates are X,Y coordinates.*/
  lcd.print("123456!!!");
  delay(1000); //Wait a second
  for (int l = 0; l < 16; l++) { //For loop. Repeating 16 times
    lcd.scrollDisplayRight(); //Scroll whole screen to the right once
    delay(90); //Slight delay for animation
  }
  delay(1000);
  for (int l = 0; l < 16; l++)
    lcd.scrollDisplayLeft();
  for (int l = 51; l > -1; l--) { //Repeating 51 times
    analogWrite(bri, l * 5);
    delay(35);
  }
  delay(1000);
  digitalWrite(bri, HIGH);
  for (int l = 0; l < 51; l++) { //Repeating 51 times
    analogWrite(contra, l * 5);
    delay(35);
  }
  delay(1000);
  digitalWrite(contra,LOW);
  lcd.setCursor(0, 0);
}//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
