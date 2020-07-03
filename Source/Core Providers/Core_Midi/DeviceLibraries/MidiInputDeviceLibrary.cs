﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hidwizards.IOWrapper.Libraries.DeviceLibrary;
using HidWizards.IOWrapper.DataTransferObjects;
using NAudio.Midi;

namespace Core_Midi.DeviceLibraries
{
    public partial class MidiDeviceLibrary
    {
        private ProviderReport _inputProviderReport;
        private DeviceReportNode _inputDeviceReportTemplate;

        public int GetInputDeviceIdentifier(DeviceDescriptor deviceDescriptor)
        {
            if (_connectedInputDevices.TryGetValue(deviceDescriptor.DeviceHandle, out var instances) &&
                instances.Count >= deviceDescriptor.DeviceInstance)
            {
                return instances[deviceDescriptor.DeviceInstance];
            }
            throw new Exception($"Could not find input device Handle {deviceDescriptor.DeviceHandle}, Instance {deviceDescriptor.DeviceInstance}");
        }

        public ProviderReport GetInputList()
        {
            return _inputProviderReport;
        }

        public DeviceReport GetInputDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            if (!_connectedInputDevices.TryGetValue(deviceDescriptor.DeviceHandle, out var deviceInstances)
                || deviceDescriptor.DeviceInstance >= deviceInstances.Count) return null;
            var devId = deviceInstances[deviceDescriptor.DeviceInstance];
            var infoIn = MidiIn.DeviceInfo(devId);
            var deviceReport = new DeviceReport
            {
                DeviceDescriptor = deviceDescriptor,
                DeviceName = infoIn.ProductName
            };
            deviceReport.Nodes = _inputDeviceReportTemplate.Nodes;

            return deviceReport;
        }

        private void BuildInputDeviceList()
        {
            var providerReport = new ProviderReport
            {
                Title = "MIDI Input (Core)",
                Description = "Provides support for MIDI devices",
                API = "Midi",
                ProviderDescriptor = _providerDescriptor
            };
            foreach (var deviceIdList in _connectedInputDevices)
            {
                for (var i = 0; i < deviceIdList.Value.Count; i++)
                {
                    var deviceDescriptor = new DeviceDescriptor { DeviceHandle = deviceIdList.Key, DeviceInstance = i };
                    providerReport.Devices.Add(GetInputDeviceReport(deviceDescriptor));
                }

            }
            _inputProviderReport = providerReport;
        }

        private void BuildInputDeviceReportTemplate()
        {
            _bindingReports = new ConcurrentDictionary<BindingDescriptor, BindingReport>();
            var node = new DeviceReportNode();
            for (var channel = 0; channel < 16; channel++)
            {
                var channelInfo = new DeviceReportNode
                {
                    Title = $"CH {channel + 1}"
                };

                // Notes - Keys, Pads

                var notesInfo = new DeviceReportNode
                {
                    Title = "Notes"
                };
                for (var octave = -1; octave < 10; octave++)
                {
                    var octaveInfo = new DeviceReportNode
                    {
                        Title = $"Octave {octave}"
                    };
                    for (var noteIndex = 0; noteIndex < NoteNames.Length; noteIndex++)
                    {
                        if (octave == 9 && noteIndex > 7) continue; // MIDI ends at G9, Skip G# to B
                        var noteName = NoteNames[noteIndex];
                        var bd = BuildNoteDescriptor(channel, octave, noteIndex);
                        var br = new BindingReport
                        {
                            Title = $"{noteName}",
                            Path = $"CH{channel + 1} {noteName}{octave}",
                            Category = BindingCategory.Signed,
                            BindingDescriptor = bd
                        };
                        _bindingReports.TryAdd(bd, br);
                        octaveInfo.Bindings.Add(br);
                    }
                    notesInfo.Nodes.Add(octaveInfo);
                }
                channelInfo.Nodes.Add(notesInfo);

                // ControlChange (CC) - Dials, Sliders etc

                var controlChangeInfo = new DeviceReportNode
                {
                    Title = "CtrlChange"
                };
                for (var controllerId = 0; controllerId < 128; controllerId++)
                {
                    var bd = BuildControlChangeDescriptor(channel, controllerId);
                    var br = new BindingReport
                    {
                        Title = $"ID {controllerId}",
                        Path = $"CH{channel} CC{controllerId}",
                        Category = BindingCategory.Signed,
                        BindingDescriptor = bd
                    };
                    _bindingReports.TryAdd(bd, br);
                    controlChangeInfo.Bindings.Add(br);
                }
                channelInfo.Nodes.Add(controlChangeInfo);

                // Pitch Wheel
                var pwbd = new BindingDescriptor
                {
                    Index = (int) MidiCommandCode.PitchWheelChange,
                    SubIndex = 0
                };
                var pwbr = new BindingReport
                {
                    Title = "Pitch Wheel",
                    Path = $"CH{channel + 1} PW",
                    Category = BindingCategory.Signed,
                    BindingDescriptor = pwbd
                };
                _bindingReports.TryAdd(pwbd, pwbr);
                channelInfo.Bindings.Add(pwbr);

                // Add the channel
                node.Nodes.Add(channelInfo);
            }

            _inputDeviceReportTemplate = node;
        }

    }
}
