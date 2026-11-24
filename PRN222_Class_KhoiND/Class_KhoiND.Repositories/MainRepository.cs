using Class_KhoiND.Repositories.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Class_KhoiND.Repositories
{
    
    public class MainRepository : GenericRepository<Item>
    {
        public MainRepository() { }
        public MainRepository(PRN222_PE01_2Context context) {
            _context = context;
        }

        // PRN222_PE01_2Context la DBContext
        // Item là tên entity  bảng chính
        // Items là tên entity bảng chính thêm chữ s
        // Group là tên entity bảng phụ cấp 1 nằm trong bảng chính
        // BPC2 là tên entity bảng phụ cấp 2 nằm trong bảng phụ cấp 1
        // cái gì liên quan đến _context.   là Tên entity  thêm chữ s

        // ItemId là tên khóa chính của bảng chính
        public async Task<List<Item>> GetAllAsync()
        {
            var items = await _context.Items
                .Include(p => p.Group)

                //     trường hợp đề cho phân cấp 2  
                //    .ThenInclude(b => b.BPC2)
                // trường hợp đề cho hai phân cấp 1 
                //.Include(p => p.Group)
                .ToListAsync();

            return items ?? new List<Item>();
        }

        public async Task<Item> GetByIdAsync(int id)
        {
            var items = await _context.Items
                  .Include(p => p.Group)


                  //     trường hợp đề cho phân cấp 2  
                  //    .ThenInclude(b => b.BPC2)
                  // trường hợp đề cho hai phân cấp 1 
                  //.Include(p => p.Group)
                  .FirstOrDefaultAsync(p => p.ItemId == id);
            return items ?? new Item();
        }


        public async Task<List<Item>> SearchAsync(int? groupId)
        {
            return await _context.Items
                .AsNoTracking()
                .Include(i => i.Group)
                .Where(i => groupId == null || i.GroupId == groupId
                // && (string.IsNullOrEmpty(condition2) || i.CONDITION2.Contains(condition2))
                // && (string.IsNullOrEmpty(condition3) || i.CONDITION3 == condition3)
                )
                .OrderBy(i => i.ItemId)
                .ToListAsync();
        }

        // Group là kiểu truyền vào  
        // V1 là tên cột trong bảng  

        //public async Task<List<Item>> SearchAsync(string Group, decimal? BPC2 //string condition3)
        //{
        //    var listItem = await _context.Items
        //        .Include(b => b.Group)
        //        //    .ThenInclude(b => b.)
        //        //.Include(p => p.)
        //        .Where(c =>
        //        (c.V1 == Group || Group == 0 || Group == null)

        //        // &&  (c..Contains() || string.IsNullOrEmpty())
        //        && (c.Group.V2.Contains(BPC2) || string.IsNullOrEmpty(BPC2))  //Ref table: foreign key                

        //        )                 // thường sẽ chấm cột date để sắp xếp
        //        .OrderByDescending(c => c.).ToListAsync();

        //    return listItem ?? new List<Item>();
        //}


    }
}
