using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace HttpService
{
    public static class DataTableExtensions
    {
        /// <summary>
        /// 데이터 테이블을 다이나믹 객체 열거형으로 변환합니다.
        /// </summary>
        /// <param name="table">데이터 테이블</param>
        /// <returns>다이나믹 객체 열거형</returns>
        public static IEnumerable<DynamicRow> ToDynamicEnumerable(this DataTable table)
        {
            return table.AsEnumerable().Select(row => new DynamicRow(row));
            
        }

        /// <summary>
        /// 데이터 테이블을 사전 열거형으로 변환합니다.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static IEnumerable< Dictionary<string, object>> ToDictionaryEnumerable(this DataTable table)
        {
            return table.Select().Select(row => GetRows(row));
        }

        private static Dictionary<string, object> GetRows(DataRow row)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            foreach (DataColumn col in row.Table.Columns)
            {
                dictionary.Add(col.ColumnName, row[col.ColumnName]);
            }

            return dictionary;
        }

      
    }

    [DataContract]
    public sealed class DynamicRow : DynamicObject, IXmlSerializable
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

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            foreach (DataColumn col in row.Table.Columns)
            {
                yield return col.ColumnName;
            }
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteXml(XmlWriter writer)
        {
            var names = GetDynamicMemberNames();
            foreach (var name in names)
            {
                writer.WriteStartElement(name);
                writer.WriteValue(row[name]);
                writer.WriteEndElement();
            }
        }
    }
}
