using System;
using Chetch.Arduino;
using Chetch.Arduino.Boards;
using Chetch.Arduino.Devices;
using Chetch.Arduino.Services;

namespace ArduinoServiceTest;

public class CANTestService : CANBusService<CANTestService>
{
    public CANTestService(ILogger<CANTestService> Logger) : base(Logger)
    {
        
    }

    #region Service Lifecycle
    protected override Task Execute(CancellationToken stoppingToken)
    {
        

        return base.Execute(stoppingToken);
    }
    #endregion
}