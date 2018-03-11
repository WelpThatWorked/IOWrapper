﻿using System;
using System.Collections.Concurrent;
using System.Threading;
using HidWizards.IOWrapper.ProviderInterface;
using HidWizards.IOWrapper.ProviderInterface.Handlers;
using SharpDX.DirectInput;
using SharpDX_DirectInput.Helpers;
using HidWizards.IOWrapper.DataTransferObjects;

namespace SharpDX_DirectInput.Handlers
{
    internal class DiDeviceHandler : DeviceHandler
    {
        //private Joystick _joystick;
        private readonly Guid _instanceGuid = Guid.Empty;

        public DiDeviceHandler(DeviceDescriptor deviceDescriptor) : base(deviceDescriptor)
        {
            //Guid instanceGuid = Guid.Empty;
            var instances = Lookups.GetDeviceOrders(deviceDescriptor.DeviceHandle);
            if (instances.Count >= deviceDescriptor.DeviceInstance)
            {
                _instanceGuid = instances[deviceDescriptor.DeviceInstance];
            }

            if (_instanceGuid == Guid.Empty)
            {
                throw new Exception($"DeviceHandle '{deviceDescriptor.DeviceHandle}' was not found");
            }
        }

        protected override int GetBindingKey(InputSubscriptionRequest subReq)
        {
            return (int)Lookups.directInputMappings[subReq.BindingDescriptor.Type][subReq.BindingDescriptor.Index];
        }

        protected override BindingHandler CreateBindingHandler(InputSubscriptionRequest subReq)
        {
            switch (subReq.BindingDescriptor.Type)
            {
                case BindingType.Axis:
                    return new DiAxisBindingHandler(subReq);
                case BindingType.Button:
                    return new DiButtonBindingHandler(subReq);
                case BindingType.POV:
                    return new DiPovBindingHandler(subReq);
                default:
                    throw new NotImplementedException();
            }
        }

        protected override DevicePoller CreateDevicePoller(DetectionMode mode)
        {
            switch (mode)
            {
                case DetectionMode.Bind:
                    return new DiDevicePoller(_deviceDescriptor, ProcessBindModePoll);
                case DetectionMode.Subscription:
                    return new DiDevicePoller(_deviceDescriptor, ProcessSubscriptionModePoll);
                default:
                    throw new NotImplementedException();
            }
        }

        public void ProcessSubscriptionModePoll(BindingDescriptor bindingDescriptor, int state)
        {
            var bindingType = bindingDescriptor.Type;
            var offset = bindingDescriptor.Index;
            if (BindingDictionary.ContainsKey(bindingType) && BindingDictionary[bindingType].ContainsKey(offset))
            {
                BindingDictionary[bindingType][offset].Poll(state);
            }
        }

        public override void Poll()
        {
            //// ToDo: Pollthread should not be spamming here if joystick is not attached


            //JoystickUpdate[] data;
            //// ToDo: Find better way of detecting unplug. DiHandler.DiInstance.IsDeviceAttached(instanceGuid) kills performance
            //try
            //{
            //    // Try / catch seems the only way for now to ensure no crashes on replug
            //    data = _joystick.GetBufferedData();
            //}
            //catch
            //{
            //    return;
            //}
            //foreach (var state in data)
            //{
            //    int offset = (int)state.Offset;
            //    var bindingType = Lookups.OffsetToType(state.Offset);
            //    if (BindingDictionary.ContainsKey(bindingType) && BindingDictionary[bindingType].ContainsKey(offset))
            //    {
            //        BindingDictionary[bindingType][offset].Poll(state.Value);
            //    }
            //}
        }

        public override void Dispose()
        {
            _devicePoller.SetPollThreadState(false);
            //try
            //{
            //    _joystick.Unacquire();
            //    _joystick.Dispose();
            //}
            //catch
            //{

            //}
            //_joystick = null;
            base.Dispose();
        }
    }
}