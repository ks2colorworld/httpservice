using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HttpService
{
    public class PathComparer : IEqualityComparer<string>
    {
        public bool Equals([AllowNull] string x, [AllowNull] string y)
        {
            return x.Equals(y, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode([DisallowNull] string obj)
        {
            return obj.GetHashCode(StringComparison.OrdinalIgnoreCase);
        }
    }
}
