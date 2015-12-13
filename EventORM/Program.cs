using EventORM.Context;
using EventORM.Model;
using System;
using System.Linq;

namespace EventORM
{
    class Program
    {
        static void Main(string[] args)
        {
            var db = new ApplicationContext();
            db.SetupDatabase();
            db.Teachers.Add(new Teacher() { Name = "TEST" });
            var t = db.Teachers.First();
            var id = db.Teachers.GetNextId();
            Console.WriteLine("ID: {0}, Name: {1}", t.TeacherId, t.Name);
            Console.WriteLine("Next ID: {0}", id);
            Console.ReadKey();
        }
    }
}