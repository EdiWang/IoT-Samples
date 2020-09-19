from gpiozero import MotionSensor
from picamera import PiCamera
from datetime import datetime
from azure.storage.blob import BlockBlobService
from azure.storage.blob import ContentSettings

camera = PiCamera()
pir = MotionSensor(4)
block_blob_service = BlockBlobService(account_name='YOURACCOUNT', account_key='YOURKEY')

while True:
    pir.wait_for_motion()
    filename = "pircam-" +  datetime.now().strftime("%Y-%m-%d_%H.%M.%S.jpg")
    camera.capture(filename)
    pir.wait_for_no_motion()
    block_blob_service.create_blob_from_path(
    'YOURCONTAINER',
    filename,
    filename,
    content_settings=ContentSettings(content_type='image/jpeg'))