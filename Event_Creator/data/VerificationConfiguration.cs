using Event_Creator.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.data
{
    public class VerificationConfiguration : IEntityTypeConfiguration<Verification>
    {
        public void Configure(EntityTypeBuilder<Verification> builder)
        {
            builder.Property(x => x.usage).HasConversion<string>();
        }
    }
}
