using System;
using InsLab.Muse;

namespace MuseLibraryDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var muse = new Muse()
            {
                DataToRead = MuseData.EEG | MuseData.Absolutes | MuseData.Relatives
            };

            Console.WriteLine($"Muse Model : {muse.Model}");
            Console.WriteLine($"Muse Name : {muse.Name}");
        }
    }
}
