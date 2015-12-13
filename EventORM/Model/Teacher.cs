using System.Collections.Generic;

namespace EventORM.Model
{
    public class Teacher
    {
        public int TeacherId { get; set; }
        public string Name { get; set; }

        public virtual List<Module> Modules { get; set; }
    }
}
