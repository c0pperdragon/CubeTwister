
// RGB pin assigment
#define redPin    4
#define greenPin  6
#define bluePin   5

// the setup function runs once when you press reset or power the board
void setup() {
  // initialize digital pin 13 as an output.
  pinMode(redPin, OUTPUT);
  pinMode(greenPin, OUTPUT);
  pinMode(bluePin, OUTPUT);

  digitalWrite(redPin, HIGH); 
  digitalWrite(greenPin, HIGH); 
  digitalWrite(bluePin, HIGH); 

}

// the loop function runs over and over again forever
void loop() {

  digitalWrite(redPin, LOW); 
  delay(500);             
  digitalWrite(redPin, HIGH);

  digitalWrite(greenPin, LOW); 
  delay(500);             
  digitalWrite(greenPin, HIGH);

  digitalWrite(bluePin, LOW); 
  delay(500);             
  digitalWrite(bluePin, HIGH);

  delay(500);
}
