using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MindVault.Api.Models;

namespace MindVault.Api.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts) : base(opts) { }

        public DbSet<Note> Notes { get; set; }
        public DbSet<NoteLink> NoteLinks { get; set; }

    }
}
