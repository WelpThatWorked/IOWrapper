﻿using HidSharp;
using Providers;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Providers
{
    public interface IProvider : IDisposable
    {
        string ProviderName { get; }
        bool IsLive { get; }

        ProviderReport GetInputList();
        ProviderReport GetOutputList();
        DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq);
        DeviceReport GetOutputDeviceReport(OutputSubscriptionRequest subReq);

        bool SetProfileState(Guid profileGuid, bool state);
        bool SubscribeInput(InputSubscriptionRequest subReq);
        bool UnsubscribeInput(InputSubscriptionRequest subReq);
        bool SubscribeOutputDevice(OutputSubscriptionRequest subReq);
        bool UnSubscribeOutputDevice(OutputSubscriptionRequest subReq);
        //bool SetOutputButton(string dev, uint button, bool state);
        bool SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state);
        //bool SubscribeAxis(string deviceHandle, uint axisId, dynamic callback);
        void RefreshLiveState();
    }

    #region Subscription Requests
    /// <summary>
    /// Base class for Subscription Requests. Shared by Input and Output
    /// </summary>
    public class SubscriptionRequest
    {
        /// <summary>
        /// Identifies the Subscriber
        /// </summary>
        public SubscriptionDescriptor SubscriptionDescriptor { get; set; }

        /// <summary>
        /// Identifies the Provider that this subscription is for
        /// </summary>
        public ProviderDescriptor ProviderDescriptor { get; set; }

        /// <summary>
        /// Identifies which (Provider-specific) Device that this subscription is for
        /// </summary>
        public DeviceDescriptor DeviceDescriptor { get; set; }
    }

    /// <summary>
    /// Contains all the required information for :
    ///     The IOController to route the request to the appropriate Provider
    ///     The Provider to subscribe to the appropriate input
    ///     The Provider to notify the subscriber of activity
    /// </summary>
    public class InputSubscriptionRequest : SubscriptionRequest
    {
        /// <summary>
        /// Identifies the (Provider+Device-specific) Input that this subscription is for
        /// </summary>
        public BindingDescriptor BindingDescriptor { get; set; }

        /// <summary>
        /// Callback to be fired when this Input changes state
        /// </summary>
        public dynamic Callback { get; set; }

        public InputSubscriptionRequest Clone()
        {
            return (InputSubscriptionRequest)this.MemberwiseClone();
        }
    }

    /// <summary>
    /// Contains all the information for:
    ///     The IOController to route the request to the appropriate Provider
    ///     
    /// Output Subscriptions are typically used to eg create virtual devices...
    /// ... so that output can be sent to them
    /// </summary>
    public class OutputSubscriptionRequest : SubscriptionRequest {
        public OutputSubscriptionRequest Clone()
        {
            return (OutputSubscriptionRequest)this.MemberwiseClone();
        }
    }
    #endregion

    // Reports allow the back-end to tell the front-end what capabilities are available
    // Reports comprise of two parts:
    // Descriptors allow the front-end to subscribe to Bindings
    // Other meta-data allows the front-end to interpret capabilities, report style etc
    #region Reporting

    #region Provider Report

    /// <summary>
    /// Contains information about each provider
    /// </summary>
    public class ProviderReport
    {
        /// <summary>
        /// The human-friendly name of the Provider
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// A description of what the Provider does
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Contains information needed to subscribe to this Provider
        /// </summary>
        public ProviderDescriptor ProviderDescriptor { get; set; }

        /// <summary>
        /// The underlying API that handles this input
        /// It is intended that many providers could support a given API
        /// This meta-data is intended to allow the front-end to present a user with alternatives
        /// </summary>
        public string API { get; set; }

        public List<DeviceReport> Devices { get; set; } = new List<DeviceReport>();
    }

    #endregion

    #region Device Reports

    /// <summary>
    /// Contains information about each Device on a Provider
    /// </summary>
    public class DeviceReport
    {
        /// <summary>
        /// The human-friendly name of the device
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// Contains information needed to subscribe to this Device via the Provider
        /// </summary>
        public DeviceDescriptor DeviceDescriptor { get; set; }

        //public List<BindingInfo> Bindings { get; set; } = new List<BindingInfo>();

        /// <summary>
        /// Nodes give the device report structure and allow the front-end to logically group items
        /// </summary>
        public List<DeviceReportNode> Nodes { get; set; } = new List<DeviceReportNode>();
    }

    #region Binding Report

    /// <summary>
    /// Contains information about each Binding on a Device on a Provider
    /// </summary>
    public class BindingReport
    {
        /// <summary>
        /// Meta-Data for the front-end to display a Human-Readable name for the Binding
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Meta-Data to allow the front-end to interpret the Binding
        /// </summary>
        public BindingCategory Category { get; set; }

        /// <summary>
        /// Contains information needed to subscribe to this Binding
        /// </summary>
        public BindingDescriptor BindingDescriptor { get; set; }
    }
    #endregion

    /// <summary>
    /// Used as a sub-node, to logically group Bindings
    /// </summary>
    public class DeviceReportNode
    {
        /// <summary>
        /// Human-friendly name of this node
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Sub-Nodes
        /// </summary>
        public List<DeviceReportNode> Nodes { get; set; } = new List<DeviceReportNode>();

        /// <summary>
        /// Bindings contained in this node
        /// </summary>
        public List<BindingReport> Bindings { get; set; } = new List<BindingReport>();
    }

    #endregion

    #endregion

    #region Descriptors
    // Descriptors are used to identify various aspects of a Binding
    // These classes control routing of the subscription request

    /// <summary>
    /// Identifies the Provider responsible for handling the Binding
    /// </summary>
    public class ProviderDescriptor
    {
        /// <summary>
        /// The API implementation that handles this input
        /// This should be unique
        /// </summary>
        public string ProviderName { get; set; }
    }

    /// <summary>
    /// Identifies a device within a Provider
    /// </summary>
    public class DeviceDescriptor
    {
        /// <summary>
        /// A way to uniquely identify a device instance via it's API
        /// Note that ideally all providers implementing the same API should ideally generate the same device handles
        /// For something like RawInput or DirectInput, this would likely be based on VID/PID
        /// For an ordered API like XInput, this would just be controller number
        /// </summary>
        public string DeviceHandle { get; set; }

        public int DeviceInstance { get; set; }
    }

    /// <summary>
    /// Identifies a Binding within a Device
    /// </summary>
    public class BindingDescriptor
    {
        /// <summary>
        /// The Type of the Binding - ie Button / Axis / POV
        /// </summary>
        public BindingType Type { get; set; }

        /// <summary>
        /// The Type-specific Index of the Binding
        /// </summary>
        public int Index { get; set; } = 0;

        // Currently un-used. May be desirable for POV directions
        /// <summary>
        /// The Type-specific SubIndex of the Binding
        /// </summary>
        public int SubIndex { get; set; } = 0;
    }

    /// <summary>
    /// Identifies the Subscriber
    /// </summary>
    public class SubscriptionDescriptor
    {
        /// <summary>
        /// Uniquely identifies a Binding - each subscriber can only be subscribed to one input / output
        /// In an application such as UCR, each binding (GuiControl) can only be bound to one input / output
        /// </summary>
        public Guid SubscriberGuid { get; set; }

        // ToDo: Move?
        /// <summary>
        /// Allows grouping of subscriptions for easy toggling on / off sets of subscriptions
        /// </summary>
        public Guid ProfileGuid { get; set; }
    }
    #endregion

    // Enums used to categorize how a binding reports
    #region Category Enums
    
    /// <summary>
    /// Describes what kind of input or output you are trying to read or emulate
    /// </summary>
    public enum BindingType { Axis, Button, POV };

    /// <summary>
    /// Describes the reporting style of a Binding
    /// Only used for the back-end to report to the front-end how to work with the binding
    /// </summary>
    public enum BindingCategory { Momentary, Event, Signed, Unsigned, Delta }
    //public enum AxisCategory { Signed, Unsigned, Delta }
    //public enum ButtonCategory { Momentary, Event }
    //public enum POVCategory { POV1, POV2, POV3, POV4 }
    #endregion

    #region Helper Classes
    static public class DeviceHelper
    {
        static public HidDeviceLoader loader = new HidDeviceLoader();

        public static string GetDeviceName(int vid, int pid, int? ser = null)
        {
            string str = "Unknown Device";
            try
            {
                var result = loader.GetDeviceOrDefault(vid, pid, ser);
                str = result.Manufacturer;
                if (str.Length > 0)
                    str += " ";
                str += result.ProductName;
            }
            catch { };
            return str;
        }

        public static string GetDevicePath(int vid, int pid, int? ser = null)
        {
            string str = null;
            try
            {
                var result = loader.GetDeviceOrDefault(vid, pid, ser);
                str = result.DevicePath;
            }
            catch { }
            return str;
        }
    }
    #endregion

    public abstract class BindingHandler
    {
        protected Dictionary<Guid, InputSubscriptionRequest> subscriptions = new Dictionary<Guid, InputSubscriptionRequest>();
        protected BindingDescriptor bindingDescriptor;
        protected int currentState = 0;

        private static int povTolerance = 4500;
        protected int povAngle;

        public BindingHandler(BindingDescriptor descriptor)
        {
            bindingDescriptor = descriptor;
            if (bindingDescriptor.Type == BindingType.POV)
            {
                povAngle = bindingDescriptor.SubIndex * 9000;
            }
        }

        public virtual bool ProfileIsActive(Guid profileGuid)
        {
            return true;
        }

        public bool Add(InputSubscriptionRequest subReq)
        {
            var subscriberGuid = subReq.SubscriptionDescriptor.SubscriberGuid;
            if (!subscriptions.ContainsKey(subscriberGuid))
            {
                subscriptions.Add(subscriberGuid, subReq);
                return true;
            }
            return false;
        }

        public bool Remove(InputSubscriptionRequest subReq)
        {
            var subscriberGuid = subReq.SubscriptionDescriptor.SubscriberGuid;
            if (subscriptions.ContainsKey(subscriberGuid))
            {
                subscriptions.Remove(subscriberGuid);
                return true;
            }
            return false;
        }

        public abstract void ProcessPollResult(int state);

        public bool HasSubscriptions()
        {
            return subscriptions.Count > 0;
        }

        protected bool ValueMatchesAngle(int value, int angle)
        {
            if (value == -1)
                return false;
            var diff = AngleDiff(value, angle);
            return value != -1 && AngleDiff(value, angle) <= povTolerance;
        }

        private int AngleDiff(int a, int b)
        {
            var result1 = a - b;
            if (result1 < 0)
                result1 += 36000;

            var result2 = b - a;
            if (result2 < 0)
                result2 += 36000;

            return Math.Min(result1, result2);
        }
    }

    public abstract class PolledBindingHandler : BindingHandler
    {
        public PolledBindingHandler(BindingDescriptor descriptor) : base(descriptor)
        {
        }

        public override void ProcessPollResult(int state)
        {
            int reportedValue = ConvertValue(state);
            if (currentState == reportedValue)
                return;
            currentState = reportedValue;

            foreach (var subscription in subscriptions.Values)
            {
                if (ProfileIsActive(subscription.SubscriptionDescriptor.ProfileGuid))
                {
                    subscription.Callback(reportedValue);
                }
            }
        }

        public virtual int ConvertValue(int state)
        {
            return state;
        }
    }

    public abstract class StickHandler : IDisposable
    {
        protected string deviceHandle;
        protected int deviceInstance;

        protected Dictionary<int, Dictionary<int, BindingHandler>> buttonMonitors
            = new Dictionary<int, Dictionary<int, BindingHandler>>();

        protected Dictionary<int, Dictionary<int, BindingHandler>> axisMonitors
            = new Dictionary<int, Dictionary<int, BindingHandler>>();

        protected Dictionary<int, Dictionary<int, BindingHandler>> povDirectionMonitors
            = new Dictionary<int, Dictionary<int, BindingHandler>>();

        protected Dictionary<BindingType, Dictionary<int, Dictionary<int, BindingHandler>>> monitors
            = new Dictionary<BindingType, Dictionary<int, Dictionary<int, BindingHandler>>>();

        public StickHandler(InputSubscriptionRequest subReq)
        {
            monitors.Add(BindingType.Axis, axisMonitors);
            monitors.Add(BindingType.Button, buttonMonitors);
            monitors.Add(BindingType.POV, povDirectionMonitors);

            deviceHandle = subReq.DeviceDescriptor.DeviceHandle;
            deviceInstance = subReq.DeviceDescriptor.DeviceInstance;
            SetAcquireState(true);
        }

        ~StickHandler()
        {
            Dispose(true);
        }

        protected abstract void SetAcquireState(bool state);

        public abstract void Poll();
        public abstract BindingHandler CreateBindingHandler(BindingDescriptor bindingDescriptor);

        public bool Add(InputSubscriptionRequest subReq)
        {
            var bindingType = subReq.BindingDescriptor.Type;
            var bindingIndex = GetInputIdentifier(bindingType, subReq.BindingDescriptor.Index);
            var bindingSubIndex = subReq.BindingDescriptor.SubIndex;

            if (!monitors[bindingType].ContainsKey(bindingIndex))
            {
                monitors[bindingType].Add(bindingIndex, new Dictionary<int, BindingHandler>());
            }
            var monitorList = monitors[bindingType][bindingIndex];
            if (!monitorList.ContainsKey(bindingSubIndex))
            {
                monitorList.Add(bindingSubIndex, CreateBindingHandler(subReq.BindingDescriptor));
            }
            return monitorList[bindingSubIndex].Add(subReq);
        }

        public bool Remove(InputSubscriptionRequest subReq)
        {
            var monitorList = monitors[subReq.BindingDescriptor.Type][subReq.DeviceDescriptor.DeviceInstance];
            var inputId = GetInputIdentifier(subReq.BindingDescriptor.Type, subReq.BindingDescriptor.Index);
            if (monitorList.ContainsKey(inputId))
            {
                var ret = monitorList[inputId].Remove(subReq);
                if (!monitorList[inputId].HasSubscriptions())
                {
                    monitorList.Remove(inputId);
                }
                return ret;
            }
            return false;
        }

        public abstract int GetInputIdentifier(BindingType bindingType, int bindingIndex);

        public bool HasSubscriptions()
        {
            foreach (var monitorList in monitors.Values)
            {
                foreach (var monitorInstance in monitorList.Values)
                {
                    foreach (var monitor in monitorInstance.Values)
                    {
                        if (monitor.HasSubscriptions())
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        #region IDisposable Members
        bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                SetAcquireState(false);
            }
            disposed = true;
        }
        #endregion
    }

    public abstract class PollManager<T> : IDisposable
    {
        // The thread which handles input detection
        protected Thread pollThread;
        // Is the thread currently running? This is set by the thread itself.
        protected volatile bool pollThreadRunning = false;
        // Do we want the thread to be on or off?
        // This is independent of whether or not the thread is running...
        // ... for example, we may be updating bindings, so the thread may be temporarily stopped
        protected bool pollThreadDesired = false;
        // Is the thread in an Active or Inactive state?
        protected bool pollThreadActive = false;

        protected Dictionary<T, StickHandler> MonitoredSticks = new Dictionary<T, StickHandler>();

        public abstract T GetMonitorKey(DeviceDescriptor descriptor);

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            var prev_state = pollThreadActive;
            if (pollThreadActive)
                SetPollThreadState(false);

            var monitorId = GetMonitorKey(subReq.DeviceDescriptor);
            if (!MonitoredSticks.ContainsKey(monitorId))
            {
                MonitoredSticks.Add(monitorId, CreateHandler(subReq));
            }
            var result = MonitoredSticks[monitorId].Add(subReq);
            if (result || prev_state)
            {
                SetPollThreadState(true);
                return true;
            }
            return false;
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            var prev_state = pollThreadActive;
            if (pollThreadActive)
                SetPollThreadState(false);

            bool ret = false;
            var monitorId = GetMonitorKey(subReq.DeviceDescriptor);
            if (MonitoredSticks.ContainsKey(monitorId))
            {
                // Remove from monitor lookup table
                MonitoredSticks[monitorId].Remove(subReq);
                // If this was the last thing monitored on this stick...
                ///...remove the stick from the monitor lookup table
                if (!MonitoredSticks[monitorId].HasSubscriptions())
                {
                    MonitoredSticks.Remove(monitorId);
                }
                ret = true;
            }
            if (prev_state)
            {
                SetPollThreadState(true);
            }
            return ret;
        }

        public abstract StickHandler CreateHandler(InputSubscriptionRequest subReq);

        public void SetPollThreadState(bool state)
        {
            if (state && !pollThreadRunning)
            {
                pollThread = new Thread(PollThread);
                pollThread.Start();
                while (!pollThreadRunning)
                {
                    Thread.Sleep(10);
                }
            }

            if (!pollThreadRunning)
                return;

            if (state && !pollThreadActive)
            {
                pollThreadDesired = true;
                while (!pollThreadActive)
                {
                    Thread.Sleep(10);
                }
                //Log("PollThread for {0} Activated", ProviderName);
            }
            else if (!state && pollThreadActive)
            {
                pollThreadDesired = false;
                while (pollThreadActive)
                {
                    Thread.Sleep(10);
                }
                //Log("PollThread for {0} De-Activated", ProviderName);
            }
        }

        private void PollThread()
        {
            pollThreadRunning = true;
            //Log("Started PollThread for {0}", ProviderName);
            while (true)
            {
                if (pollThreadDesired)
                {
                    pollThreadActive = true;
                    while (pollThreadDesired)
                    {
                        foreach (var monitoredStick in MonitoredSticks)
                        {
                            monitoredStick.Value.Poll();
                        }
                        Thread.Sleep(1);
                    }
                }
                else
                {
                    pollThreadActive = false;
                    while (!pollThreadDesired)
                    {
                        Thread.Sleep(1);
                    }
                }
            }
        }

        #region IDisposable
        bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                pollThread.Abort();
                pollThreadRunning = false;
                //Log("Stopped PollThread for {0}", ProviderName);
                foreach (var stick in MonitoredSticks.Values)
                {
                    //stick.Dispose();
                }
                MonitoredSticks = null;
            }
            disposed = true;
            //Log("Provider {0} was Disposed", ProviderName);
        }

        #endregion
    }
}
