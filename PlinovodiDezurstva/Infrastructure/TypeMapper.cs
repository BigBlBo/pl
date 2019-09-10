using Dapper;
using PlinovodiDezurstva.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PlinovodiDezurstva.Infrastructure
{
    public class TypeMapper
    {
        public static void Initialize(string @namespace)
        {
            var types = from type in typeof(DummyTypeMapperClass).GetTypeInfo().Assembly.GetTypes()
                        where type.GetTypeInfo().IsAnsiClass && type.Namespace == @namespace
                        select type;

            types.ToList().ForEach(type =>
            {
                var mapper = (SqlMapper.ITypeMap)Activator
                    .CreateInstance(typeof(ColumnAttributeTypeMapper<>)
                                    .MakeGenericType(type));
                SqlMapper.SetTypeMap(type, mapper);
            });
        }
    }
}
