# Raspberry Pi Home Surveillance Camera
Raspberry Pi Motion Camera with an Azure Website to remotely view the photos.

## Techonology and Framework Used
- Python
  - Azure Python SDK
- ASP.NET MVC
  - C#
  - Azure .NET SDK
  - Bootstrap
  - jQuery

## Deploy and Run
0. Connect PIR Sensor to GPIO 4 and setup Pi Camera.
1. Edit RaspbianProgram\pricam.py, replace YOURACCOUNT, YOURKEY, YOURCONTAINER with your azure storage account information.
2. Copy RaspbianProgram\pricam.py to Raspberry Pi, use "sudo python pricam.py" to run.
3. Edit AzureWebsite\PirCamWatcher\Web.config, replace StorageConnectionString, AzureStorageAccountContainer, AuthUserName, AuthPassword with your information
4. Deploy AzureWebsite to Microsoft Azure
5. You are good to go!

## License
The MIT License (MIT)
Copyright (c) 2016 edi.wang
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
