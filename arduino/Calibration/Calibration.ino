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

// rgb calibration
unsigned int calibration[8][3] = {
  { 125, 114, 103 },      // sensor 0
  { 110, 107, 80 },      // sensor 1
  { 113, 91, 90 },      // sensor 2
  { 124, 144, 122 },     // sensor 3
  { 143, 114, 117 },      // sensor 4
  { 119, 112, 87 },      // sensor 5
  { 120, 106, 100 },      // sensor 6
  { 112, 95, 75 }      // sensor 7
};
unsigned int calibration_offset = -20;

void setup()
{
  // set prescale to 16  (to speed up the analogRead function)
  sbi(ADCSRA,ADPS2) ;
  cbi(ADCSRA,ADPS1) ;
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

void loop()
{
  int raw[8][4];
  int i;
  
  while (true)
  {
    // take ambient reading from all sensors
    delayMicroseconds(50);
    for (i=0; i<8; i++)
    {  
      raw[i][3] = analogRead(scan_pin[i]);
    }

    // take red readings from all sensors
    digitalWrite(redPin, LOW);
    delayMicroseconds(50);
    for (i=0; i<8; i++)
    {  
      raw[i][0] = analogRead(scan_pin[i]);
    }
    digitalWrite(redPin, HIGH);

    // take green readings from all sensors
    digitalWrite(greenPin, LOW);
    delayMicroseconds(50);
    for (i=0; i<8; i++)
    {  
      raw[i][1] = analogRead(scan_pin[i]);
    }
    digitalWrite(greenPin, HIGH);

    // take blue readings from all sensors
    digitalWrite(bluePin, LOW);
    delayMicroseconds(50);
    for (int i=0; i<8; i++)
    {  
      raw[i][2] = analogRead(scan_pin[i]);
    }
    digitalWrite(bluePin, HIGH);
       
    // write reading to console 
    for (i=0; i<8; i++)
    {
      int x = raw[i][3];
      unsigned int r = max(raw[i][0] - x, 0);
      unsigned int g = max(raw[i][1] - x, 0);
      unsigned int b = max(raw[i][2] - x, 0);            
            
      Serial.print(i);
      Serial.print(":");
      Serial.print((byte) (((r*calibration[i][0]) >> 8) + calibration_offset));
      Serial.print(",");
      Serial.print((byte) (((g*calibration[i][1]) >> 8) + calibration_offset));
      Serial.print(",");
      Serial.print((byte) (((b*calibration[i][2]) >> 8) + calibration_offset));
      Serial.print(" ");
    }
    Serial.print("\n");
  
    // do only one reading per second
    delay(1000);  
  }
}


