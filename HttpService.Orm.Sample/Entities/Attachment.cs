using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HttpService.Orm.Sample.Entities
{
    /// <summary>
    /// Attachment 테이블과 맵핑될 객체
    /// </summary>
    public class Attachment
    {
        public string Attachment_key { get; set; }

        public string Attachment_gubun { get; set; }

        public string Attachment_detail_code { get; set; }

        public string File_name { get; set; }

        public string File_format { get; set; }

        public long? File_size { get; set; }

        public string Thumbnail_path { get; set; }

        public string Note { get; set; }

        public string Operator_key { get; set; }

        public string Operator_ip { get; set; }

        public DateTime? Input_datetime { get; set; }
    }
}
