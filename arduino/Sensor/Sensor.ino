#include <Wire.h>

// defines for setting and clearing register bits
#ifndef cbi
#define cbi(sfr, bit) (_SFR_BYTE(sfr) &= ~_BV(bit))
#endif
#ifndef sbi
#define sbi(sfr, bit) (_SFR_BYTE(sfr) |= _BV(bit))
#endif

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
  cbi(ADCSRA,ADPS2) ;
  sbi(ADCSRA,ADPS1) ;
  cbi(ADCSRA,ADPS0) ;
  
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
        for (numsamples=0; numsamples<50; numsamples++) {    
             byte active = B11111111;
             if (numsamples>4) {
                if (numsamples>35 && numsamples<44) active = B01100110;
                else                                active = B10011001;
             }
             while (millis() < start+1+numsamples);
             take_sample(samples[numsamples], active);
        }
        colors[0]  = extract_color(0,0,0);
        colors[1]  = extract_color(0,2,33);
        colors[2]  = extract_color(0,3,49);
        colors[3]  = extract_color(1,0,0);
        colors[4]  = extract_color(1,1,40);
        colors[5]  = extract_color(2,0,0);
        colors[6]  = extract_color(2,1,40);
        colors[7]  = extract_color(3,0,0);
        colors[8]  = extract_color(3,2,32);
        colors[9]  = extract_color(3,3,47);
        colors[10] = extract_color(4,0,0);
        colors[11] = extract_color(4,2,34);
        colors[12] = extract_color(4,3,46);
        colors[13] = extract_color(5,0,0);
        colors[14] = extract_color(5,1,40);
        colors[15] = extract_color(6,0,0);
        colors[16] = extract_color(6,1,40);
        colors[17] = extract_color(7,0,0);
        colors[18] = extract_color(7,2,33);    
        colors[19] = extract_color(7,3,46);    
        break;            
    case 1:      // scan when moving motor A & C (slow)
        scan_mode = -1;
        delay(16);
        start = millis();
        for (numsamples=0; numsamples<60; numsamples++) { 
             byte active = B11111111;
             if (numsamples>4) {
                if (numsamples>36 && numsamples<44) active = B10011001;
                else                                active = B01100110;
             }
             while (millis() < start+1+numsamples);
             take_sample(samples[numsamples], active);
        }
        colors[0]  = extract_color(0,0,0);
        colors[1]  = extract_color(0,1,41);
        colors[2]  = extract_color(1,0,0);
        colors[3]  = extract_color(1,2,33);
        colors[4]  = extract_color(1,3,56);
        colors[5]  = extract_color(2,0,0);
        colors[6]  = extract_color(2,2,33);
        colors[7]  = extract_color(2,3,55);      
        colors[8]  = extract_color(3,0,0);
        colors[9]  = extract_color(3,1,41);
        colors[10] = extract_color(4,0,0);
        colors[11] = extract_color(4,1,41);
        colors[12] = extract_color(5,0,0);
        colors[13] = extract_color(5,2,36);
        colors[14] = extract_color(5,3,58);
        colors[15] = extract_color(6,0,0);
        colors[16] = extract_color(6,2,33);
        colors[17] = extract_color(6,3,56);
        colors[18] = extract_color(7,0,0);
        colors[19] = extract_color(7,1,41);    
        break;                
    case 2:
        scan_mode = -1;
        take_sample(samples[0], B11111111);
        colors[0] = extract_color(0,0,0);
        colors[1] = extract_color(1,0,0);
        colors[2] = extract_color(2,0,0);
        colors[3] = extract_color(3,0,0);      
        colors[4] = extract_color(4,0,0);
        colors[5] = extract_color(5,0,0);
        colors[6] = extract_color(6,0,0);
        colors[7] = extract_color(7,0,0);
        break;        
  }  
}

int rd(int i)
{
  int p = scan_pin[i];
  analogRead(p);
  return (analogRead(p) + analogRead(p) + analogRead(p)) >> 2;
}

void take_sample(byte* s, byte active)
{  
  for (byte i=0; i<8; i++) {
    // take no reading of inactive sensors
    if ( (active & (0x01<<i)) == 0) {
      s[3*i+0] = 0;
      s[3*i+1] = 0;
      s[3*i+2] = 0;
      continue;
    }

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
        s[3*i+0] = 255;
      } else if (r>ambient) {
        s[3*i+0] = (r - ambient); 
      } else {
        s[3*i+0] = 0;
      }
      if (g>ambient+255) {
        s[3*i+1] = 255;
      } else if (g>ambient) {
        s[3*i+1] = (g - ambient); 
      } else {
        s[3*i+1] = 0;
      }
      if (b>ambient+255) {
        s[3*i+2] = 255;
      } else if (b>ambient) {
        s[3*i+2] = (b - ambient);
      } else {
        s[3*i+2] = 0;
      }
  }
}

byte extract_color(int sensor, int orientation, int sample)
{
  long bestdistance = 10000000;
  byte bestcolor = 0;

  byte r = samples[sample][sensor*3];  
  byte g = samples[sample][sensor*3+1];  
  byte b = samples[sample][sensor*3+2];  
  
  byte reference[3];
  
  byte c;
  for (c=0; c<6; c++)
  {
      getcalibration(sensor,orientation,c,reference);
      
      int dr = ((int) r)-reference[0];
      int dg = ((int) g)-reference[1];
      int db = ((int) b)-reference[2];
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

void getcalibration(byte sensor,byte orientation,byte color,byte* rgb) {
   switch (sensor*24+orientation*6+color) {
        case 0: rgb[0]=116; rgb[1]=180; rgb[2]=153; break;
        case 1: rgb[0]=102; rgb[1]=45; rgb[2]=39; break;
        case 2: rgb[0]=30; rgb[1]=99; rgb[2]=42; break;
        case 3: rgb[0]=29; rgb[1]=47; rgb[2]=58; break;
        case 4: rgb[0]=109; rgb[1]=148; rgb[2]=49; break;
        case 5: rgb[0]=105; rgb[1]=55; rgb[2]=40; break;

        case 6: rgb[0]=136; rgb[1]=186; rgb[2]=191; break;
        case 7: rgb[0]=113; rgb[1]=37; rgb[2]=31; break;
        case 8: rgb[0]=23; rgb[1]=97; rgb[2]=39; break;
        case 9: rgb[0]=25; rgb[1]=42; rgb[2]=62; break;
        case 10: rgb[0]=131; rgb[1]=154; rgb[2]=52; break;
        case 11: rgb[0]=125; rgb[1]=52; rgb[2]=37; break;

        case 12: rgb[0]=133; rgb[1]=211; rgb[2]=178; break;
        case 13: rgb[0]=107; rgb[1]=15; rgb[2]=10; break;
        case 14: rgb[0]=5; rgb[1]=91; rgb[2]=17; break;
        case 15: rgb[0]=5; rgb[1]=17; rgb[2]=43; break;
        case 16: rgb[0]=122; rgb[1]=161; rgb[2]=30; break;
        case 17: rgb[0]=122; rgb[1]=38; rgb[2]=22; break;

        case 18: rgb[0]=110; rgb[1]=166; rgb[2]=140; break;
        case 19: rgb[0]=94; rgb[1]=12; rgb[2]=7; break;
        case 20: rgb[0]=2; rgb[1]=72; rgb[2]=12; break;
        case 21: rgb[0]=2; rgb[1]=13; rgb[2]=32; break;
        case 22: rgb[0]=104; rgb[1]=129; rgb[2]=24; break;
        case 23: rgb[0]=106; rgb[1]=29; rgb[2]=15; break;

        case 24: rgb[0]=140; rgb[1]=208; rgb[2]=237; break;
        case 25: rgb[0]=122; rgb[1]=48; rgb[2]=49; break;
        case 26: rgb[0]=27; rgb[1]=110; rgb[2]=55; break;
        case 27: rgb[0]=26; rgb[1]=51; rgb[2]=79; break;
        case 28: rgb[0]=134; rgb[1]=169; rgb[2]=69; break;
        case 29: rgb[0]=132; rgb[1]=59; rgb[2]=53; break;

        case 30: rgb[0]=125; rgb[1]=183; rgb[2]=199; break;
        case 31: rgb[0]=104; rgb[1]=47; rgb[2]=42; break;
        case 32: rgb[0]=25; rgb[1]=103; rgb[2]=49; break;
        case 33: rgb[0]=25; rgb[1]=48; rgb[2]=68; break;
        case 34: rgb[0]=116; rgb[1]=146; rgb[2]=60; break;
        case 35: rgb[0]=115; rgb[1]=57; rgb[2]=47; break;

        case 36: rgb[0]=137; rgb[1]=206; rgb[2]=216; break;
        case 37: rgb[0]=111; rgb[1]=17; rgb[2]=10; break;
        case 38: rgb[0]=5; rgb[1]=91; rgb[2]=21; break;
        case 39: rgb[0]=6; rgb[1]=20; rgb[2]=50; break;
        case 40: rgb[0]=127; rgb[1]=161; rgb[2]=44; break;
        case 41: rgb[0]=128; rgb[1]=37; rgb[2]=24; break;

        case 42: rgb[0]=147; rgb[1]=145; rgb[2]=180; break;
        case 43: rgb[0]=123; rgb[1]=15; rgb[2]=10; break;
        case 44: rgb[0]=7; rgb[1]=62; rgb[2]=18; break;
        case 45: rgb[0]=5; rgb[1]=25; rgb[2]=50; break;
        case 46: rgb[0]=141; rgb[1]=115; rgb[2]=32; break;
        case 47: rgb[0]=147; rgb[1]=31; rgb[2]=22; break;

        case 48: rgb[0]=100; rgb[1]=193; rgb[2]=165; break;
        case 49: rgb[0]=87; rgb[1]=50; rgb[2]=35; break;
        case 50: rgb[0]=26; rgb[1]=106; rgb[2]=41; break;
        case 51: rgb[0]=26; rgb[1]=54; rgb[2]=58; break;
        case 52: rgb[0]=97; rgb[1]=154; rgb[2]=49; break;
        case 53: rgb[0]=89; rgb[1]=56; rgb[2]=37; break;

        case 54: rgb[0]=86; rgb[1]=180; rgb[2]=139; break;
        case 55: rgb[0]=75; rgb[1]=44; rgb[2]=28; break;
        case 56: rgb[0]=21; rgb[1]=99; rgb[2]=35; break;
        case 57: rgb[0]=20; rgb[1]=47; rgb[2]=48; break;
        case 58: rgb[0]=84; rgb[1]=143; rgb[2]=44; break;
        case 59: rgb[0]=77; rgb[1]=49; rgb[2]=30; break;

        case 60: rgb[0]=90; rgb[1]=165; rgb[2]=149; break;
        case 61: rgb[0]=80; rgb[1]=14; rgb[2]=7; break;
        case 62: rgb[0]=4; rgb[1]=75; rgb[2]=12; break;
        case 63: rgb[0]=3; rgb[1]=17; rgb[2]=35; break;
        case 64: rgb[0]=92; rgb[1]=138; rgb[2]=26; break;
        case 65: rgb[0]=93; rgb[1]=30; rgb[2]=16; break;

        case 66: rgb[0]=103; rgb[1]=151; rgb[2]=152; break;
        case 67: rgb[0]=85; rgb[1]=13; rgb[2]=8; break;
        case 68: rgb[0]=14; rgb[1]=77; rgb[2]=26; break;
        case 69: rgb[0]=3; rgb[1]=17; rgb[2]=33; break;
        case 70: rgb[0]=96; rgb[1]=113; rgb[2]=25; break;
        case 71: rgb[0]=98; rgb[1]=26; rgb[2]=16; break;

        case 72: rgb[0]=88; rgb[1]=126; rgb[2]=117; break;
        case 73: rgb[0]=77; rgb[1]=27; rgb[2]=25; break;
        case 74: rgb[0]=23; rgb[1]=69; rgb[2]=31; break;
        case 75: rgb[0]=23; rgb[1]=28; rgb[2]=43; break;
        case 76: rgb[0]=84; rgb[1]=100; rgb[2]=34; break;
        case 77: rgb[0]=79; rgb[1]=32; rgb[2]=27; break;

        case 78: rgb[0]=90; rgb[1]=147; rgb[2]=133; break;
        case 79: rgb[0]=77; rgb[1]=24; rgb[2]=25; break;
        case 80: rgb[0]=25; rgb[1]=73; rgb[2]=33; break;
        case 81: rgb[0]=25; rgb[1]=28; rgb[2]=45; break;
        case 82: rgb[0]=85; rgb[1]=111; rgb[2]=34; break;
        case 83: rgb[0]=79; rgb[1]=34; rgb[2]=30; break;

        case 84: rgb[0]=89; rgb[1]=120; rgb[2]=124; break;
        case 85: rgb[0]=73; rgb[1]=4; rgb[2]=5; break;
        case 86: rgb[0]=1; rgb[1]=52; rgb[2]=8; break;
        case 87: rgb[0]=1; rgb[1]=6; rgb[2]=31; break;
        case 88: rgb[0]=81; rgb[1]=89; rgb[2]=15; break;
        case 89: rgb[0]=82; rgb[1]=17; rgb[2]=8; break;

        case 90: rgb[0]=81; rgb[1]=103; rgb[2]=105; break;
        case 91: rgb[0]=59; rgb[1]=2; rgb[2]=4; break;
        case 92: rgb[0]=1; rgb[1]=43; rgb[2]=7; break;
        case 93: rgb[0]=1; rgb[1]=6; rgb[2]=23; break;
        case 94: rgb[0]=70; rgb[1]=74; rgb[2]=14; break;
        case 95: rgb[0]=67; rgb[1]=14; rgb[2]=8; break;

        case 96: rgb[0]=93; rgb[1]=165; rgb[2]=130; break;
        case 97: rgb[0]=81; rgb[1]=37; rgb[2]=31; break;
        case 98: rgb[0]=25; rgb[1]=91; rgb[2]=35; break;
        case 99: rgb[0]=28; rgb[1]=43; rgb[2]=55; break;
        case 100: rgb[0]=88; rgb[1]=133; rgb[2]=41; break;
        case 101: rgb[0]=85; rgb[1]=48; rgb[2]=32; break;

        case 102: rgb[0]=130; rgb[1]=186; rgb[2]=187; break;
        case 103: rgb[0]=108; rgb[1]=34; rgb[2]=27; break;
        case 104: rgb[0]=23; rgb[1]=99; rgb[2]=35; break;
        case 105: rgb[0]=23; rgb[1]=35; rgb[2]=57; break;
        case 106: rgb[0]=120; rgb[1]=146; rgb[2]=44; break;
        case 107: rgb[0]=117; rgb[1]=49; rgb[2]=33; break;

        case 108: rgb[0]=122; rgb[1]=198; rgb[2]=169; break;
        case 109: rgb[0]=104; rgb[1]=8; rgb[2]=5; break;
        case 110: rgb[0]=2; rgb[1]=86; rgb[2]=15; break;
        case 111: rgb[0]=1; rgb[1]=10; rgb[2]=41; break;
        case 112: rgb[0]=116; rgb[1]=150; rgb[2]=27; break;
        case 113: rgb[0]=114; rgb[1]=31; rgb[2]=16; break;

        case 114: rgb[0]=101; rgb[1]=160; rgb[2]=133; break;
        case 115: rgb[0]=84; rgb[1]=5; rgb[2]=4; break;
        case 116: rgb[0]=1; rgb[1]=69; rgb[2]=11; break;
        case 117: rgb[0]=7; rgb[1]=15; rgb[2]=37; break;
        case 118: rgb[0]=96; rgb[1]=122; rgb[2]=20; break;
        case 119: rgb[0]=98; rgb[1]=23; rgb[2]=13; break;

        case 120: rgb[0]=94; rgb[1]=117; rgb[2]=138; break;
        case 121: rgb[0]=80; rgb[1]=24; rgb[2]=25; break;
        case 122: rgb[0]=18; rgb[1]=64; rgb[2]=29; break;
        case 123: rgb[0]=18; rgb[1]=26; rgb[2]=49; break;
        case 124: rgb[0]=93; rgb[1]=95; rgb[2]=42; break;
        case 125: rgb[0]=87; rgb[1]=34; rgb[2]=28; break;

        case 126: rgb[0]=97; rgb[1]=117; rgb[2]=135; break;
        case 127: rgb[0]=76; rgb[1]=21; rgb[2]=23; break;
        case 128: rgb[0]=14; rgb[1]=60; rgb[2]=27; break;
        case 129: rgb[0]=14; rgb[1]=22; rgb[2]=44; break;
        case 130: rgb[0]=88; rgb[1]=91; rgb[2]=37; break;
        case 131: rgb[0]=88; rgb[1]=31; rgb[2]=27; break;

        case 132: rgb[0]=65; rgb[1]=92; rgb[2]=103; break;
        case 133: rgb[0]=54; rgb[1]=5; rgb[2]=4; break;
        case 134: rgb[0]=3; rgb[1]=40; rgb[2]=6; break;
        case 135: rgb[0]=3; rgb[1]=6; rgb[2]=23; break;
        case 136: rgb[0]=64; rgb[1]=72; rgb[2]=15; break;
        case 137: rgb[0]=64; rgb[1]=15; rgb[2]=9; break;

        case 138: rgb[0]=114; rgb[1]=113; rgb[2]=147; break;
        case 139: rgb[0]=95; rgb[1]=6; rgb[2]=7; break;
        case 140: rgb[0]=5; rgb[1]=48; rgb[2]=12; break;
        case 141: rgb[0]=5; rgb[1]=9; rgb[2]=34; break;
        case 142: rgb[0]=105; rgb[1]=87; rgb[2]=21; break;
        case 143: rgb[0]=105; rgb[1]=18; rgb[2]=16; break;

        case 144: rgb[0]=127; rgb[1]=210; rgb[2]=185; break;
        case 145: rgb[0]=109; rgb[1]=40; rgb[2]=34; break;
        case 146: rgb[0]=29; rgb[1]=112; rgb[2]=40; break;
        case 147: rgb[0]=33; rgb[1]=49; rgb[2]=64; break;
        case 148: rgb[0]=121; rgb[1]=167; rgb[2]=51; break;
        case 149: rgb[0]=114; rgb[1]=57; rgb[2]=40; break;

        case 150: rgb[0]=91; rgb[1]=144; rgb[2]=123; break;
        case 151: rgb[0]=79; rgb[1]=38; rgb[2]=30; break;
        case 152: rgb[0]=26; rgb[1]=84; rgb[2]=36; break;
        case 153: rgb[0]=26; rgb[1]=44; rgb[2]=48; break;
        case 154: rgb[0]=89; rgb[1]=119; rgb[2]=42; break;
        case 155: rgb[0]=79; rgb[1]=43; rgb[2]=32; break;

        case 156: rgb[0]=113; rgb[1]=195; rgb[2]=176; break;
        case 157: rgb[0]=96; rgb[1]=9; rgb[2]=7; break;
        case 158: rgb[0]=4; rgb[1]=85; rgb[2]=15; break;
        case 159: rgb[0]=3; rgb[1]=12; rgb[2]=38; break;
        case 160: rgb[0]=108; rgb[1]=150; rgb[2]=26; break;
        case 161: rgb[0]=108; rgb[1]=29; rgb[2]=15; break;

        case 162: rgb[0]=130; rgb[1]=150; rgb[2]=164; break;
        case 163: rgb[0]=107; rgb[1]=7; rgb[2]=6; break;
        case 164: rgb[0]=3; rgb[1]=61; rgb[2]=12; break;
        case 165: rgb[0]=4; rgb[1]=10; rgb[2]=37; break;
        case 166: rgb[0]=120; rgb[1]=113; rgb[2]=23; break;
        case 167: rgb[0]=122; rgb[1]=21; rgb[2]=13; break;

        case 168: rgb[0]=119; rgb[1]=219; rgb[2]=196; break;
        case 169: rgb[0]=103; rgb[1]=50; rgb[2]=37; break;
        case 170: rgb[0]=24; rgb[1]=119; rgb[2]=42; break;
        case 171: rgb[0]=24; rgb[1]=53; rgb[2]=66; break;
        case 172: rgb[0]=113; rgb[1]=175; rgb[2]=58; break;
        case 173: rgb[0]=112; rgb[1]=62; rgb[2]=43; break;

        case 174: rgb[0]=110; rgb[1]=228; rgb[2]=196; break;
        case 175: rgb[0]=93; rgb[1]=49; rgb[2]=38; break;
        case 176: rgb[0]=31; rgb[1]=127; rgb[2]=51; break;
        case 177: rgb[0]=24; rgb[1]=55; rgb[2]=68; break;
        case 178: rgb[0]=105; rgb[1]=179; rgb[2]=57; break;
        case 179: rgb[0]=102; rgb[1]=64; rgb[2]=45; break;

        case 180: rgb[0]=169; rgb[1]=240; rgb[2]=240; break;
        case 181: rgb[0]=141; rgb[1]=13; rgb[2]=13; break;
        case 182: rgb[0]=5; rgb[1]=104; rgb[2]=24; break;
        case 183: rgb[0]=5; rgb[1]=20; rgb[2]=58; break;
        case 184: rgb[0]=161; rgb[1]=183; rgb[2]=39; break;
        case 185: rgb[0]=158; rgb[1]=37; rgb[2]=28; break;

        case 186: rgb[0]=134; rgb[1]=253; rgb[2]=223; break;
        case 187: rgb[0]=111; rgb[1]=15; rgb[2]=14; break;
        case 188: rgb[0]=5; rgb[1]=109; rgb[2]=21; break;
        case 189: rgb[0]=4; rgb[1]=20; rgb[2]=57; break;
        case 190: rgb[0]=131; rgb[1]=197; rgb[2]=42; break;
        case 191: rgb[0]=127; rgb[1]=40; rgb[2]=26; break;

    }
}


