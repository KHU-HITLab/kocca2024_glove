void setup() {
  // put your setup code here, to run once:
  Serial.begin(921600);
  analogWriteFreq(100'000);
  analogWriteResolution(8);
}

void loop() {
  // put your main code here, to run repeatedly:
  if (Serial.available())
  {
    uint8_t buf[5];
    Serial.readBytes(buf, 5);
    Serial.print(buf[0]);
    Serial.print(buf[1]);
    Serial.print(buf[2]);
    Serial.print(buf[3]);
    Serial.print(buf[4]);
    Serial.print('\n');

    for (int i = 0; i < 3; i++)
    {
      analogWrite(2 + i, buf[i]);
    }

    // clear rx buffer
    while (Serial.available())
    {
      Serial.read();
    }
  }
}
