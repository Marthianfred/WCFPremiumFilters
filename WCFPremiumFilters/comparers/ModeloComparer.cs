using System.Collections.Generic;
using System.Data;

namespace WCFPremiumFilters.Comparers
{
    public class ModeloComparer : IEqualityComparer<DataRow>
    {
        public bool Equals(DataRow x, DataRow y)
        {
            return x.Field<int>("Id_RefMk") == y.Field<int>("Id_RefMk");
        }

        public int GetHashCode(DataRow obj)
        {
            return obj.Field<int>("Id_RefMk").GetHashCode();
        }
    }
}