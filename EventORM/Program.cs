using EventORM.Context;
using System;

namespace EventORM
{
    class Program
    {
        static void Main(string[] args)
        {
            var db = new ApplicationContext();
            //db.SetupDatabase();

            var id = db.Teachers.GetNextId();
            Console.WriteLine(id);
            Console.ReadKey();
        }
    }
}