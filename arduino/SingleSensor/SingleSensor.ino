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

void setup()
{
  // set prescale to 16  (to speed up the analogRead function)
//  sbi(ADCSRA,ADPS2) ;
//  cbi(ADCSRA,ADPS1) ;
//  cbi(ADCSRA,ADPS0) ;
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

#define SAMPLES 5

int rd()
{
  return (analogRead(0) + analogRead(0) + analogRead(0) + analogRead(0)) >> 2;
//  return (analogRead(0) + analogRead(0) ) >> 1;
}

void loop()
{
  int raw[SAMPLES];
  int i;
  long t0;
  long t1;
  
  while (true)
  {
    t0= micros();

    raw[0] = rd();

    digitalWrite(redPin,LOW);      
    raw[1] = rd();
    digitalWrite(redPin,HIGH);      

    digitalWrite(greenPin,LOW);      
    raw[2] = rd();
    digitalWrite(greenPin,HIGH);      

    digitalWrite(bluePin,LOW);      
    raw[3] = rd();
    digitalWrite(bluePin,HIGH);      

    raw[4] = rd();
          
    t1= micros();

    Serial.print(t1-t0);
    Serial.print("us ");
    // write reading to console 
    for (i=0; i<SAMPLES; i++)
    {
      Serial.print(raw[i]);
      Serial.print(" ");
    }
    Serial.print("\n");
  
    delay(1000);  
  }
}


