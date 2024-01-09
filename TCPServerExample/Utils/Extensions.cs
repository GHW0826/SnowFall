using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPServerExample.DB;

namespace TCPServerExample.Utils
{
    public static class Extensions
    {
        public static bool SaveChangesEx(this AppDbContext db)
        {
            try
            {
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
