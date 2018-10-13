﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;
using Hidwizards.IOWrapper.Libraries.SubscriptionHandlerNs;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Interfaces;

namespace Core_SpaceMouse
{
    [Export(typeof(IProvider))]
    public class Core_SpaceMouse : IInputProvider
    {
        private static HidDevice _device;
        private readonly UpdateProcessor _updateProcessor = new UpdateProcessor();
        //private InputSubscriptionRequest _subReq;
        private readonly SubscriptionHandler _subHandler;
        private ProviderDescriptor _providerDescriptor;

        private DeviceDescriptor _spaceMouseProDescriptor =
            new DeviceDescriptor {DeviceHandle = "VID_046D&PID_C62B", DeviceInstance = 0};


        public Core_SpaceMouse()
        {
            _providerDescriptor = new ProviderDescriptor {ProviderName = ProviderName};

            _subHandler = new SubscriptionHandler(new DeviceDescriptor(), OnDeviceEmpty);

            _device = HidDevices.Enumerate(0x046d, 0xc62b).FirstOrDefault();
            _device.OpenDevice();
            _device.MonitorDeviceEvents = true;
            _device.ReadReport(OnReport);
        }

        private void OnDeviceEmpty(object sender, DeviceDescriptor e)
        {
            //throw new NotImplementedException();
        }

        private void OnReport(HidReport report)
        {
            var updates = _updateProcessor.ProcessUpdate(report);
            foreach (var update in updates)
            {
                if (_subHandler.ContainsKey(update.BindingType, update.Index))
                {
                    _subHandler.FireCallbacks(new BindingDescriptor{Type = update.BindingType, Index = update.Index}, update.Value);
                }
                //Console.WriteLine($"Type: {update.BindingType}, Index: {update.Index}, Value: {update.Value}");
            }
            _device.ReadReport(OnReport);
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public string ProviderName { get; } = "Core_SpaceMouse";
        public bool IsLive { get; }
        public void RefreshLiveState()
        {
            //throw new NotImplementedException();
        }

        public void RefreshDevices()
        {
            //throw new NotImplementedException();
        }

        public ProviderReport GetInputList()
        {
            var providerReport = new ProviderReport
            {
                Title = "SpaceMouse (Core)",
                Description = "Allows reading of SpaceMouse devices.",
                API = "HidLibrary",
                ProviderDescriptor = _providerDescriptor
            };
            //var deviceDescriptor = new DeviceDescriptor { DeviceHandle = guidList.Key, DeviceInstance = i };
            providerReport.Devices.Add(GetInputDeviceReport(_spaceMouseProDescriptor));

            return providerReport;
        }

        public DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq)
        {
            return GetInputDeviceReport(subReq.DeviceDescriptor);
        }

        
        public DeviceReport GetInputDeviceReport(DeviceDescriptor deviceDescriptor)
    {
            //throw new NotImplementedException();

            var deviceReport = new DeviceReport
            {
                DeviceDescriptor = _spaceMouseProDescriptor,
                DeviceName = "SpaceMouse Pro"
            };

            // ----- Axes -----
            var axisInfo = new DeviceReportNode
            {
                Title = "Axes"
            };
            // SharpDX tells us how many axes there are, but not *which* axes.
            // Enumerate all possible DI axes and check to see if this stick has each axis
            for (var i = 0; i < 6; i++)
            {
                axisInfo.Bindings.Add(new BindingReport
                {
                    Title = $"Axis {i + 1}",
                    Category = BindingCategory.Signed,
                    BindingDescriptor = new BindingDescriptor
                    {
                        //Index = i,
                        Index = i,
                        //Name = axisNames[i],
                        Type = BindingType.Axis
                    }
                });
            }

            deviceReport.Nodes.Add(axisInfo);

            // ----- Buttons -----
            var buttonInfo = new DeviceReportNode
            {
                Title = "Buttons"
            };
            for (var btn = 0; btn < 32; btn++)
            {
                buttonInfo.Bindings.Add(new BindingReport
                {
                    Title = "Button " + (btn + 1),
                    Category = BindingCategory.Momentary,
                    BindingDescriptor = new BindingDescriptor
                    {
                        Index = btn,
                        Type = BindingType.Button
                    }
                });
            }

            deviceReport.Nodes.Add(buttonInfo);

            return deviceReport;
        }

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            _subHandler.Subscribe(subReq);
            return true;
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            //throw new NotImplementedException();
            _subHandler.Unsubscribe(subReq);
            return true;
        }
    }
}