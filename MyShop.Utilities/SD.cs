using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Utilities
{
    public static class SD
    {
        public const string AdminRole = "Admin";
        public const string EditorRole = "Editor";

        // Order Status Constants
        public const string New = "New";
        public const string Processing = "Processing";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";

        public const string SessionKey = "ShoppingCartSession";

    }
}
