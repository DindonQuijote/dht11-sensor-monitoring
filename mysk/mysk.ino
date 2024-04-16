#include <DHT.h>
//#include <DHT_U.h>
#define DHTPIN 2
#define DHTTYPE DHT11
DHT dht(DHTPIN,DHTTYPE);

void setup() {
  // put your setup code here, to run once:
  Serial.begin(57600);
  dht.begin();

}

void loop() {
  // put your main code here, to run repeatedly:
      
      float humidity = dht.readHumidity();
      float temperature = dht.readTemperature();

      if(isnan(humidity)|| isnan(temperature)){
        Serial.println("error in scanning sensor");
        return;
      }
      // send data to C#
      //Serial.print("Hum: ");
      Serial.print("@"); // start data charakter
      Serial.print(temperature); Serial.print("A");
      Serial.print(humidity); Serial.print("B");
      Serial.print("\n"); //end data character
      delay(2000);
  }
