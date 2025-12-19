using Ime.Core;

for (int i = 0; i < 10; i++)
{
    var r = ImeController.TryForceHiraganaOnce();
    Console.WriteLine($"{DateTime.Now:HH:mm:ss} {r}");
    Thread.Sleep(1000);
}
