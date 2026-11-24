using Class_KhoiND.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Class_KhoiND.Repositories
{
    public class SecondaryRepository : GenericRepository<LookupGroup>
    {
        public SecondaryRepository() { }
        public SecondaryRepository(PRN222_PE01_2Context context)
        {
            _context = context;
        }
    }
}
