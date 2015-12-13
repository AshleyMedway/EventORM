using EventORM.Data;
using EventORM.Model;

namespace EventORM.Context
{
    public class ApplicationContext : EventContext
    {
        public RecordSet<Teacher> Teachers { get; set; }
        public RecordSet<Student> Students { get; set; }
        public RecordSet<Module> Modules { get; set; }
    }
}
