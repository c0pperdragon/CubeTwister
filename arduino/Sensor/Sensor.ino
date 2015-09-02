#include <Wire.h>


// I2C protocol constants
#define SLAVE_ADDRESS       0x04
#define COMMAND_STARTSCAN0  0x80
#define COMMAND_STARTSCAN1  0x81
#define COMMAND_STARTSCAN2  0x82
#define COMMAND_GETSAMPLE   0x83

// maximum number of samples that can be taken
#define BUFFERSAMPLES 60

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

// reference colors
byte reference[6][3] = {  
  {  170, 170, 150  },    // white
  {  170, 57,  45   },    // red  
  {  52,  150, 53   },    // green
  {  50,  63,  75   },    // blue
  {  185, 175, 67   },    // yellow
  {  180, 80,  50   }     // orange
};

// buffer to store samples and detected colors
byte samples[BUFFERSAMPLES][8*3];
byte colors[20];

// communicate data from wire callback to main program
volatile int get_sample_number;
volatile int scan_mode;

void setup()
{
  Serial.begin(9600);
  
  // set prescale to 16  (to speed up the analogRead function)
  _SFR_BYTE(ADCSRA) |= _BV(ADPS2);
  _SFR_BYTE(ADCSRA) &= ~_BV(ADPS1);
  _SFR_BYTE(ADCSRA) &= ~_BV(ADPS0);
  
  // set mode and initial values for RGB control pins
  pinMode(redPin, OUTPUT);          
  pinMode(greenPin, OUTPUT);     
  pinMode(bluePin, OUTPUT);     
  digitalWrite(redPin, HIGH); 
  digitalWrite(greenPin, HIGH); 
  digitalWrite(bluePin, HIGH); 
  
  // communication with EV3
  get_sample_number = -1;
  scan_mode = -1;
  Wire.begin(SLAVE_ADDRESS);
  Wire.onReceive(receiveData);
  Wire.onRequest(sendData);
}

void loop()
{
  int numsamples;
  long start;
  
  // do program action dependend on received wire command
  switch (scan_mode)
  {
    case 0:      // scan when moving motor B&D (fast)
        scan_mode = -1;
        delay(6);
        start = millis();
        for (numsamples=0; numsamples<50; numsamples++)
        {    while (millis() < start+1+numsamples);
             take_sample(samples[numsamples]);
        }
        colors[0] = extract_color(0,0);
        colors[1] = extract_color(0,32);
        colors[2] = extract_color(0,49);
        colors[3] = extract_color(1,0);
        colors[4] = extract_color(1,40);
        colors[5] = extract_color(2,0);
        colors[6] = extract_color(2,40);
        colors[7] = extract_color(3,0);
        colors[8] = extract_color(3,32);
        colors[9] = extract_color(3,47);
        colors[10] = extract_color(4,0);
        colors[11] = extract_color(4,33);
        colors[12] = extract_color(4,46);
        colors[13] = extract_color(5,0);
        colors[14] = extract_color(5,40);
        colors[15] = extract_color(6,0);
        colors[16] = extract_color(6,40);
        colors[17] = extract_color(7,0);
        colors[18] = extract_color(7,33);    
        colors[19] = extract_color(7,46);    
        break;            
    case 1:      // scan when moving motor A & C (slow)
        scan_mode = -1;
        delay(16);
        start = millis();
        for (numsamples=0; numsamples<60; numsamples++)
        {    while (millis() < start+1+numsamples);
             take_sample(samples[numsamples]);
        }
        colors[0] = extract_color(0,0);
        colors[1] = extract_color(0,41);
        colors[2] = extract_color(1,0);
        colors[3] = extract_color(1,33);
        colors[4] = extract_color(1,55);
        colors[5] = extract_color(2,0);
        colors[6] = extract_color(2,33);
        colors[7] = extract_color(2,55);      
        colors[8] = extract_color(3,0);
        colors[9] = extract_color(3,41);
        colors[10] = extract_color(4,0);
        colors[11] = extract_color(4,41);
        colors[12] = extract_color(5,0);
        colors[13] = extract_color(5,36);
        colors[14] = extract_color(5,58);
        colors[15] = extract_color(6,0);
        colors[16] = extract_color(6,33);
        colors[17] = extract_color(6,55);
        colors[18] = extract_color(7,0);
        colors[19] = extract_color(7,41);    
        break;                
    case 2:
        scan_mode = -1;
        take_sample(samples[0]);
        colors[0] = extract_color(0,0);
        colors[1] = extract_color(1,0);
        colors[2] = extract_color(2,0);
        colors[3] = extract_color(3,0);      
        colors[4] = extract_color(4,0);
        colors[5] = extract_color(5,0);
        colors[6] = extract_color(6,0);
        colors[7] = extract_color(7,0);
        break;        
  }  
}

void take_sample(byte* s)
{  
  int raw[8][4];
  int i;
  
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
       
  for (i=0; i<8; i++)
  {
    int x = raw[i][3];
    unsigned int r = max(raw[i][0] - x, 0);
    unsigned int g = max(raw[i][1] - x, 0);
    unsigned int b = max(raw[i][2] - x, 0);            
    
    s[3*i]   = (byte) (((r*calibration[i][0]) >> 8) + calibration_offset);
    s[3*i+1] = (byte) (((g*calibration[i][1]) >> 8) + calibration_offset);
    s[3*i+2] = (byte) (((b*calibration[i][2]) >> 8) + calibration_offset);
  }      
}

byte extract_color(int sensor, int sample)
{
  long bestdistance = 10000000;
  byte bestcolor = 0;

  int r = samples[sample][sensor*3];  
  int g = samples[sample][sensor*3+1];  
  int b = samples[sample][sensor*3+2];  
  int c;
  for (c=0; c<6; c++)
  {
      int dr = r-reference[c][0];
      int dg = g-reference[c][1];
      int db = b-reference[c][2];
      long distance = (dr*(long)dr) + (dg*(long)dg) + (db*(long)db); 
      if (distance<bestdistance)
      {
        bestdistance = distance;
        bestcolor = c;
      }
  }
  return bestcolor;
}


// callback to process commands from EV3
void receiveData(int byteCount)
{
  byte cmd = Wire.read();   
  switch (cmd)
  {
    case COMMAND_STARTSCAN0:
    case COMMAND_STARTSCAN1:
    case COMMAND_STARTSCAN2:
      scan_mode = cmd - COMMAND_STARTSCAN0;
      break;
    case COMMAND_GETSAMPLE:
      get_sample_number = Wire.read();
      break;
  }
  while (Wire.available())
  {
    Wire.read();
  }
}

// callback for sending data (must be fast to not let master wait)
void sendData()
{  
  // check if have requested to give a specific raw sample data
  if (get_sample_number>=0)
  {
      Wire.write(samples[get_sample_number],24);
      get_sample_number = -1;
  }
  // default mode: return previously converted colors
  else
  {
      Wire.write(colors,20);
  }
}


