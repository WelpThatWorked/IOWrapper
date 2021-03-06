﻿using HidWizards.IOWrapper.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestApp.Wrappers;
using HidWizards.IOWrapper.DataTransferObjects;
using TestApp.Testers;

namespace TestApp
{
    class Launcher
    {
        static void Main(string[] args)
        {
            Debug.WriteLine("DBGVIEWCLEAR");
            var inputList = IOW.Instance.GetInputList();
            var outputList = IOW.Instance.GetOutputList();

            ProviderStatus.LogProviderStatuses();

            //var bindModeTester = new BindModeTester();

            //var vigemDs4OutputTester = new VigemDs4OutputTester();

            //var spaceMouse = new SpaceMouseTester("SpaceMouse", Library.Devices.SpaceMouse.Pro);
            //IOW.Instance.SetDetectionMode(DetectionMode.Bind, Library.Providers.SpaceMouse, Library.Devices.SpaceMouse.Pro, BindModeHandler);
            //var motör49Tester = new MidiTester("MIDI", Library.Devices.Midi.Motör49Main);
            //IOW.Instance.SetDetectionMode(DetectionMode.Bind, Library.Providers.Midi, Library.Devices.Midi.Motör49Main, BindModeHandler);
            //var subReq = new OutputSubscriptionRequest
            //{
            //    ProviderDescriptor = Library.Providers.Midi,
            //    DeviceDescriptor = Library.Devices.Midi.Motör49Main,
            //    SubscriptionDescriptor = new SubscriptionDescriptor(),
            //};
            //IOW.Instance.SubscribeOutput(subReq);
            //IOW.Instance.SetOutputstate(subReq, Library.Bindings.Midi.Notes.CH1CMinus2, 127);
            //IOW.Instance.SetOutputstate(subReq, Library.Bindings.Midi.ControlChange.MotorSliderF1, short.MaxValue);

            //var tobiiTester = new TobiiTester("Gaze Point X", Library.Devices.Tobii.GazePoint);

            #region Interception

            //IOW.Instance.SetDetectionMode(DetectionMode.Bind, Library.Providers.Interception, Library.Devices.Interception.DellKeyboard1, BindModeHandler);
            //IOW.Instance.SetDetectionMode(DetectionMode.Subscription, Library.Providers.Interception, Library.Devices.Interception.DellKeyboard1);
            //var interceptionKeyboardInputTester = new InterceptionKeyboardInputTester();
            //IOW.Instance.SetDetectionMode(DetectionMode.Subscription, Library.Providers.Interception, Library.Devices.Interception.DellKeyboard1);
            //var interceptionMouseInputTester = new InterceptionMouseInputTester();
            //Console.WriteLine("Press ENTER to unsubscribe...");
            //Console.ReadLine();
            //interceptionKeyboardInputTester.Dispose();
            //interceptionMouseInputTester.Dispose();
            //interceptionMouseInputTester.Unsubscribe();
            //Console.WriteLine("Press ENTER to re-subscribe...");
            //Console.ReadLine();
            //interceptionMouseInputTester.Subscribe();
            //Console.WriteLine("Press ENTER to Dispose...");
            //Console.ReadLine();
            //interceptionMouseInputTester.Dispose();
            //IOW.Instance.SetDetectionMode(DetectionMode.Bind, Library.Providers.Interception, Library.Devices.Interception.LogitechWeelMouseUSB, BindModeHandler);

            //var interceptionMouseOutputTester = new InterceptionMouseOutputTester();
            //var interceptionKeyboardOutputTester = new InterceptionKeyboardOutputTester();

            #endregion

            #region Bind Mode Testing
            //var genericStick_1 = new GenericDiTester("T16K", Library.Devices.DirectInput.T16000M);
            //var genericStick_2 = new GenericDiTester("TWCS", Library.Devices.DirectInput.TWCS);
            //while (true)
            //{
            //    Console.WriteLine("Press Enter to Unsubscribe");
            //    Console.ReadLine();
            //    genericStick_1.Unsubscribe();
            //    genericStick_2.Unsubscribe();
            //    Console.WriteLine("Unsubscribed, press Enter to re-subscribe");
            //    Console.ReadLine();
            //    genericStick_1.Subscribe();
            //    genericStick_2.Subscribe();
            //}
            //genericStick_1 = new GenericDiTester("T16K", Library.Devices.DirectInput.T16000M);
            //genericStick_2 = new GenericDiTester("TWCS", Library.Devices.DirectInput.TWCS);
            //var vj1 = new VJoyTester(1, false);
            //var vj2 = new VJoyTester(2, false);
            //var xInputPad_1 = new XiTester(1);
            //Console.WriteLine("Press Enter for Bind Mode...");
            //Console.ReadLine();
            //IOW.Instance.SetDetectionMode(DetectionMode.Bind, Library.Providers.DirectInput, Library.Devices.DirectInput.T16000M, BindModeHandler);
            //IOW.Instance.SetDetectionMode(DetectionMode.Bind, Library.Providers.XInput, Library.Devices.Console.Xb360_1, BindModeHandler);
            //IOW.Instance.SetDetectionMode(DetectionMode.Subscription, Library.Providers.XInput, Library.Devices.Console.Xb360_1);
            //genericStick_1.Unsubscribe();
            //Console.WriteLine("Press Enter to leave Bind Mode...");
            //Console.ReadLine();
            //IOW.Instance.SetDetectionMode(DetectionMode.Subscription, Library.Providers.DirectInput, Library.Devices.DirectInput.T16000M);
            #endregion

            //xInputPad_1.Unsubscribe();

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();
            IOW.Instance.Dispose();
        }

        private static void BindModeHandler(ProviderDescriptor provider, DeviceDescriptor device, BindingReport binding, short value)
        {
            Console.WriteLine($"BIND MODE: Provider: {provider.ProviderName} | Device: {device.DeviceHandle}/{device.DeviceInstance} | Binding: {binding.BindingDescriptor.Type}/{binding.BindingDescriptor.Index}/{binding.BindingDescriptor.SubIndex} | Title: {binding.Title} | Path: {binding.Path} | Value: {value}");
        }
    }
}

