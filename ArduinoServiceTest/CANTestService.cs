using System;
using System.Text;
using Chetch.Arduino;
using Chetch.Arduino.Boards;
using Chetch.Arduino.Devices;
using Chetch.Arduino.Devices.Comms;
using Chetch.Arduino.Services;
using Chetch.Messaging;
using Chetch.Utilities;
using XmppDotNet.Xmpp.Avatar;
using XmppDotNet.Xmpp.Base;

namespace ArduinoServiceTest;

public class CANTestService : CANBusService<CANTestService>
{
    public const String COMMAND_SHOW_LOG = "show-log";
    public const String COMMAND_PAUSE = "pause";

    public const String COMMAND_RESUME = "resume";

    public const String COMMAND_SHOW_ANOMALIES = "show-ans";

    public const String COMMAND_SHOW_ANOMALY = "show-an";

    public const String COMMAND_SHOW_MESSAGE_DATA = "show-md";

    public const String COMMAND_SHOW_MESSAGE_COUNTS = "show-counts";

    public const String COMMAND_SHOW_UNEXPECTED_DATA = "show-unx";

    public const String COMMAND_SHOW_STATUS_FLAG_CHANGES = "show-sfg";

    public const String COMMAND_SHOW_ERROR_FLAG_CHANGES = "show-efg";

    public const int REMOTE_NODES = 3; //change this depending on size of bus

    public class ReportData
    {
        public byte NodeID = 0;

        //Tag 1
        byte ErrorUnknownSend = 0;
        byte ErrorFailTX = 0;
        byte ErrorTXBusy = 0;
        byte ErrorDebugAssert = 0;
        
        //Tag 2
        byte ErrorUnknownReceive = 0;
        byte ErrorReadFail = 0;
        byte ErrorMessageFormat = 0;
        UInt32 FirstUnequalXValue = 0;
        UInt32 FirstUnequalXTime = 0;
        
        //Tag 3
        UInt32 FirstUnequalValue = 0;
        UInt32 FirstUnequalTime = 0;

        //Tag 4
        public byte NodeCausingError = 0;
        byte BitTrace1 = 0;
        byte BitTrace2 = 0;
        UInt32 CanID = 0;

        //Tag 5
        byte DiffEqualRepeatErrorCount = 0;
        byte DiffEqualResentErrorCount = 0;
        byte DiffEqualUnknownErrorCount = 0;
        byte DiffNotEqualErrorCount = 0;

        //Tag 6
        UInt32 SentMessages = 0;
        UInt32 ReceivedMessages = 0;

        //Tag 7
        byte NodeMaxIdle = 0;
        UInt32 MaxIdle = 0;

        public DateTime CompletedOn;

        public bool Complete { get; internal set; } = false;

        public ReportData(byte nodeID)
        {
            NodeID = nodeID;
        }

        public bool Read(ArduinoMessage msg)
        {
            if (Complete) return false;

            if (msg.Tag == 1)
            {
                ErrorUnknownSend = msg.Get<byte>(0);
                ErrorFailTX = msg.Get<byte>(1);
                ErrorTXBusy = msg.Get<byte>(2);
                ErrorDebugAssert = msg.Get<byte>(3);

            }
            else if (msg.Tag == 2)
            {
                /*ErrorUnknownReceive = msg.Get<byte>(0);
                ErrorReadFail = msg.Get<byte>(1);
                ErrorMessageFormat = msg.Get<byte>(2);
                FirstUnequalXValue = msg.Get<UInt32>(3);*/

                FirstUnequalXValue = msg.Get<UInt32>(0);
                FirstUnequalXTime = msg.Get<UInt32>(1);
            }
            else if (msg.Tag == 3)
            {
                FirstUnequalValue = msg.Get<UInt32>(0);
                FirstUnequalTime = msg.Get<UInt32>(1);
            }
            else if (msg.Tag == 4)
            {
                NodeCausingError = msg.Get<byte>(0);
                BitTrace1 = msg.Get<byte>(1);
                BitTrace2 = msg.Get<byte>(2);
                CanID = msg.Get<UInt32>(3);
            }
            else if (msg.Tag == 5)
            {
                DiffEqualRepeatErrorCount = msg.Get<byte>(0);
                DiffEqualResentErrorCount = msg.Get<byte>(1);
                DiffEqualUnknownErrorCount = msg.Get<byte>(2);
                DiffNotEqualErrorCount = msg.Get<byte>(3);
            }
            else if (msg.Tag == 6)
            {
                SentMessages = msg.Get<UInt32>(0);
                ReceivedMessages = msg.Get<UInt32>(1);
            }
            else if (msg.Tag == 7)
            {
                NodeMaxIdle = msg.Get<byte>(0);
                MaxIdle = msg.Get<UInt32>(1);

                //Here we are complete
                Complete = true;
                CompletedOn = DateTime.Now;
            }

            return true;
        }

        public void Clear()
        {
            Complete = false;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Report for Node {0} @ {1}", NodeID, CompletedOn.ToString("s"));
            sb.AppendLine();
            sb.AppendFormat(" - Send Errors UKS/TX/BZ/DA: {0} {1} {2} {3}", ErrorUnknownSend, ErrorFailTX, ErrorTXBusy, ErrorDebugAssert);
            sb.AppendLine();
            //sb.AppendFormat(" - Recv and other Errors UKR/RF/MF: {0} {1} {2}", ErrorUnknownReceive, ErrorReadFail, ErrorMessageFormat);
            //sb.AppendLine();
            sb.AppendFormat(" - UnequalError: Node {0} -> {1} {2} / {3} {4}", NodeCausingError, FirstUnequalXValue, FirstUnequalXTime, FirstUnequalValue, FirstUnequalTime);
            sb.AppendLine();
            sb.AppendFormat(" - BitTraces and Can ID: {0} {1} {2}", Chetch.Utilities.Convert.ToBitString(BitTrace1), Chetch.Utilities.Convert.ToBitString(BitTrace2), Chetch.Utilities.Convert.ToBitString(CanID));
            sb.AppendLine();
            sb.AppendFormat(" - DiffEqualRepeatError: {0}", DiffEqualRepeatErrorCount);
            sb.AppendLine();
            sb.AppendFormat(" - DiffEqualResentError: {0}", DiffEqualResentErrorCount);
            sb.AppendLine();
            sb.AppendFormat(" - DiffEqualUnknownError: {0}", DiffEqualUnknownErrorCount);
            sb.AppendLine();
            sb.AppendFormat(" - DiffNotEqualError: {0}", DiffNotEqualErrorCount);
            sb.AppendLine();
            sb.AppendFormat(" - Sent/Received: {0} {1}", SentMessages, ReceivedMessages);
            sb.AppendLine();
            sb.AppendFormat(" - MaxIdle: {0} {1}", NodeMaxIdle, MaxIdle);
            sb.AppendLine();

            return sb.ToString();
        }

        public String ToString(String sw)
        {
            StringBuilder sb = new StringBuilder();
            switch (sw)
            {
                case "s":
                case "S":

                    return sb.ToString();
            }

            return String.Empty;
        }
    }

    public class MessageData
    {
        public enum Status
        {
            NOT_SET = 0,
            OK,
            EQUAL_REPEAT,
            EQUAL_RESENT,
            EQUAL_UNKNOWN,
            NOT_EQUAL_SMALL,
            NOT_EQUAL_LARGE,

            TAG_ERROR
        }

        public Status ReadStatus = Status.NOT_SET;

        public bool IsOK => ReadStatus == Status.OK;

        public byte NodeID = 0;
        public byte Header = 0;

        public byte BitTrace = 0;

        public byte Sender = 0;

        public byte Tag = 0;

        public UInt32 Value = 0;
        public UInt32 Time = 0; //time in ms that it was sent

        public UInt32 NewValue = 0;
        public UInt32 NewTime = 0; //time in ms that it was sent

        public MessageData(byte nodeID)
        {
            NodeID = nodeID;
        }

        public MessageData(MessageData md)
        {
            NodeID = md.NodeID;
            Header = md.Header;
            BitTrace = md.BitTrace;
            Value = md.Value;
            Time = md.Time;
            NewValue = md.NewValue;
            NewTime = md.NewTime;
            ReadStatus = md.ReadStatus;
            Sender = md.Sender;
            Tag = md.Tag;
        }

        public void Read(MCP2515.BusMessageEventArgs eargs)
        {
            
            ArduinoMessage msg = eargs.Message;

            ReadStatus = Status.NOT_SET;
            Header = eargs.CanID.Header;
            BitTrace = eargs.BitTrace;
            Sender = msg.Sender;
            Tag = msg.Tag;
            
            NewValue = msg.Get<UInt32>(0);
            NewTime = msg.Get<UInt32>(1);

            if (NewValue == Value + 1)
            {
                Value = NewValue;
                Time = NewTime;

                //So value is ok, we check if there is a Tag issue
                if (msg.Tag == (Value % 8))
                {
                    ReadStatus = Status.OK;
                }
                else
                {
                    ReadStatus = Status.TAG_ERROR;
                }
            }
            else
            {
                if (NewValue != Value)
                {
                    var diff = Math.Abs(NewValue - Value);
                    ReadStatus = diff > 255 ? Status.NOT_EQUAL_LARGE : Status.NOT_EQUAL_SMALL;
                }
                else if (NewValue == Value)
                {
                    if (NewTime == Time)
                    {
                        ReadStatus = Status.EQUAL_REPEAT; //repeat or reflection or something
                    }
                    else if (NewTime > Time)
                    {
                        ReadStatus = Status.EQUAL_RESENT; //a later time sent so this is indiciative of a resend
                    }
                    else
                    {
                        ReadStatus = Status.EQUAL_UNKNOWN; //Time is less ... weird
                    }
                }
            }
        }
        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Message Data for Node {0} status {1}", NodeID, ReadStatus);
            sb.AppendLine();
            sb.AppendFormat(" - Header {0}", Chetch.Utilities.Convert.ToBitString(Header));
            sb.AppendLine();
            sb.AppendFormat(" - BitTrace {0}", Chetch.Utilities.Convert.ToBitString(BitTrace));
            sb.AppendLine();
            sb.AppendFormat(" - Sender/Tag: {0}/{1}", Sender, Tag);
            sb.AppendLine();
            sb.AppendFormat(" - Value: {0} {1}", Value, NewValue);
            sb.AppendLine();
            sb.AppendFormat(" - Time: {0} {1}", Time, NewTime);
            sb.AppendLine();
            return sb.ToString();
        }
    }

    public class DataAnomaly
    {
        public byte NodeID => EventArgs.NodeID;

        public int Count = 0;

        MCP2515.BusMessageEventArgs EventArgs;
        MessageData Data;

        public DataAnomaly(MCP2515.BusMessageEventArgs eargs, MessageData data)
        {
            EventArgs = eargs;
            Data = data;
            Count = 1;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("CAN data for message from node {0}", NodeID);
            sb.AppendLine();
            sb.AppendFormat(" - CanID: {0}", Chetch.Utilities.Convert.ToBitString(EventArgs.CanID.ID));
            sb.AppendLine();
            sb.AppendFormat(" - CanID.Header: {0}", Chetch.Utilities.Convert.ToBitString(EventArgs.CanID.Header));
            sb.AppendLine();
            sb.AppendFormat(" - CanDLC: {0}", EventArgs.CanDLC);
            sb.AppendLine();
            sb.AppendFormat(" - BitTrace: {0}", Chetch.Utilities.Convert.ToBitString(EventArgs.BitTrace));
            sb.AppendLine();
            sb.AppendLine(" - CanData: ");
            for(int i = 0; i < EventArgs.CanData.Count; i++){
                sb.AppendFormat(" -- {0}: {1}", i, Chetch.Utilities.Convert.ToBitString(EventArgs.CanData[i]));
                sb.AppendLine();
            }
            sb.AppendLine();
            sb.Append(Data.ToString());
            sb.AppendLine();
            sb.AppendFormat("Count: {0}", Count);
            sb.AppendLine();

            return sb.ToString();
        }
    }

    public CANBusMonitor BusMonitor { get; } //For easy access

    RingBuffer<ReportData> log = new RingBuffer<ReportData>(100, true);
    Dictionary<byte, ReportData> reportData = new Dictionary<byte, ReportData>();

    Dictionary<byte, MessageData> recvMessageData = new Dictionary<byte, MessageData>();
    Dictionary<byte, UInt32> recvMessageCounts = new Dictionary<byte, UInt32>();

    Dictionary<String, DataAnomaly> anomalies = new Dictionary<String, DataAnomaly>();

    RingBuffer<MessageData> messageData = new RingBuffer<MessageData>(100, true);

    List<MCP2515.FlagsChangedEventArgs> statusFlagChanges = new List<MCP2515.FlagsChangedEventArgs>();

    List<MCP2515.FlagsChangedEventArgs> errorFlagChanges = new List<MCP2515.FlagsChangedEventArgs>();

    List<ReportData> unexpectedReportData = new List<ReportData>();

    public CANTestService(ILogger<CANTestService> Logger) : base(Logger)
    {
        BusMonitor = new CANBusMonitor(REMOTE_NODES);

        //capture messages and add them to the log
        BusMonitor.BusMessageReceived += (sender, eargs) =>
        {
            var msg = eargs.Message;

            //Console.WriteLine("Bus message node {0} type {1} tag {2}", eargs.NodeID, eargs.Message.Type, eargs.Message.Tag);
            if (msg.Type == MessageType.INFO)
            {
                try
                {
                    if (!reportData.ContainsKey(eargs.NodeID))
                    {
                        reportData[eargs.NodeID] = new ReportData(eargs.NodeID);
                    }
                    //Console.WriteLine("Message from {0} with tag {1}", eargs.NodeID, msg.Tag);
                    var rd = reportData[eargs.NodeID];
                    rd.Read(msg);
                    if (rd.Complete)
                    {
                        //Console.WriteLine(rd.ToString());
                        reportData.Remove(rd.NodeID);
                        log.Add(rd);

                        if (rd.NodeCausingError > 0)
                        {
                            unexpectedReportData.Add(rd);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else if (msg.Type == MessageType.DATA)
            {
                if (!recvMessageData.ContainsKey(eargs.NodeID))
                {
                    recvMessageData[eargs.NodeID] = new MessageData(eargs.NodeID);
                    recvMessageCounts[eargs.NodeID] = 0;
                }
                recvMessageCounts[eargs.NodeID]++;
                var md = recvMessageData[eargs.NodeID];
                md.Read(eargs);

                messageData.Add(new MessageData(md));
                //Console.WriteLine(recvMessageData[eargs.NodeID].ToString());
                if (!md.IsOK)
                {
                    String key = String.Format("N{0}-{1}", eargs.NodeID, md.ReadStatus);
                    if (!anomalies.ContainsKey(key))
                    {
                        anomalies[key] = new DataAnomaly(eargs, new MessageData(md));
                    }
                    else
                    {
                        anomalies[key].Count++;
                    }
                    //Console.WriteLine(anomalies[messageData.ReadStatus].ToString());
                }
            }
        };

        BusMonitor.MasterNode.StatusFlagsChanged += (sender, eargs) =>
        {
            statusFlagChanges.Add(eargs);
        };

        BusMonitor.MasterNode.ErrorFlagsChanged += (sender, eargs) =>
        {
            errorFlagChanges.Add(eargs);
        };

        //Add the bus monitor to the service
        AddBusMonitor(BusMonitor);
    }

    #region Service Lifecycle
    protected override Task Execute(CancellationToken stoppingToken)
    {
        return base.Execute(stoppingToken);
    }
    #endregion


    #region Client issued Command handling and general Messaging
    protected override void AddCommands()
    {
        AddCommand(COMMAND_SHOW_LOG, "Show <n?> items from log, if no number is given will show last item");
        AddCommand(COMMAND_SHOW_ANOMALIES, "Show <n?> last message data anomalises");
        AddCommand(COMMAND_SHOW_ANOMALY, "Show anomaly with <id>");
        AddCommand(COMMAND_SHOW_MESSAGE_DATA, "Show <n?> last messages");
        AddCommand(COMMAND_PAUSE, "Pause the current test");
        AddCommand(COMMAND_RESUME, "Resume the current test");
        AddCommand(COMMAND_SHOW_MESSAGE_COUNTS, "Show received message counts");
        AddCommand(COMMAND_SHOW_UNEXPECTED_DATA, "Show list of unexpected report data");
        AddCommand(COMMAND_SHOW_STATUS_FLAG_CHANGES, "Show status flag changes");
        AddCommand(COMMAND_SHOW_ERROR_FLAG_CHANGES, "Show error flag changes");
        
        base.AddCommands();
    }

    protected override bool HandleCommandReceived(ServiceCommand command, List<object> arguments, Chetch.Messaging.Message response)
    {
        int n;
        int c;

        switch (command.Command)
        {
            case COMMAND_SHOW_LOG:
                n = arguments.Count > 0 ? System.Convert.ToInt32(arguments[0].ToString()) : 1;
                c = 0;
                foreach (var rd in log)
                {
                    response.AddValue("E" + c, rd.ToString());
                    c++;
                    if (c == n) break;
                }
                return true;

            case COMMAND_SHOW_MESSAGE_DATA:
                n = arguments.Count > 0 ? System.Convert.ToInt32(arguments[0].ToString()) : 1;
                c = 0;
                foreach (var md in messageData)
                {
                    response.AddValue("M" + c, md.ToString());
                    c++;
                    if (c == n) break;
                }
                return true;

            case COMMAND_SHOW_MESSAGE_COUNTS:
                StringBuilder sb = new StringBuilder();
                foreach (var kv in recvMessageCounts)
                {
                    sb.AppendFormat("N{0}={1}", kv.Key, kv.Value);
                    sb.AppendLine();
                }
                response.AddValue("MessageCounts", sb.ToString());
                return true;

            case COMMAND_SHOW_ANOMALIES:
                n = arguments.Count > 0 ? System.Convert.ToInt32(arguments[0].ToString()) : 1;
                c = 0;
                foreach (var ad in anomalies.Values)
                {
                    response.AddValue("A" + c, ad.ToString());
                    c++;
                    if (c == n) break;
                }
                return true;

            case COMMAND_SHOW_UNEXPECTED_DATA:
                n = arguments.Count > 0 ? System.Convert.ToInt32(arguments[0].ToString()) : 1;
                c = 0;
                foreach (var unx in unexpectedReportData)
                {
                    response.AddValue("X" + c, unx.ToString());
                    c++;
                    if (c == n) break;
                }
                return true;

            case COMMAND_SHOW_STATUS_FLAG_CHANGES:
                n = arguments.Count > 0 ? System.Convert.ToInt32(arguments[0].ToString()) : 1;
                c = 0;
                foreach (var fc in statusFlagChanges)
                {
                    response.AddValue("SF" + c, String.Format("Value: {0}, Changes: {1}", Chetch.Utilities.Convert.ToBitString(fc.Flags), Chetch.Utilities.Convert.ToBitString(fc.FlagsChanged)));
                    c++;
                    if (c == n) break;
                }
                return true;

            case COMMAND_SHOW_ERROR_FLAG_CHANGES:
                n = arguments.Count > 0 ? System.Convert.ToInt32(arguments[0].ToString()) : 1;
                c = 0;
                foreach (var fc in errorFlagChanges)
                {
                    response.AddValue("EF" + c, String.Format("Value: {0}, Changes: {1}", Chetch.Utilities.Convert.ToBitString(fc.Flags), Chetch.Utilities.Convert.ToBitString(fc.FlagsChanged)));
                    c++;
                    if (c == n) break;
                }
                return true;

            case COMMAND_PAUSE:
                BusMonitor.MCPNode.SendCommand(ArduinoDevice.DeviceCommand.PAUSE);
                return true;

            case COMMAND_RESUME:
                BusMonitor.MCPNode.SendCommand(ArduinoDevice.DeviceCommand.RESUME);
                return true;

            default:
                return base.HandleCommandReceived(command, arguments, response);
        }
    }

    protected override void PopulateStatusResponse(Chetch.Messaging.Message response)
    {
        StatusDetails["MasterNode"] = "Todo";
        StatusDetails["Log"] = String.Format("Contains {0} entries", log.Count);
        StatusDetails["Anomalies"] = String.Format("Contains {0} items", anomalies.Count);
        StatusDetails["MessageData"] = String.Format("Contains {0} items", messageData.Count);
        StatusDetails["UnexpectedData"] = String.Format("Contains {0} items", unexpectedReportData.Count);
        StatusDetails["StatusFlagChanges"] = String.Format("Contains {0} items", statusFlagChanges.Count);
        StatusDetails["ErrorFlagChanges"] = String.Format("Contains {0} items", errorFlagChanges.Count);
        base.PopulateStatusResponse(response);
    }
    #endregion
}