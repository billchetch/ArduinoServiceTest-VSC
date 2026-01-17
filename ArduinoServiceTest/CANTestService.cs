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
    public const String COMMAND_START = "start";
    public const String COMMAND_STOP = "stop";

    public const int REMOTE_NODES = 3; //change this depending on size of bus

    public CANBusMonitor BusMonitor { get; } //For easy access

    public SwitchGroup Switches { get; } = new SwitchGroup("switches");


    public CANTestService(ILogger<CANTestService> Logger) : base(Logger)
    {
        BusMonitor = new CANBusMonitor(REMOTE_NODES);

        //Add stuff to nodes
        var allnNodes = BusMonitor.GetAllNodes();
        foreach(var node in allnNodes)
        {
            var sw = new ActiveSwitch("sw" + node.NodeID);
            sw.Switched += (sender, on) =>
            {
                Logger.LogInformation("Switch {0} on: {1}", sw.SID, on);
            };
            Switches.Add(sw);
            ((ArduinoBoard)node).AddDevice(sw);
        }

        //Add the bus monitor to the service
        AddBusMonitor(BusMonitor);
    }

    #region Service Lifecycle
    protected override Task Execute(CancellationToken stoppingToken)
    {
        return base.Execute(stoppingToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        Switches.TurnOff();
        return base.StopAsync(cancellationToken);
    }
    #endregion


    #region Client issued Command handling and general Messaging
    protected override void AddCommands()
    {
        AddCommand(COMMAND_START, "Start bus message test on <?node>");
        AddCommand(COMMAND_STOP, "Stop bus message test on <?node>");
        base.AddCommands();
    }

    protected override bool HandleCommandReceived(ServiceCommand command, List<object> arguments, Chetch.Messaging.Message response)
    {
        byte nodeID = 0;
        switch (command.Command)
        {
            case COMMAND_START:
                if (arguments.Count > 0)
                {
                    nodeID = System.Convert.ToByte(arguments[0].ToString());
                }
                if(nodeID == 0)
                {
                    Switches.TurnOn();
                } 
                else
                {
                    var sw = (ActiveSwitch)Switches[nodeID - 1];
                    sw.TurnOn();
                }
                return true;

            case COMMAND_STOP:
                if (arguments.Count > 0)
                {
                    nodeID = System.Convert.ToByte(arguments[0].ToString());
                }
                if(nodeID == 0)
                {
                    Switches.TurnOff();
                } 
                else
                {
                    var sw = (ActiveSwitch)Switches[nodeID - 1];
                    sw.TurnOff();
                }
                return true;
            
            default:
                return base.HandleCommandReceived(command, arguments, response);
        }
    }
    #endregion
}