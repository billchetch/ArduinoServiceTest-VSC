using System;
using Chetch.Arduino;
using Chetch.Arduino.Boards;
using Chetch.Arduino.Devices;
using Chetch.Arduino.Services;

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
}