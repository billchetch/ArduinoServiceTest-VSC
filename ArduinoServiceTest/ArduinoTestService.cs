using Chetch.Arduino;
using Chetch.Arduino.Devices;

namespace ArduinoServiceTest;

public class ArduinoServiceTest : ArduinoService<ArduinoServiceTest>
{
    public ArduinoServiceTest(ILogger<ArduinoServiceTest> Logger) : base(Logger)
    {

    }

    #region Service Lifecycle
    protected override Task Execute(CancellationToken stoppingToken)
    {
        ArduinoBoard board = new ArduinoBoard("first", 0x7523, 9600); //, Frame.FrameSchema.SMALL_NO_CHECKSUM);
        /*board.Ready += (sender, ready) => {
            Console.WriteLine("Board is ready: {0}", ready);
        };*/

        var device = new Ticker("testDevice01");
        board.AddDevice(device);

        AddBoard(board);

        return base.Execute(stoppingToken);
    }
    #endregion
}
