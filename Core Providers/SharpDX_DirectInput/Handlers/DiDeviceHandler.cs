﻿using System;
using System.Collections.Concurrent;
using Providers;
using Providers.Handlers;
using SharpDX.DirectInput;
using SharpDX_DirectInput.Helpers;

namespace SharpDX_DirectInput.Handlers
{
    class DiDeviceHandler : DeviceHandler
    {
        private Joystick _joystick;
        private Guid _instanceGuid = Guid.Empty;
        private InputSubscriptionRequest _inputSubscriptionRequest = null;

        public  override void Initialize(InputSubscriptionRequest subReq)
        {
            base.Initialize(subReq);
            //Guid instanceGuid = Guid.Empty;
            var instances = Lookups.GetDeviceOrders(subReq.DeviceDescriptor.DeviceHandle);
            if (instances.Count >= subReq.DeviceDescriptor.DeviceInstance)
            {
                _instanceGuid = instances[subReq.DeviceDescriptor.DeviceInstance];
            }

            if (_instanceGuid == Guid.Empty)
            {
                throw new Exception($"DeviceHandle '{subReq.DeviceDescriptor.DeviceHandle}' was not found");
            }
            else
            {
                //ToDo: When should we re-attempt to acquire?
                if (DiHandler.DiInstance.IsDeviceAttached(_instanceGuid))
                {
                    _joystick = new Joystick(DiHandler.DiInstance, _instanceGuid);
                    _joystick.Properties.BufferSize = 128;
                    _joystick.Acquire();
                }
            }
        }

        public override bool Subscribe(InputSubscriptionRequest subReq)
        {
            if (_inputSubscriptionRequest == null)
            {
                Initialize(subReq);
            }
            var bindingType = subReq.BindingDescriptor.Type;
            var dict = _bindingDictionary
                .GetOrAdd(subReq.BindingDescriptor.Type,
                    new ConcurrentDictionary<int, BindingHandler>());

            switch (bindingType)
            {
                case BindingType.Axis:
                    return dict
                        .GetOrAdd((int)Lookups.directInputMappings[subReq.BindingDescriptor.Type][subReq.BindingDescriptor.Index], new DiAxisBindingHandler())
                        .Subscribe(subReq);
                case BindingType.Button:
                    return dict
                        .GetOrAdd((int)Lookups.directInputMappings[subReq.BindingDescriptor.Type][subReq.BindingDescriptor.Index], new DiButtonBindingHandler())
                        .Subscribe(subReq);
                case BindingType.POV:
                    return dict
                        .GetOrAdd((int)Lookups.directInputMappings[subReq.BindingDescriptor.Type][subReq.BindingDescriptor.Index], new DiPovBindingHandler())
                        .Subscribe(subReq);
                default:
                    throw new NotImplementedException();
            }
        }

        public override bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            return true;
        }

        public override void Poll()
        {
            // ToDo: Pollthread should not be spamming here if joystick is not attached


            JoystickUpdate[] data;
            // ToDo: Find better way of detecting unplug. DiHandler.DiInstance.IsDeviceAttached(instanceGuid) kills performance
            try
            {
                // Try / catch seems the only way for now to ensure no crashes on replug
                data = _joystick.GetBufferedData();
            }
            catch
            {
                return;
            }
            foreach (var state in data)
            {
                int offset = (int)state.Offset;
                var bindingType = Lookups.OffsetToType(state.Offset);
                if (_bindingDictionary.ContainsKey(bindingType) && _bindingDictionary[bindingType].ContainsKey(offset))
                {
                    _bindingDictionary[bindingType][offset].Poll(state.Value);
                }
            }
        }
    }
}