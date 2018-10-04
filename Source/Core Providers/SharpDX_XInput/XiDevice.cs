﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Devices;
using HidWizards.IOWrapper.ProviderInterface.Subscriptions;
using SharpDX.XInput;

namespace SharpDX_XInput
{
    public class XiDevice
    {
        private DeviceDescriptor _deviceDescriptor;
        private IDeviceLibrary<int> _deviceLibrary;
        private SubscriptionHandler _subHandler;
        private XiDeviceUpdateHandler _deviceUpdateHandler;
        private Thread _pollThread;
        private readonly Controller _controller;

        public XiDevice(DeviceDescriptor deviceDescriptor, IDeviceLibrary<int> deviceLibrary)
        {
            _deviceDescriptor = deviceDescriptor;
            _deviceLibrary = deviceLibrary;
            _subHandler = new SubscriptionHandler(deviceDescriptor, DeviceEmptyHandler);
            _deviceUpdateHandler = new XiDeviceUpdateHandler(deviceDescriptor, _subHandler);

            _controller = new Controller((UserIndex)deviceDescriptor.DeviceInstance);

            _pollThread = new Thread(PollThread);
            _pollThread.Start();

        }

        public void SubscribeInput(InputSubscriptionRequest subReq)
        {
            _subHandler.Subscribe(subReq);
        }

        private void DeviceEmptyHandler(object sender, DeviceDescriptor e)
        {
            throw new NotImplementedException();
        }

        private void PollThread()
        {
            while (true)
            {
                if (!_controller.IsConnected)
                    return;
                _deviceUpdateHandler.ProcessUpdate(_controller.GetState());
                Thread.Sleep(10);
            }

        }
    }
}