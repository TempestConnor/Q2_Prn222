using Class_KhoiND.Repositories;
using Class_KhoiND.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Class_KhoiND.Services
{
    public class MainService : IMainService
    {
        //MainRepository repository của bảng chính
        //NS  tên service của bảng chính ( contructor)
        // Item là tên entity  bảng chính
        // Items là tên entity bảng chính thêm chữ s


        private readonly MainRepository _repository;
        public MainService() => _repository = new MainRepository();


        public async Task<int> CreateAsync(Item bc)
        {
            try
            {
                return await _repository.CreateAsync(bc);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var item = await _repository.GetByIdAsync(id);
                if (item != null)
                {
                    return await _repository.RemoveAsync(item);

                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return false;
        }

        public async Task<List<Item>> GetAllAsync()
        {
            try
            {
                ////Business rules here
                var items = await _repository.GetAllAsync();
                return items;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return new List<Item>();
        }

        public async Task<Item> GetByIdAsync(int id)
        {
            try
            {
                var item = await _repository.GetByIdAsync(id);
                return item;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return null;
        }

        public async Task<List<Item>> SearchAsync(int groupId)
        {
            try
            {
                var items = await _repository.SearchAsync(groupId);
                return items;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return new List<Item>();
        }

        public async Task<int> UpdateAsync(Item bc)
        {
            try
            {
                return await _repository.UpdateAsync(bc);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return 0;
        }

    }
}

