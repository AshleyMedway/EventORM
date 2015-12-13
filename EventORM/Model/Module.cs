using System.Collections.Generic;

namespace EventORM.Model
{
    public class Module
    {
        public int ModuleId { get; set; }
        public string Name { get; set; }

        public int TecherId { get; set; }
        public virtual Teacher Teacher { get; set; }

        public virtual List<Student> Students { get; set; }
    }
}
