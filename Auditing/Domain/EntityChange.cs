namespace Auditing.Domain
{
    public class EntityChange
    {
        public string Field { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}