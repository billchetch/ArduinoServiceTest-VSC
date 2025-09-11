using System;
using Chetch.Arduino;
using Chetch.Arduino.Boards;
using Chetch.Arduino.Devices;
using Chetch.Arduino.Services;
using Chetch.Messaging;

namespace ArduinoServiceTest;

public class CANTestService : CANBusService<CANTestService>
{
    public const int REMOTE_NODES = 3;
    public CANBusMonitor BusMonitor { get; }

    public CANTestService(ILogger<CANTestService> Logger) : base(Logger)
    {
        BusMonitor = new CANBusMonitor(REMOTE_NODES);

        AddBusMonitor(BusMonitor);
    }

    #region Service Lifecycle
    protected override Task Execute(CancellationToken stoppingToken)
    {


        return base.Execute(stoppingToken);
    }
    #endregion


    #region Client issued Command handling
    protected override void AddCommands()
    {
        base.AddCommands();
    }

    protected override bool HandleCommandReceived(ServiceCommand command, List<object> arguments, Message response)
    {
        switch (command)
        {
            default:
                return base.HandleCommandReceived(command, arguments, response);
        }
    }
    #endregion
}