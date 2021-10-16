using System;
using OpenTK;


namespace OpenTKV3
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Game game = new Game())
            {

                game.Run(30.0,30.0);

            }
        }
    }
}
