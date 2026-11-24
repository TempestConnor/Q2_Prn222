using Class_KhoiND.Repositories;
using Class_KhoiND.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Class_KhoiND.Services
{
    public class SecondaryService
    {
        //LookupGroup tên entity bảng phụ
        //RPB repository của bảng phụ
        //NLTS tên service của bảng phụ ( contructor)

        private readonly SecondaryRepository _repository;

        public SecondaryService() => _repository = new SecondaryRepository();


        public async Task<List<LookupGroup>> GetAllAsync()
        {
            try
            {
                var items = await _repository.GetAllAsync();
                return items;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
