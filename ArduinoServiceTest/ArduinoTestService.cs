using Chetch.Arduino;
using Chetch.Arduino.Devices;

namespace ArduinoServiceTest;

public class ArduinoServiceTest : ArduinoService<ArduinoServiceTest>
{
    public ArduinoServiceTest(ILogger<ArduinoServiceTest> Logger) : base(Logger)
    {
        ArduinoBoard board = new ArduinoBoard("test"); //, Frame.FrameSchema.SMALL_NO_CHECKSUM);
        board.Ready += (sender, ready) => {
            Console.WriteLine("Board is ready: {0}", ready);
            if(ready)
            {
                Console.WriteLine("Board millis: {0}", board. Millis);
                Console.WriteLine("Devices: {0}", board. DeviceCount);
                Console.WriteLine("Free memory: {0}", board.FreeMemory);
            }
        };

        var ticker = new Ticker(10, "ticker1");
        //board.AddDevice(ticker);

        var sd = new PassiveSwitch("psw1");
        sd.Switched += (sender, pinState) => {
            Console.WriteLine("Switch {0} has pin state {1}", sd.SID, pinState);
        };
        board.AddDevice(sd);

        AddBoard(board);
    }

    #region Service Lifecycle
    protected override Task Execute(CancellationToken stoppingToken)
    {
        

        return base.Execute(stoppingToken);
    }
    #endregion
}
