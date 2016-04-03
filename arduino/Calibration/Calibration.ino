#include <Wire.h>


// defines for setting and clearing register bits
#ifndef cbi
#define cbi(sfr, bit) (_SFR_BYTE(sfr) &= ~_BV(bit))
#endif
#ifndef sbi
#define sbi(sfr, bit) (_SFR_BYTE(sfr) |= _BV(bit))
#endif


// RGB pin assigment
#define redPin    4
#define greenPin  6
#define bluePin   5

// scan pin assignment
byte scan_pin[8] = { 8,1,0,2,3,5,4,9 };


void setup()
{
  // set prescale to 16  (to speed up the analogRead function)
  cbi(ADCSRA,ADPS2) ;
  sbi(ADCSRA,ADPS1) ;
  cbi(ADCSRA,ADPS0) ;
    
  // set mode and initial values for RGB controll pins
  pinMode(redPin, OUTPUT);          
  pinMode(greenPin, OUTPUT);     
  pinMode(bluePin, OUTPUT);     
  digitalWrite(redPin, HIGH); 
  digitalWrite(greenPin, HIGH); 
  digitalWrite(bluePin, HIGH); 
    
  // start serial for output
  Serial.begin(9600);  
}

int rd(int i)
{
  int p = scan_pin[i];
  analogRead(p);
  return (analogRead(p) + analogRead(p) + analogRead(p)) >> 2;
}


void loop()
{
  int i;
  byte rgb[8][3];
  
  while (true)
  {
    long t0= micros();
    // take readings from all sensors
    for (i=0; i<8; i++)
    {  
      // take red reading
      digitalWrite(redPin, LOW);
      unsigned int r = rd(i);
      digitalWrite(redPin, HIGH);

      // ambient readings 
      unsigned int ambient = rd(i);

      // take green reading
      digitalWrite(greenPin, LOW);
      unsigned int g = rd(i);
      digitalWrite(greenPin, HIGH);

      // take blue reading
      digitalWrite(bluePin, LOW);
      unsigned int b = rd(i);
      digitalWrite(bluePin, HIGH);

      if (r>ambient+255) {
        rgb[i][0] = 255;
      } else if (r>ambient) {
        rgb[i][0] = (r - ambient);
      } else {
        rgb[i][0] = 0;
      }
      if (g>ambient+255) {
        rgb[i][1] = 0;
      } else if (g>ambient) {
        rgb[i][1] = (g - ambient);
      } else {
        rgb[i][1] = 0;
      }
      if (b>ambient+255) {
        rgb[i][2] = 255;        
      } else if (b>ambient) {
        rgb[i][2] = (b - ambient);
      } else {
        rgb[i][2] = 0;
      }
    }
    long t1= micros();
    
    // write readings to console 
    for (i=0; i<8; i++)
    {
      Serial.print(i);
      Serial.print(":");
      Serial.print(rgb[i][0]);
      Serial.print(",");
      Serial.print(rgb[i][1]);
      Serial.print(",");
      Serial.print(rgb[i][2]);
      Serial.print("  ");
    }
    Serial.print(" time:");
    Serial.print(t1-t0);
    Serial.print("\n");
  
    // do only one reading per second
    delay(1000);  
  }
}


