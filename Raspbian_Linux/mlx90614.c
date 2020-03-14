// gcc mlx90614.c -o mlx90614 -l bcm2835
// sudo ./mlx90614
#include <stdio.h>
#include <bcm2835.h>
#include <stdlib.h>
#include <fcntl.h>
#include <string.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <unistd.h>
#include <time.h>
#define AVG 1   //averaging samples


int main(int argc, char **argv)
{
    unsigned char buf[6];
    unsigned char i,reg;
    double temp=0,calc=0, skytemp,atemp;
    bcm2835_init();
    bcm2835_i2c_begin();
    bcm2835_i2c_set_baudrate(25000);
    // set address
    bcm2835_i2c_setSlaveAddress(0x5a);

    printf("\ndevice is working!!\n");

    calc=0;
    reg=7;

    for(i=0;i<AVG;i++)
	{
        bcm2835_i2c_begin();
        bcm2835_i2c_write (&reg, 1);
        bcm2835_i2c_read_register_rs(&reg,&buf[0],3);
        temp = (double) (((buf[1]) << 8) + buf[0]);
        temp = (temp * 0.02)-0.01;
        temp = temp - 273.15;
        calc+=temp;
        sleep(1);
    }

    skytemp=calc/AVG;
    calc=0;
    reg=6;

    for(i=0;i<AVG;i++){
        bcm2835_i2c_begin();
        bcm2835_i2c_write (&reg, 1);
        bcm2835_i2c_read_register_rs(&reg,&buf[0],3);
        temp = (double) (((buf[1]) << 8) + buf[0]);
        temp = (temp * 0.02)-0.01;
        temp = temp - 273.15;
        calc+=temp;
        sleep(1);
    }

    atemp=calc/AVG;

    printf("ambient temperature = %04.2f\n", atemp);
    printf("object temperature = %04.2f\n", skytemp);

    printf("done\n");

    return 0;
}
