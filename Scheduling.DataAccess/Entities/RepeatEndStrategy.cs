using System;

namespace Scheduling.DataAccess.Entities
{
    public class RepeatEndStrategy
    {
        public static RepeatEndStrategy NotUsed = new RepeatEndStrategy(0, "Not Used");
        public static RepeatEndStrategy Never = new RepeatEndStrategy(1, "Never");
        public static RepeatEndStrategy OnEndDate = new RepeatEndStrategy(2, "On End Date");
        public static RepeatEndStrategy AfterOccurrenceNumber = new RepeatEndStrategy(3, "After Occurrence Number");

        public int Id { get; set; }
        public string Name { get; set; }

        protected RepeatEndStrategy()
        {
        }

        public RepeatEndStrategy(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}