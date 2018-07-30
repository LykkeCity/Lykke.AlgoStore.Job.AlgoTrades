using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.AlgoStore.Service.AlgoTrades
{
    public class NullableTypeSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema model, SchemaFilterContext context)
        {
            RemoveNullableTypesFromRequiredSection(model);
            AddAttributeToNullableTypes(model, context);
        }

        private void RemoveNullableTypesFromRequiredSection(Schema model)
        {
            if (model.Required != null && model.Properties != null)
            {
                foreach (var prop in model.Properties)
                {
                    var schema = prop.Value;
                    if (schema.Extensions != null && schema.Extensions.ContainsKey("x-nullable") && (bool)schema.Extensions["x-nullable"] == true)
                    {
                        model.Required.Remove(prop.Key);
                    }
                }
            }
        }

        private void AddAttributeToNullableTypes(Schema model, SchemaFilterContext context)
        {
            if (context.SystemType.IsValueType && Nullable.GetUnderlyingType(context.SystemType) != null)
            {
                model.Extensions.Add("x-nullable", true);
            }
        }
    }
}
