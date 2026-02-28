using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Enums
{
    namespace Common.Enums
    {
        public static class RoleNames
        {
            public const string Customer = "Customer";
            public const string Employee = "Employee";
        };

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum Roles : byte
        {
            Customer = 0,
            Employee,
            Admin
        }

    }
}
