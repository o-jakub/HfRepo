using System;

namespace SamplePlugin
{
    public class Plugin
    {
        public string Kuk(string input,string idJob)
        {
            Console.WriteLine($"Response: {input} : {idJob}");
            return input;
        }
    }
}
