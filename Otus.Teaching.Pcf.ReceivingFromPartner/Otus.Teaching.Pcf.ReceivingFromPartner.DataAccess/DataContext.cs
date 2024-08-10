using Microsoft.EntityFrameworkCore;
using Otus.Teaching.Pcf.ReceivingFromPartner.Core.Domain;
using Otus.Teaching.Pcf.ReceivingFromPartner.DataAccess.Data;
using System;

namespace Otus.Teaching.Pcf.ReceivingFromPartner.DataAccess
{
    public class DataContext
        : DbContext
    {

        public DbSet<Partner> Partners { get; set; }

        
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}