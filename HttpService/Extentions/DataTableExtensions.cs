using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace HttpService
{
    public static class DataTableExtensions
    {
        /// <summary>
        /// 데이터 테이블을 다이나믹 객체 열거형으로 변환합니다.
        /// </summary>
        /// <param name="table">데이터 테이블</param>
        /// <returns>다이나믹 객체 열거형</returns>
        public static IEnumerable<dynamic> ToDynamicEnumerable(this DataTable table)
        {
            return table.AsEnumerable().Select(row => new DynamicRow(row));
        }

        private sealed class DynamicRow : DynamicObject
        {
            private readonly DataRow row;

            internal DynamicRow(DataRow row)
            {
                this.row = row;
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                var hasField = row.Table.Columns.Contains(binder.Name);
                
                result = hasField ? row[binder.Name] : null;

                return hasField;
            }
        }
    }
}
