using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplicationWithIdentity.Models;

namespace WebApplicationWithIdentity.DAL
{
    public class ChatContext : DbContext
    {
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public ChatContext(DbContextOptions<ChatContext> options)
            :base(options)
        {
            Database.EnsureCreated();
        }
    }
}
