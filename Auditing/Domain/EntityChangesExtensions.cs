using System.Collections.Generic;
using System.Linq;
using Shared.Application.Exceptions;

namespace Auditing.Domain
{
    public static class EntityChangesExtensions
    {
        public static IEnumerable<EntityChangeProperty> GetPropertyNamesIgnoringDuplicates(this IEnumerable<EntityChange> entityChanges)
        {
            return entityChanges.Select(ec => 
                    {
                        var propName = ec.Field.Split(".")[0];

                        if (!ec.Field.Contains($"{propName}.")) //case Plain
                        {
                            return new EntityChangeProperty
                            {
                                Name = ec.Field.Split(".")[0],
                                Type = EntityChangePropertyType.Plain
                            };
                        }                    
                        
                        if (ec.Field.Contains($"{propName}.")) //case nested
                        {
                            if (propName.Contains("[") && propName.Contains("]")) //case NestedArray
                            {
                                return new EntityChangeProperty
                                {
                                    Name = propName.Split("[").First(),
                                    Type = EntityChangePropertyType.NestedArray,
                                };
                            }

                            return new EntityChangeProperty //case NestedObject
                            {
                                Name = propName,
                                Type = EntityChangePropertyType.NestedObject
                            };
                        }

                        throw new InternalServerException("An error has occurred trying to determinate 'EntityChangeProperty'");
            }).GroupBy(ecp => ecp.Name).Select(g => g.First());
        }
    }
}