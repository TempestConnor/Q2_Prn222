# CRUD templates: Razor Pages + Services + Repositories (.NET 8, EF Core)

Layout assumed: `Repositories` project (GenericRepository, models, DbContext) -> `Services` project -> Razor Pages web project. Same as your repo.

## 0. Code words (replace with Match Case ON)

| Token | Replace with | Example (this practice paper) |
|---|---|---|
| `Items` | main entity DbSet name (plural) | `Items` |
| `Item` | main entity class | `Item` |
| `BPs` | secondary entity DbSet name (plural) | `LookupGroups` |
| `LookupGroup` | secondary entity class | `LookupGroup` |
| `MAIN_PK` | main entity primary key property | `ItemId` |
| `MAIN_NAME` | main entity text property (trimmed/required) | `Name` |
| `MAIN_FK` | main entity foreign key to secondary | `GroupId` |
| `MAIN_NAV` | main entity navigation property to secondary | `Group` |
| `MAIN_NUM` | main entity decimal property | `Amount` |
| `MAIN_DATE` | main entity DateOnly property | `EventDate` |
| `MAIN_STATUS` | main entity status string property | `Status` |
| `SEC_PK` | secondary entity primary key property | `GroupId` |
| `SEC_NAME` | secondary entity display property | `Name` |
| `CTX` | DbContext class | `PRN222_PE01_2Context` |
| `NS_REPO` | repositories namespace | `Class_KhoiND.Repositories` |
| `NS_SVC` | services namespace | `Class_KhoiND.Services` |
| `NS_PAGES` | PageModel namespace | `PRN222_Class_KhoiND.Pages_Equipment` |
| `Items` | Pages folder = URL segment | `Equipment` |
| `FILTER_PARAM` | query-string name of the filter `<select>` | `groupId` |

### Replace order (Ctrl+Shift+H, Replace in Files, Match Case ON)

1. All tokens containing an underscore, any order: `MAIN_PK`, `MAIN_NAME`, `MAIN_FK`, `MAIN_NAV`, `MAIN_NUM`, `MAIN_DATE`, `MAIN_STATUS`, `SEC_PK`, `SEC_NAME`, `NS_REPO`, `NS_SVC`, `NS_PAGES`, `Items`, `FILTER_PARAM`
2. `CTX`
3. `Items`, then `BPs` (plurals first, or `Item` would eat the start of `Items`)
4. `Item`, then `LookupGroup`

After replacing, search the solution for `MAIN_|SEC_|NS_|CTX|Items|FILTER_PARAM|Item|LookupGroup` to confirm none are left, then build.

Things to edit by hand, not by token: the rule constants and message strings at the top of the Create and Edit models, and the HTML IDs (paste the exam's own HTML and keep only the Razor expressions).

---

## 1. Repositories

### MainRepository.cs

```csharp
using NS_REPO.Models;
using Microsoft.EntityFrameworkCore;

namespace NS_REPO
{
    public class MainRepository : GenericRepository<Item>
    {
        public MainRepository() { }
        public MainRepository(CTX context) : base(context) { }

        // READ all (with the related secondary row)
        public new async Task<List<Item>> GetAllAsync()
        {
            return await _context.Items
                .Include(e => e.MAIN_NAV)
                .OrderBy(e => e.MAIN_PK)
                .ToListAsync();
        }

        // READ one. Returns null when not found (the pages turn that into 404).
        public new async Task<Item?> GetByIdAsync(int id)
        {
            return await _context.Items
                .Include(e => e.MAIN_NAV)
                .FirstOrDefaultAsync(e => e.MAIN_PK == id);
        }

        // READ filtered. filterId == null means "All". Add more optional parameters for text/date filters.
        public async Task<List<Item>> SearchAsync(int? filterId)
        {
            return await _context.Items
                .Include(e => e.MAIN_NAV)
                .Where(e => filterId == null || e.MAIN_FK == filterId)
                .OrderBy(e => e.MAIN_PK)
                .ToListAsync();
        }

        // DELETE by key. One SQL DELETE, no tracking or graph problems.
        public async Task<bool> DeleteByIdAsync(int id)
        {
            return await _context.Items
                .Where(e => e.MAIN_PK == id)
                .ExecuteDeleteAsync() > 0;
        }

        // CREATE: inherited CreateAsync(entity)
        // UPDATE: inherited UpdateAsync(entity)
    }
}
```

### SecondaryRepository.cs

```csharp
using NS_REPO.Models;
using Microsoft.EntityFrameworkCore;

namespace NS_REPO
{
    public class SecondaryRepository : GenericRepository<LookupGroup>
    {
        public SecondaryRepository() { }
        public SecondaryRepository(CTX context) : base(context) { }

        // For dropdowns: every row, including ones with no children.
        public async Task<List<LookupGroup>> GetAllOrderedAsync()
        {
            return await _context.BPs.OrderBy(e => e.SEC_NAME).ToListAsync();
        }

        // For validation: "must refer to an existing row".
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.BPs.AnyAsync(e => e.SEC_PK == id);
        }
    }
}
```

---

## 2. Services

### IMainService.cs

```csharp
using NS_REPO.Models;

namespace NS_SVC
{
    public interface IMainService
    {
        Task<List<Item>> GetAllAsync();
        Task<Item?> GetByIdAsync(int id);
        Task<List<Item>> SearchAsync(int? filterId);
        Task<int> CreateAsync(Item entity);
        Task<int> UpdateAsync(Item entity);
        Task<bool> DeleteAsync(int id);
    }
}
```

### MainService.cs

```csharp
using NS_REPO;
using NS_REPO.Models;

namespace NS_SVC
{
    public class MainService : IMainService
    {
        private readonly MainRepository _repository;

        public MainService() => _repository = new MainRepository();

        public Task<List<Item>> GetAllAsync() => _repository.GetAllAsync();

        public Task<Item?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

        public Task<List<Item>> SearchAsync(int? filterId) => _repository.SearchAsync(filterId);

        public Task<int> CreateAsync(Item entity) => _repository.CreateAsync(entity);

        public Task<int> UpdateAsync(Item entity) => _repository.UpdateAsync(entity);

        public Task<bool> DeleteAsync(int id) => _repository.DeleteByIdAsync(id);
    }
}
```

### SecondaryService.cs

```csharp
using NS_REPO;
using NS_REPO.Models;

namespace NS_SVC
{
    public class SecondaryService
    {
        private readonly SecondaryRepository _repository = new();

        public Task<List<LookupGroup>> GetAllAsync() => _repository.GetAllOrderedAsync();

        public Task<bool> ExistsAsync(int id) => _repository.ExistsAsync(id);
    }
}
```

### Program.cs (registration + start page)

No `AddDbContext` is needed, because the repositories create the context themselves.

```csharp
using NS_SVC;

builder.Services.AddScoped<IMainService, MainService>();
builder.Services.AddScoped<SecondaryService>();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddPageRoute("/Items/Index", "");   // "/" opens the list page
});
```

---

## 3. READ: list page with group filter

Files: `Pages/Items/Index.cshtml` and `Index.cshtml.cs`. The explicit route makes the page answer at `/Items/List`. If you rename the files to `List.*` instead, use a plain `@page` and `RedirectToPage("List")` everywhere.

### Index.cshtml.cs

```csharp
using NS_REPO.Models;
using NS_SVC;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NS_PAGES
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
        public int? FilterId { get; set; }

        public List<LookupGroup> BPs { get; set; } = new();
        public List<Item> Items { get; set; } = new();

        public async Task OnGetAsync()
        {
            BPs = await _secondaryService.GetAllAsync();
            Items = await _mainService.SearchAsync(FilterId);
        }
    }
}
```

### Index.cshtml

Paste the exam's `list.html` and keep its `<style>` and text. Change `@media` in the CSS to `@@media`. Replace its hardcoded options and rows with the Razor below, and fix any hardcoded URLs.

```cshtml
@page "/Items/List"
@using System.Globalization
@model NS_PAGES.IndexModel
@{
    Layout = null;
}

<a id="lnk_addNew" href="/Items/Create">Add New</a>

<form method="get" action="/Items/List">
    <select id="sl_filter" name="FILTER_PARAM">
        <option id="opt_filter_all" value="" selected="@(!Model.FilterId.HasValue)">All</option>
        @foreach (var grp in Model.BPs)
        {
            <option id="opt_filter_@grp.SEC_PK" value="@grp.SEC_PK" selected="@(Model.FilterId == grp.SEC_PK)">@grp.SEC_NAME</option>
        }
    </select>
    <button id="btn_filter" type="submit">Filter</button>
</form>

<p>Items: <span id="span_count">@Model.Items.Count</span></p>

<table id="tbl_items">
    <thead>
        <tr>
            <th>MAIN_PK</th>
            <th>MAIN_NAME</th>
            <th>MAIN_NAV</th>
            <th>MAIN_NUM</th>
            <th>MAIN_DATE</th>
            <th>MAIN_STATUS</th>
            <th></th>
        </tr>
    </thead>
    <tbody id="tbody_items">
        @foreach (var row in Model.Items)
        {
            <tr id="tr_@row.MAIN_PK">
                <td id="td_id_@row.MAIN_PK">@row.MAIN_PK</td>
                <td id="td_name_@row.MAIN_PK">@row.MAIN_NAME</td>
                <td id="td_group_@row.MAIN_PK">@row.MAIN_NAV.SEC_NAME</td>
                <td id="td_amount_@row.MAIN_PK">@row.MAIN_NUM.ToString("0.00", CultureInfo.InvariantCulture)</td>
                <td id="td_date_@row.MAIN_PK">@row.MAIN_DATE.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)</td>
                <td id="td_status_@row.MAIN_PK">@row.MAIN_STATUS</td>
                <td>
                    <a href="/Items/Details/@row.MAIN_PK">Details</a> |
                    <a href="/Items/Edit/@row.MAIN_PK">Edit</a> |
                    <a href="/Items/Delete/@row.MAIN_PK">Delete</a>
                </td>
            </tr>
        }
    </tbody>
</table>

<div id="div_empty">@(Model.Items.Count == 0 ? "No records" : "")</div>
```

Always use `CultureInfo.InvariantCulture` for numbers and dates. On a Vietnamese-locale machine the default formatting prints `10,00`.

---

## 4. READ: details page

### Details.cshtml.cs

```csharp
using NS_REPO.Models;
using NS_SVC;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NS_PAGES
{
    public class DetailsModel : PageModel
    {
        private readonly IMainService _mainService;

        public DetailsModel(IMainService mainService) => _mainService = mainService;

        public Item Entity { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var entity = await _mainService.GetByIdAsync(id);
            if (entity == null) return NotFound();

            Entity = entity;
            return Page();
        }
    }
}
```

### Details.cshtml

```cshtml
@page "/Items/Details/{id:int}"
@using System.Globalization
@model NS_PAGES.DetailsModel
@{
    Layout = null;
}

<dl>
    <dt>MAIN_PK</dt><dd>@Model.Entity.MAIN_PK</dd>
    <dt>MAIN_NAME</dt><dd>@Model.Entity.MAIN_NAME</dd>
    <dt>MAIN_NAV</dt><dd>@Model.Entity.MAIN_NAV.SEC_NAME</dd>
    <dt>MAIN_NUM</dt><dd>@Model.Entity.MAIN_NUM.ToString("0.00", CultureInfo.InvariantCulture)</dd>
    <dt>MAIN_DATE</dt><dd>@Model.Entity.MAIN_DATE.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)</dd>
    <dt>MAIN_STATUS</dt><dd>@Model.Entity.MAIN_STATUS</dd>
</dl>

<a href="/Items/Edit/@Model.Entity.MAIN_PK">Edit</a> |
<a href="/Items/List">Back to List</a>
```

---

## 5. CREATE

### Create.cshtml.cs

```csharp
using NS_REPO.Models;
using NS_SVC;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;

namespace NS_PAGES
{
    public class CreateModel : PageModel
    {
        // ---- EDIT THESE: rules and exact messages from the paper ----
        private const int NameMaxLength = 50;
        private const decimal NumMin = 0m;
        private const decimal NumMax = 1000m;
        private const int NumMaxDecimals = 2;
        private static readonly DateOnly DefaultDate = new(2026, 3, 10);
        private const string DefaultStatus = "Pending";

        private const string MsgNameRequired = "Name is required";
        private const string MsgNameTooLong = "Name must be at most 50 characters";
        private const string MsgGroupInvalid = "Group is required";
        private const string MsgNumRequired = "Amount is required";
        private const string MsgNumInvalid = "Amount must be a number";
        private const string MsgNumRange = "Amount must be between 0 and 1000";
        private const string MsgNumDecimals = "Amount can have at most 2 decimal places";
        // -------------------------------------------------------------

        private readonly IMainService _mainService;
        private readonly SecondaryService _secondaryService;

        public CreateModel(IMainService mainService, SecondaryService secondaryService)
        {
            _mainService = mainService;
            _secondaryService = secondaryService;
        }

        // Property names = the "name" attributes of the exam's form fields.
        [BindProperty] public string? MAIN_NAME { get; set; }
        [BindProperty] public int? MAIN_FK { get; set; }
        [BindProperty(Name = "MAIN_NUM")] public string? NumText { get; set; }   // string so we control the checks

        public List<LookupGroup> BPs { get; set; } = new();

        public string? Err(string key) =>
            ModelState.TryGetValue(key, out var entry) ? entry.Errors.FirstOrDefault()?.ErrorMessage : null;

        public async Task OnGetAsync() => await LoadGroupsAsync();

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadGroupsAsync();                       // needed again when the form is redisplayed
            var (name, number) = await ValidateAsync();
            if (!ModelState.IsValid) return Page();

            await _mainService.CreateAsync(new Item
            {
                MAIN_NAME = name!,
                MAIN_FK = MAIN_FK!.Value,
                MAIN_NUM = number,
                MAIN_DATE = DefaultDate,
                MAIN_STATUS = DefaultStatus
            });

            return RedirectToPage("Index");
        }

        private async Task LoadGroupsAsync() => BPs = await _secondaryService.GetAllAsync();

        private async Task<(string? Name, decimal Number)> ValidateAsync()
        {
            var name = MAIN_NAME?.Trim();
            if (string.IsNullOrEmpty(name))
                ModelState.AddModelError("MAIN_NAME", MsgNameRequired);
            else if (name.Length > NameMaxLength)
                ModelState.AddModelError("MAIN_NAME", MsgNameTooLong);

            if (MAIN_FK == null || !await _secondaryService.ExistsAsync(MAIN_FK.Value))
                ModelState.AddModelError("MAIN_FK", MsgGroupInvalid);

            decimal number = 0;
            var text = NumText?.Trim();
            if (string.IsNullOrEmpty(text))
                ModelState.AddModelError("MAIN_NUM", MsgNumRequired);
            else if (!decimal.TryParse(text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                         CultureInfo.InvariantCulture, out number))
                ModelState.AddModelError("MAIN_NUM", MsgNumInvalid);
            else if (number < NumMin || number > NumMax)
                ModelState.AddModelError("MAIN_NUM", MsgNumRange);
            else if (decimal.Round(number, NumMaxDecimals) != number)
                ModelState.AddModelError("MAIN_NUM", MsgNumDecimals);

            return (name, number);
        }
    }
}
```

### Create.cshtml

Paste the exam's `create.html`, change `@media` to `@@media`, and replace its `<form>` contents with the Razor below. No `action` attribute, so it posts back to the same page.

```cshtml
@page "/Items/Create"
@model NS_PAGES.CreateModel
@{
    Layout = null;
}

<form method="post" novalidate>
    <div>
        <label for="txt_name">MAIN_NAME</label>
        <input id="txt_name" name="MAIN_NAME" type="text" value="@Model.MAIN_NAME" />
        <span id="val_name">@Model.Err("MAIN_NAME")</span>
    </div>

    <div>
        <label for="sl_group">MAIN_NAV</label>
        <select id="sl_group" name="MAIN_FK">
            <option id="opt_group_default" value="" selected="@(!Model.MAIN_FK.HasValue)">Select group</option>
            @foreach (var grp in Model.BPs)
            {
                <option id="opt_group_@grp.SEC_PK" value="@grp.SEC_PK" selected="@(Model.MAIN_FK == grp.SEC_PK)">@grp.SEC_NAME</option>
            }
        </select>
        <span id="val_group">@Model.Err("MAIN_FK")</span>
    </div>

    <div>
        <label for="txt_amount">MAIN_NUM</label>
        <input id="txt_amount" name="MAIN_NUM" type="text" value="@Model.NumText" />
        <span id="val_amount">@Model.Err("MAIN_NUM")</span>
    </div>

    <button id="btn_save" type="submit">Save</button>
    <a id="btn_back" href="/Items/List">Back to List</a>
</form>
```

Keep the exam's input `type` and its span IDs if they differ. If the exam HTML has `min`/`max` attributes on the amount input, remove them, or the server-side rules never get tested.

---

## 6. UPDATE

### Edit.cshtml.cs

```csharp
using NS_REPO.Models;
using NS_SVC;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;

namespace NS_PAGES
{
    public class EditModel : PageModel
    {
        // ---- EDIT THESE: same rules and messages as Create ----
        private const int NameMaxLength = 50;
        private const decimal NumMin = 0m;
        private const decimal NumMax = 1000m;
        private const int NumMaxDecimals = 2;

        private const string MsgNameRequired = "Name is required";
        private const string MsgNameTooLong = "Name must be at most 50 characters";
        private const string MsgGroupInvalid = "Group is required";
        private const string MsgNumRequired = "Amount is required";
        private const string MsgNumInvalid = "Amount must be a number";
        private const string MsgNumRange = "Amount must be between 0 and 1000";
        private const string MsgNumDecimals = "Amount can have at most 2 decimal places";
        // -------------------------------------------------------

        private readonly IMainService _mainService;
        private readonly SecondaryService _secondaryService;

        public EditModel(IMainService mainService, SecondaryService secondaryService)
        {
            _mainService = mainService;
            _secondaryService = secondaryService;
        }

        [BindProperty] public int MAIN_PK { get; set; }                       // hidden field
        [BindProperty] public string? MAIN_NAME { get; set; }
        [BindProperty] public int? MAIN_FK { get; set; }
        [BindProperty(Name = "MAIN_NUM")] public string? NumText { get; set; }

        public List<LookupGroup> BPs { get; set; } = new();

        public string? Err(string key) =>
            ModelState.TryGetValue(key, out var entry) ? entry.Errors.FirstOrDefault()?.ErrorMessage : null;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var entity = await _mainService.GetByIdAsync(id);
            if (entity == null) return NotFound();

            MAIN_PK = entity.MAIN_PK;
            MAIN_NAME = entity.MAIN_NAME;
            MAIN_FK = entity.MAIN_FK;
            NumText = entity.MAIN_NUM.ToString(CultureInfo.InvariantCulture);

            await LoadGroupsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadGroupsAsync();

            var entity = await _mainService.GetByIdAsync(MAIN_PK);
            if (entity == null) return NotFound();

            var (name, number) = await ValidateAsync();
            if (!ModelState.IsValid) return Page();

            entity.MAIN_NAME = name!;
            entity.MAIN_FK = MAIN_FK!.Value;
            entity.MAIN_NUM = number;
            entity.MAIN_NAV = null!;            // avoid a stale navigation overriding the new foreign key

            await _mainService.UpdateAsync(entity);
            return RedirectToPage("Index");
        }

        private async Task LoadGroupsAsync() => BPs = await _secondaryService.GetAllAsync();

        private async Task<(string? Name, decimal Number)> ValidateAsync()
        {
            var name = MAIN_NAME?.Trim();
            if (string.IsNullOrEmpty(name))
                ModelState.AddModelError("MAIN_NAME", MsgNameRequired);
            else if (name.Length > NameMaxLength)
                ModelState.AddModelError("MAIN_NAME", MsgNameTooLong);

            if (MAIN_FK == null || !await _secondaryService.ExistsAsync(MAIN_FK.Value))
                ModelState.AddModelError("MAIN_FK", MsgGroupInvalid);

            decimal number = 0;
            var text = NumText?.Trim();
            if (string.IsNullOrEmpty(text))
                ModelState.AddModelError("MAIN_NUM", MsgNumRequired);
            else if (!decimal.TryParse(text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                         CultureInfo.InvariantCulture, out number))
                ModelState.AddModelError("MAIN_NUM", MsgNumInvalid);
            else if (number < NumMin || number > NumMax)
                ModelState.AddModelError("MAIN_NUM", MsgNumRange);
            else if (decimal.Round(number, NumMaxDecimals) != number)
                ModelState.AddModelError("MAIN_NUM", MsgNumDecimals);

            return (name, number);
        }
    }
}
```

Fields not on the form (here `MAIN_DATE` and `MAIN_STATUS`) are left untouched because the edit starts from the loaded entity.

### Edit.cshtml

```cshtml
@page "/Items/Edit/{id:int}"
@model NS_PAGES.EditModel
@{
    Layout = null;
}

<form method="post" novalidate>
    <input type="hidden" name="MAIN_PK" value="@Model.MAIN_PK" />

    <div>
        <label for="txt_name">MAIN_NAME</label>
        <input id="txt_name" name="MAIN_NAME" type="text" value="@Model.MAIN_NAME" />
        <span id="val_name">@Model.Err("MAIN_NAME")</span>
    </div>

    <div>
        <label for="sl_group">MAIN_NAV</label>
        <select id="sl_group" name="MAIN_FK">
            <option id="opt_group_default" value="" selected="@(!Model.MAIN_FK.HasValue)">Select group</option>
            @foreach (var grp in Model.BPs)
            {
                <option id="opt_group_@grp.SEC_PK" value="@grp.SEC_PK" selected="@(Model.MAIN_FK == grp.SEC_PK)">@grp.SEC_NAME</option>
            }
        </select>
        <span id="val_group">@Model.Err("MAIN_FK")</span>
    </div>

    <div>
        <label for="txt_amount">MAIN_NUM</label>
        <input id="txt_amount" name="MAIN_NUM" type="text" value="@Model.NumText" />
        <span id="val_amount">@Model.Err("MAIN_NUM")</span>
    </div>

    <button id="btn_save" type="submit">Save</button>
    <a id="btn_back" href="/Items/List">Back to List</a>
</form>
```

---

## 7. DELETE

### Delete.cshtml.cs

```csharp
using NS_REPO.Models;
using NS_SVC;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NS_PAGES
{
    public class DeleteModel : PageModel
    {
        private readonly IMainService _mainService;

        public DeleteModel(IMainService mainService) => _mainService = mainService;

        public Item Entity { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var entity = await _mainService.GetByIdAsync(id);
            if (entity == null) return NotFound();

            Entity = entity;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            await _mainService.DeleteAsync(id);
            return RedirectToPage("Index");
        }
    }
}
```

### Delete.cshtml

```cshtml
@page "/Items/Delete/{id:int}"
@model NS_PAGES.DeleteModel
@{
    Layout = null;
}

<h1>Delete</h1>
<p>Delete "@Model.Entity.MAIN_NAME" (MAIN_PK @Model.Entity.MAIN_PK)?</p>

<form method="post">
    <button id="btn_delete" type="submit">Delete</button>
    <a href="/Items/List">Cancel</a>
</form>
```

---

## 8. Adapting to other papers

- **String or Guid primary key:** change `int id` to `string id` (or `Guid`) in `IMainService`, `MainService`, `MainRepository` (`GetByIdAsync`, `DeleteByIdAsync`), and in the page signatures and routes (`{id}` instead of `{id:int}`). `SecondaryRepository.ExistsAsync` and `MAIN_FK` follow the secondary key type.
- **No secondary table:** delete the `LookupGroup`/`BPs` parts: `SecondaryRepository`, `SecondaryService`, the filter, the group dropdown, the group validation, and `.Include(e => e.MAIN_NAV)`.
- **Text search instead of a group filter:** add a `string? keyword` parameter to `SearchAsync` and a clause like `&& (string.IsNullOrEmpty(keyword) || e.MAIN_NAME.Contains(keyword))`. Bind it with `[BindProperty(SupportsGet = true)]`.
- **Different sort order:** change the `OrderBy` in `MainRepository`. The paper's list is sorted by ItemId ascending.
- **Fields the paper doesn't have:** delete the matching `MAIN_NUM`, `MAIN_DATE` or `MAIN_STATUS` lines from the views, the models and the entity initializer.
- **Existing repo bug:** the `GetByIdAsync` in your current `MainRepository` returns `new Item()` when nothing is found, so a missing id shows an empty record instead of a 404. The template returns null instead.
