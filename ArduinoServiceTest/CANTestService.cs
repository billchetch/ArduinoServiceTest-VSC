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

    public const int REMOTE_NODES = 3;

    public class ReportData
    {
        public byte NodeID = 0;

        byte MaxDiffNode = 0;
        byte MaxDiff = 0;
        byte MaxIdleNode = 0;
        UInt32 MaxIdle = 0;

        byte DiffErrorNode = 0;
        byte DiffError = 0;
        byte LoopTime = 0;
        UInt32 MaxLoopTime = 0;

        UInt32 SentMessages = 0;
        UInt32 ReceivedMessages = 0;

        int tagCount = 0;

        public DateTime CompletedOn;

        public bool Complete => tagCount == 3;

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
                MaxDiff = msg.Get<byte>(1);
                MaxIdleNode = msg.Get<byte>(2);
                MaxIdle = msg.Get<UInt32>(3);
                tagCount++;
            }
            else if (msg.Tag == 2)
            {
                DiffErrorNode = msg.Get<byte>(0);
                DiffError = msg.Get<byte>(1);
                LoopTime = msg.Get<byte>(2);
                MaxLoopTime = msg.Get<UInt32>(3);

                tagCount++;
            }
            else if (msg.Tag == 3)
            {
                SentMessages = msg.Get<UInt32>(0);
                ReceivedMessages = msg.Get<UInt32>(1);
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
            sb.AppendFormat(" - DiffError: Node {0} -> {1}", DiffErrorNode, DiffError);
            sb.AppendLine();
            sb.AppendFormat(" - Loop: Last={0} Max={1}", LoopTime, MaxLoopTime);
            sb.AppendLine();
            sb.AppendFormat(" - Sent/Received: {0} {1}", SentMessages, ReceivedMessages);
            sb.AppendLine();

            return sb.ToString();
        }
    }

    public CANBusMonitor BusMonitor { get; } //For easy access

    RingBuffer<ReportData> log = new RingBuffer<ReportData>(100);
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
                        Console.WriteLine(rd.ToString());
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
        base.AddCommands();
    }

    protected override bool HandleCommandReceived(ServiceCommand command, List<object> arguments, Message response)
    {
        switch (command.Command)
        {
            case COMMAND_SHOW_LOG:

                return true;

            default:
                return base.HandleCommandReceived(command, arguments, response);
        }
    }

    protected override void PopulateStatusResponse(Message response)
    {
        StatusDetails["MasterNode"] = "Todo";
        base.PopulateStatusResponse(response);
    }
    #endregion
}