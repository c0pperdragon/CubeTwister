// Control for the stopwatch cirtuit.
// Uses an Arduino Leonardo

#include <Wire.h>

#define SLAVE_ADDRESS    0x04
#define COMMAND_START    0x80
#define COMMAND_STOP     0x81
#define COMMAND_RESET    0x82


long int counter;      // value of counter in stopped mode (or time base in started mode)
bool isstarted;        // true, when the timer is running
long int starttime;    // system timestamp of counter start (the value of "counter" was frozen at this moment)


void setup() {
  // using digital pin 0,1,4-7 and analog pin 0 - 5 as output
  int p;
  for (p=0; p<2; p++)
  {  
    pinMode(p, OUTPUT);
    digitalWrite(p, LOW); 
  }  
  for (p=4; p<8; p++)
  {  
    pinMode(p, OUTPUT);
    digitalWrite(p, LOW); 
  }  
  for (p=0; p<6; p++)
  {  
    pinMode(A0+p, OUTPUT);
    digitalWrite(A0+p, LOW); 
  }
  
  counter = 0;
  isstarted = false;
  
  Wire.begin(SLAVE_ADDRESS);
  Wire.onReceive(receiveData);
  
}

void loop() 
{  
  if (isstarted)
  {
     long v = counter + (millis() - starttime);
     if (v>9999)
     {  counter = 9999;
        isstarted = false;
        showTime(counter);
     }
     else {
       showTime(v);
     }
  }
  else
  {
     showTime(counter);
  }
}

byte digitpatterns[] = { 
  0b01111110,   // "0"
  0b01001000,   // "1"
  0b00111101,   // "2"
  0b01101101,   // "3"
  0b01001011,   // "4"
  0b01100111,   // "5"
  0b01110111,   // "6"
  0b01001100,   // "7"
  0b01111111,   // "8"
  0b01101111,   // "9"  
};

void showTime(int value)
{
  int pos;
  for (pos=0; pos<4; pos++)
  {
      int tenth = value / 10;
      int digit = value-tenth*10;
      showPattern(pos, digitpatterns[digit] | (pos==3 ? 0b10000000 : 0));      
      value = tenth;
  }  
}

void showPattern(int pos, int pattern)
{
  byte segment;
  for (segment=0; segment<8; segment++)
  {
    byte x = (pattern&(1<<segment))==0 ? LOW : HIGH;
    if (segment<2) digitalWrite(segment, x);
    else if (segment<4) digitalWrite(A4 + (3-segment), x);
    else digitalWrite(A0+(segment-4), x);    
//    digitalWrite(segment<4 ? segment : A0+(segment-4),  (pattern&(1<<segment))==0 ? LOW : HIGH);
  }
  digitalWrite(4+pos, HIGH);
  delay(1);
  digitalWrite(4+pos, LOW);  
}



// callback to process commands from EV3
void receiveData(int byteCount)
{
  byte cmd = Wire.read();   
  switch (cmd)
  {
    case COMMAND_START:
      if (!isstarted)
      { starttime = millis();
        isstarted = true;
      }
      break;
    case COMMAND_STOP:
      if (isstarted)
      { counter = counter + (millis() - starttime) - 11;   // compensate I2C lag
        isstarted = false;
      }
      break;
    case COMMAND_RESET:
      counter = 0;
      isstarted = false;
      break;
  }
}


