using HttpService.Orm.Sample.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HttpService.Orm.Sample.Data.Configurations
{
    public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
    {
        /// <summary>
        /// Attachment 테이블의 정의
        /// </summary>
        /// <param name="builder"></param>
        public void Configure(EntityTypeBuilder<Attachment> builder)
        {
            builder.ToTable("Attachment", "kesso");

            builder.HasKey(x => x.Attachment_key);

            builder.Property(x => x.Attachment_key)
                //.HasColumnName(nameof(Attachment.Attackment_key)) // 컬럼 이름과 필드 이름이 다른 경우
                .IsRequired()
                .HasMaxLength(18)
                ;
            builder.Property(x => x.Attachment_gubun)
                .IsRequired()
                .HasMaxLength(18)
                ;
            builder.Property(x => x.Attachment_detail_code)
                .IsRequired()
                .HasMaxLength(18)
                ;
            builder.Property(x => x.File_name)
                .IsRequired()
                .HasMaxLength(300)
                ;
            builder.Property(x => x.File_format)
                .IsRequired(false)
                .HasMaxLength(10)
                ;

            builder.Property(x => x.File_size)
                .IsRequired(false)
                ;
            builder.Property(x => x.Thumbnail_path)
                .IsRequired(false)
                .HasMaxLength(1000)
                ;

            builder.Property(x => x.Note)
                .IsRequired(false)
                .HasMaxLength(400)
                ;
            builder.Property(x => x.Operator_key)
                .IsRequired(false)
                .HasMaxLength(18)
                ;
            builder.Property(x => x.Operator_ip)
                .IsRequired(false)
                .HasMaxLength(15)
                ;

            builder.Property(x => x.Input_datetime)
                .IsRequired(false)
                ;
        }
    }
}
