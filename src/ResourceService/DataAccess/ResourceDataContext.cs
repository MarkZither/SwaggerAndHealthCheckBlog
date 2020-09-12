using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResourceService.DataAccess
{
    public class ResourceDataContext : DbContext
    {
        public ResourceDataContext(DbContextOptions<ResourceDataContext> options)
            : base(options) { }
    }
}
