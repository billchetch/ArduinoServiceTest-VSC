using System;
using System.Text;
using Chetch.Arduino;
using Chetch.Arduino.Boards;
using Chetch.Arduino.Devices;
using Chetch.Arduino.Services;
using Chetch.Messaging;
using Chetch.Utilities;

namespace ArduinoServiceTest;

public class CANTestService : CANBusService<CANTestService>
{
    public const String COMMAND_SHOW_LOG = "show-log";
    public const String COMMAND_PAUSE = "pause";

    public const String COMMAND_RESUME = "resume";

    public const int REMOTE_NODES = 3;

    public class ReportData
    {
        public byte NodeID = 0;

        //Tag 1
        byte MaxDiffNode = 0;
        UInt32 MaxDiff = 0;

        //Tag 2
        byte MaxIdleNode = 0;
        UInt32 MaxIdle = 0;

        //Tag 3
        byte TXErrorCount = 0;
        byte RXErrorCount = 0;
        byte MsgErrorCount = 0;

        //Tag 4
        /*
        msg->add(diffEqualErrorNode);
        msg->add(diffEqualError);
        msg->add(diffLessErrorNode);
        msg->add(diffLessError);
        */
        byte DiffEqualErrorNode = 0;
        byte DiffEqualErrorCount = 0;

        byte DiffLessErrorNode = 0;
        byte DiffLessErrorCount = 0;

        //Tag 5
        UInt32 SentMessages = 0;
        UInt32 ReceivedMessages = 0;

        int tagCount = 0;

        public DateTime CompletedOn;

        public bool Complete => tagCount == 6;

        public ReportData(byte nodeID)
        {
            NodeID = nodeID;
        }

        public bool Read(ArduinoMessage msg)
        {
            if (Complete) return false;

            if (msg.Tag == 1)
            {
                MaxDiffNode = msg.Get<byte>(0);
                MaxDiff = msg.Get<UInt32>(1);
                tagCount++;
            }
            else if (msg.Tag == 2)
            {
                MaxIdleNode = msg.Get<byte>(0);
                MaxIdle = msg.Get<UInt32>(1);
                tagCount++;
            }
            else if (msg.Tag == 3)
            {
                TXErrorCount = msg.Get<byte>(0);
                RXErrorCount = msg.Get<byte>(1);
                MsgErrorCount = msg.Get<byte>(2);
                tagCount++;
            }
            else if (msg.Tag == 4)
            {

                tagCount++;
            }
            else if (msg.Tag == 5)
            {
                SentMessages = msg.Get<UInt32>(0);
                ReceivedMessages = msg.Get<UInt32>(1);
                tagCount++;
            }

            if (Complete)
            {
                CompletedOn = DateTime.Now;
            }
            return true;
        }

        public void Clear()
        {
            tagCount = 0;
            MaxDiffNode = 0;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Report for Node {0} @ {1}", NodeID, CompletedOn.ToString("s"));
            sb.AppendLine();
            sb.AppendFormat(" - MaxDiff: Node {0} -> {1}", MaxDiffNode, MaxDiff);
            sb.AppendLine();
            sb.AppendFormat(" - MaxIdle: Node {0} -> {1}", MaxIdleNode, MaxIdle);
            sb.AppendLine();
            sb.AppendFormat(" - Errors TX/RX/MSG: {0} {1} {2}", TXErrorCount, RXErrorCount, MsgErrorCount);
            sb.AppendLine();
            sb.AppendFormat(" - DiffEqualError: Node {0} -> {1}", DiffEqualErrorNode, DiffEqualErrorCount);
            sb.AppendLine();
            sb.AppendFormat(" - DiffLessError: Node {0} -> {1}", DiffLessErrorNode, DiffLessErrorCount);
            sb.AppendLine();
            sb.AppendFormat(" - Sent/Received: {0} {1}", SentMessages, ReceivedMessages);
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

    public CANBusMonitor BusMonitor { get; } //For easy access

    RingBuffer<ReportData> log = new RingBuffer<ReportData>(100, true);
    Dictionary<byte, ReportData> reportData = new Dictionary<byte, ReportData>();

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
                    var rd = reportData[eargs.NodeID];
                    rd.Read(msg);
                    if (rd.Complete)
                    {
                        Console.WriteLine(rd.ToString("s"));
                        reportData.Remove(rd.NodeID);
                        log.Add(rd);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
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
        AddCommand(COMMAND_PAUSE, "Pause the current test");
        AddCommand(COMMAND_RESUME, "Resume the current test");
        base.AddCommands();
    }

    protected override bool HandleCommandReceived(ServiceCommand command, List<object> arguments, Message response)
    {
        switch (command.Command)
        {
            case COMMAND_SHOW_LOG:
                int n = arguments.Count > 0 ? System.Convert.ToInt32(arguments[0].ToString()) : 1;
                int c = 0;
                foreach (var rd in log)
                {
                    response.AddValue("E" + c, rd.ToString());
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

    protected override void PopulateStatusResponse(Message response)
    {
        StatusDetails["MasterNode"] = "Todo";
        StatusDetails["Log"] = String.Format("Contains {0} entries", log.Count);
        base.PopulateStatusResponse(response);
    }
    #endregion
}