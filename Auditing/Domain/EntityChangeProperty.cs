namespace Auditing.Domain
{
    public class EntityChangeProperty
    {
        public string Name { get; set; }
        public EntityChangePropertyType Type { get; set; }
    }
}