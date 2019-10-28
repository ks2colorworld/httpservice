using HttpService.Orm.Sample.Data.Configurations;
using HttpService.Orm.Sample.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HttpService.Orm.Sample.Data
{
    public class DefaultDatabaseContext:DbContext
    {
        public DefaultDatabaseContext(DbContextOptions<DefaultDatabaseContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Attachment 테이블 쿼리
        /// </summary>
        public DbSet<Attachment> Attachments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 테이블 정의
            modelBuilder.ApplyConfiguration(new AttachmentConfiguration());
        }
    }
}
