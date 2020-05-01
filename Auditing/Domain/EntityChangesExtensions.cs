using System.Collections.Generic;
using System.Linq;

namespace Auditing.Domain
{
    public static class EntityChangesExtensions
    {
        public static IEnumerable<string> GetNotDuplicatedPropertyNames(this IEnumerable<EntityChange> entityChanges)
        {
            return entityChanges.Select(
                ec => ec.Field.Split(".")[0]
            ).Distinct();
        }
        
        public static bool IsNestedObject(this IEnumerable<EntityChange> entityChanges, string property)
        {
            return entityChanges.Any(ec => ec.Field.Contains($"{property}."));
        }
    }
}