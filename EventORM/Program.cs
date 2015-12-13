using EventORM.Context;
using EventORM.Model;
using System;

namespace EventORM
{
    class Program
    {
        static void Main(string[] args)
        {
            var db = new ApplicationContext();
            db.SetupDatabase();
            db.Teachers.Add(new Teacher() { Name = "TEST" });
            var id = db.Teachers.GetNextId();
            Console.WriteLine(id);
            Console.ReadKey();
        }
    }
}