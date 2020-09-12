using Microsoft.EntityFrameworkCore;

namespace ResourceService.DataAccess
{
    public class ResourceDataContext : DbContext
    {
        public ResourceDataContext(DbContextOptions<ResourceDataContext> options)
            : base(options) { }
    }
}
