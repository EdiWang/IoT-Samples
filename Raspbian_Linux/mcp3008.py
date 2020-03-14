#!/usr/bin/python
 
import spidev
import time
import os
 
# Open SPI bus
spi = spidev.SpiDev()
spi.open(0,0)
 
# Function to read SPI data from MCP3008 chip
# Channel must be an integer 0-7
def ReadChannel(channel):
  adc = spi.xfer2([1,(8+channel)<<4,0])
  data = ((adc[1]&3) << 8) + adc[2]
  return data
 
# Function to convert data to voltage level,
# rounded to specified number of decimal places.
def ConvertVolts(data,places):
  volts = (data * 3.3) / float(1023)
  volts = round(volts,places)
  return volts
 
# Define sensor channels
channel0 = 0
channel1 = 1
 
# Define delay between readings
delay = 0.7

print("--------------MCP3008 ADC--------------")
print(" SPI Bus, Refreshing at {}s".format(delay))
print("---------------------------------------")
 
while True:
 
  # Read channel 0 data
  s_level0 = ReadChannel(channel0)
  s_volts0 = ConvertVolts(s_level0,2)

  # Read channel 1 data
  s_level1 = ReadChannel(channel1)
  s_volts1 = ConvertVolts(s_level1,2)
 
  # Print out results
  print("[CH0] {} ({}V)\t[CH1] {} ({}V)".format('%04d' % s_level0,s_volts0,'%04d' % s_level1,s_volts1))

  # Wait before repeating loop
  time.sleep(delay)
