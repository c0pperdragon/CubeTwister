#include <Wire.h>


// defines for setting and clearing register bits
#ifndef cbi
#define cbi(sfr, bit) (_SFR_BYTE(sfr) &= ~_BV(bit))
#endif
#ifndef sbi
#define sbi(sfr, bit) (_SFR_BYTE(sfr) |= _BV(bit))
#endif

// scan pin assignment
byte scan_pin[8] = { 8,1,0,2,3,5,4,9 };


void setup()
{
  // set prescale to 16  (to speed up the analogRead function)
  sbi(ADCSRA,ADPS2) ;
  cbi(ADCSRA,ADPS1) ;
  cbi(ADCSRA,ADPS0) ;
      
  // start serial for output
  Serial.begin(9600);  
}

void loop()
{
  int raw[8];
  int i;
  
  while (true)
  {
    // take ambient reading from all sensors
    delayMicroseconds(50);
    for (i=0; i<8; i++)
    {  
      raw[i] = analogRead(scan_pin[i]);
    }
       
    // write reading to console 
    for (i=0; i<8; i++)
    {
      int x = raw[i];

      Serial.print(i);
      Serial.print(":");
      Serial.print(raw[i]);
      Serial.print(" ");
    }
    Serial.print("\n");
  
    // do only one reading per second
    delay(1000);  
  }
}


