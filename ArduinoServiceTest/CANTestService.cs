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
    public const int REMOTE_NODES = 3; //change this depending on size of bus

    public CANBusMonitor BusMonitor { get; } //For easy access


    public CANTestService(ILogger<CANTestService> Logger) : base(Logger)
    {
        BusMonitor = new CANBusMonitor(REMOTE_NODES);

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
        
        base.AddCommands();
    }

    protected override bool HandleCommandReceived(ServiceCommand command, List<object> arguments, Chetch.Messaging.Message response)
    {
        switch (command.Command)
        {
            
            
            default:
                return base.HandleCommandReceived(command, arguments, response);
        }
    }
    #endregion
}