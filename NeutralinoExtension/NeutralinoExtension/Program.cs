
namespace NeutralinoExtension
{
    public class Program
    {
        static NeutralinoExt neutralino;

        static void Main(string[] args)
        {
            // -- Blocking Example -- 
            // "RunForever" blocks the app from ending until the UI is closed
            // Only reacts to events from the UI

            //neutralino = new NeutralinoExtension(true);
            //neutralino.AddEvent("test", Test);
            //neutralino.RunForever();


            // -- Non-blocking Example --
            // "Run" doesn't block the app, so you can continue to do stuff and update the UI
            // When the main code ends, the backend shuts down, so be careful with that

            neutralino = new NeutralinoExt(true);
            neutralino.Run();
            neutralino.AddEvent("test", Test);
            while (true)
            {
                neutralino.SendMessage("receivedText", DateTime.Now.ToString("HH:mm:ss"));
                Thread.Sleep(1000);
            }

        }

        private static void Test(string s)
        {
            neutralino.DebugLog("Successfully received call to function 'Test'. Sending back a message with the same parameter.");
            neutralino.SendMessage("receivedText", s);
        }
    }
}
