using Event_Creator.models.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Event_Creator.data
{
    public class FailedLoginConfigurations : IEntityTypeConfiguration<FailedLogin>
    {
        public void Configure(EntityTypeBuilder<FailedLogin> builder)
        {
            throw new NotImplementedException();
        }
    }
}
