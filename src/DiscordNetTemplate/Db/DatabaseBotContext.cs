using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordNetTemplate.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordNetTemplate.Db;

public class DatabaseBotContext : DbContext
{
    public DatabaseBotContext(DbContextOptions<DatabaseBotContext> options)
    : base(options)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite();
    }
    public DbSet<UserModel> Users { get; set; }
}
