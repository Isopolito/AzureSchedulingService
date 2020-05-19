namespace Scheduling.DataAccess.Entities
{
    public class RepeatInterval
    {
        public static RepeatInterval NotUsed = new RepeatInterval(0, "Not Used");
        public static RepeatInterval Never = new RepeatInterval(1, "Never");
        public static RepeatInterval Daily = new RepeatInterval(2, "Daily");
        public static RepeatInterval Weekly = new RepeatInterval(3, "Weekly");
        public static RepeatInterval BiMonthly = new RepeatInterval(4, "Bi-Monthly");
        public static RepeatInterval Monthly = new RepeatInterval(5, "Monthly");
        public static RepeatInterval Quarterly = new RepeatInterval(6, "Quarterly");

        public int Id { get; set; }
        public string Name { get; set; }

        public RepeatInterval(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}