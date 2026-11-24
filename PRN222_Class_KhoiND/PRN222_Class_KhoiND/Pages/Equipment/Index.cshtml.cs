using Class_KhoiND.Repositories.Models;
using Class_KhoiND.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PRN222_Class_KhoiND.Pages_Equipment
{
    public class IndexModel : PageModel
    {
        private readonly IMainService _mainService;
        private readonly SecondaryService _secondaryService;

        public IndexModel(IMainService mainService, SecondaryService secondaryService)
        {
            _mainService = mainService;
            _secondaryService = secondaryService;
        }

        // Empty option value "" binds to null = "All"
        [BindProperty(SupportsGet = true, Name = "FILTER_PARAM")]
        public int FilterId { get; set; }

        public List<LookupGroup> BPs { get; set; } = new();
        public List<Item> Items { get; set; } = new();

        public async Task OnGetAsync()
        {
            BPs = await _secondaryService.GetAllAsync();
            Items = await _mainService.SearchAsync(FilterId);
        }
    }
}
