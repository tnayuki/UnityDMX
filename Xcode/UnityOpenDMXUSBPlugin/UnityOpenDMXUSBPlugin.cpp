//
//  UnityOpenDMXUSB.m
//  UnityOpenDMXUSBPlugin
//
//  Created by Toru Nayuki on 2013/11/24.
//  Copyright (c) 2013年 Toru Nayuki. All rights reserved.
//

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <dispatch/dispatch.h>

#include <ftdi.h>
#include <libusb.h>

static struct ftdi_context *ftdic;

static unsigned char buffer[512];
static dispatch_semaphore_t buffer_semaphore;

static bool stop = false;

#define DMX_MAB 160    // Mark After Break 8 uS or more
#define DMX_BREAK 110  // Break 88 uS or more

static int do_dmx_break(struct ftdi_context* ftdic)
{
    int ret;
    
	if ((ret = ftdi_set_line_property2(ftdic, BITS_8, STOP_BIT_2, NONE, BREAK_ON)) < 0)
	{
        fprintf(stderr, "unable to set BREAK ON: %d (%s)\n", ret, ftdi_get_error_string(ftdic));
        return EXIT_FAILURE;
	}
    usleep(DMX_BREAK);
	if ((ret = ftdi_set_line_property2(ftdic, BITS_8, STOP_BIT_2, NONE, BREAK_OFF)) < 0)
	{
        fprintf(stderr, "unable to set BREAK OFF: %d (%s)\n", ret, ftdi_get_error_string(ftdic));
        return EXIT_FAILURE;
	}
    usleep(DMX_MAB);
	return EXIT_SUCCESS;
}

static int dmx_init(struct ftdi_context* ftdic)
{
    int ret;
    
    if (ftdi_init(ftdic) < 0)
    {
        fprintf(stderr, "ftdi_init failed\n");
        return EXIT_FAILURE;
    }
    
    if ((ret = ftdi_usb_open(ftdic, 0x0403, 0x6001)) < 0)
    {
        fprintf(stderr, "unable to open ftdi device: %d (%s)\n", ret, ftdi_get_error_string(ftdic));
        return EXIT_FAILURE;
    }
    
	return EXIT_SUCCESS;
}

static int dmx_write(struct ftdi_context* ftdic, unsigned char* dmx, int size)
{
    int ret;
	do
	{
        ftdi_usb_reset(ftdic);

        if ((ret = ftdi_set_baudrate(ftdic, 250000)) < 0)
        {
            fprintf(stderr, "unable to set baudrate: %d (%s)\n", ret, ftdi_get_error_string(ftdic));
            return EXIT_FAILURE;
        }
        
        if ((ret = ftdi_set_line_property(ftdic, BITS_8, STOP_BIT_2, NONE)) < 0)
        {
            fprintf(stderr, "unable to set line property: %d (%s)\n", ret, ftdi_get_error_string(ftdic));
            return EXIT_FAILURE;
        }
        
        if ((ret = ftdi_setflowctrl(ftdic, SIO_DISABLE_FLOW_CTRL)) < 0)
        {
            fprintf(stderr, "unable to set flow control: %d (%s)\n", ret, ftdi_get_error_string(ftdic));
            return EXIT_FAILURE;
        }
    } while ((ret = do_dmx_break(ftdic)) != EXIT_SUCCESS);
    
    struct ftdi_transfer_control *tc;
    if ((tc = ftdi_write_data_submit(ftdic, dmx, size)) == NULL) {
        fprintf(stderr, "unable to write data: %d (%s)\n", ret, ftdi_get_error_string(ftdic));
        ret = EXIT_FAILURE;
    } else {
        while (!ftdi_transfer_data_done(tc)) {
            usleep(88);
        }
    }
    
	return ret;
}

extern "C" {
    void InitializePlugin() {
        buffer_semaphore = dispatch_semaphore_create(1);
        
        dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_HIGH, 0), ^{
            ftdic = ftdi_new();
            dmx_init(ftdic);
            
            unsigned char dmx[513];
            while (!stop) {
                dmx[0] = 0;
                
                dispatch_semaphore_wait(buffer_semaphore, DISPATCH_TIME_FOREVER);
                memcpy(dmx + 1, buffer, 512);
                dispatch_semaphore_signal(buffer_semaphore);
                
                dmx_write(ftdic, dmx, 513);
            }
            
            ftdi_free(ftdic);
            ftdic = NULL;
        });
    }

    void DeinitializePlugin() {
        stop = true;
    }

    void UpdateBuffer(unsigned char *buffer2) {
        dispatch_semaphore_wait(buffer_semaphore, DISPATCH_TIME_FOREVER);
        memcpy(buffer, buffer2, 512);
        dispatch_semaphore_signal(buffer_semaphore);
    }
}
