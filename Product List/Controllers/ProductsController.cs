using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Product_List.Services;

public class ProductsController : Controller
{
    private readonly ApplicationDbContext context;
    private readonly IWebHostEnvironment environment;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment, ILogger<ProductsController> logger)
    {
        this.context = context;
        this.environment = environment;
        _logger = logger;
    }

    public IActionResult Index()
    {
        var products = context.MyProperty.OrderByDescending(p => p.Id).ToList();
        return View(products);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ProductDto productDto)
    {
        if (!ModelState.IsValid)
        {
            return View(productDto);
        }

        // Handle image file
        if (productDto.ImageFile != null)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(productDto.ImageFile.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("ImageFile", "Invalid image file type.");
                return View(productDto);
            }

            if (productDto.ImageFile.Length > 5 * 1024 * 1024) 
            {
                ModelState.AddModelError("ImageFile", "The image file size should not exceed 5MB.");
                return View(productDto);
            }

            var newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + fileExtension;
            var imageFullPath = Path.Combine(environment.WebRootPath, "images", newFileName);

            if (!Directory.Exists(Path.Combine(environment.WebRootPath, "images")))
            {
                Directory.CreateDirectory(Path.Combine(environment.WebRootPath, "images"));
            }

            using (var stream = new FileStream(imageFullPath, FileMode.Create))
            {
                productDto.ImageFile.CopyTo(stream);
            }

            productDto.ImageFileName = newFileName; 
        }
        else
        {
            productDto.ImageFileName = "default.jpg";
        }

        // Save product to the database
        try
        {
            context.Database.ExecuteSqlRaw(
                "EXEC InsertProduct @Name, @Brand, @Category, @Price, @Description, @ImageFileName, @CreatedAt",
                new SqlParameter("@Name", productDto.Name),
                new SqlParameter("@Brand", productDto.Brand),
                new SqlParameter("@Category", productDto.Category),
                new SqlParameter("@Price", productDto.Price),
                new SqlParameter("@Description", productDto.Description),
                new SqlParameter("@ImageFileName", productDto.ImageFileName),
                new SqlParameter("@CreatedAt", DateTime.Now)
            );

            TempData["SuccessMessage"] = "Product successfully created.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while saving the product.");
            ModelState.AddModelError(string.Empty, "An error occurred while saving the product.");
            return View(productDto);
        }
    }

    public IActionResult Edit(int id)
    {
        var product = context.MyProperty.Find(id);
        if (product == null)
        {
            return RedirectToAction("Index");
        }

        var productDto = new ProductDto()
        {
            Name = product.Name,
            Brand = product.Brand,
            Category = product.Category,
            Price = product.Price,
            Description = product.Description,
            ImageFileName = product.ImageFileName
        };

        ViewData["ProductId"] = product.Id;
        ViewData["ImageFileName"] = product.ImageFileName;
        ViewData["CreatedAt"] = product.CreatedAt.ToString("MM/dd/yyyy");

        return View(productDto);
    }

    [HttpPost]
    public IActionResult Edit(int id, ProductDto productDto)
    {
        var product = context.MyProperty.Find(id);
        if (product == null)
        {
            return RedirectToAction("Index");
        }

        if (!ModelState.IsValid)
        {
            ViewData["ProductId"] = product.Id;
            ViewData["ImageFileName"] = product.ImageFileName;
            ViewData["CreatedAt"] = product.CreatedAt.ToString("MM/dd/yyyy");

            return View(productDto);
        }

        if (productDto.ImageFile != null)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(productDto.ImageFile.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("ImageFile", "Invalid image file type.");
                return View(productDto);
            }

            if (productDto.ImageFile.Length > 5 * 1024 * 1024) 
            {
                ModelState.AddModelError("ImageFile", "The image file size should not exceed 5MB.");
                return View(productDto);
            }

            var newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + fileExtension;
            var imageFullPath = Path.Combine(environment.WebRootPath, "images", newFileName);

            if (!Directory.Exists(Path.Combine(environment.WebRootPath, "images")))
            {
                Directory.CreateDirectory(Path.Combine(environment.WebRootPath, "images"));
            }

            using (var stream = new FileStream(imageFullPath, FileMode.Create))
            {
                productDto.ImageFile.CopyTo(stream);
            }

            // Delete the old image if it exists
            if (product.ImageFileName != "default.jpg")
            {
                string oldImageFullPath = Path.Combine(environment.WebRootPath, "images", product.ImageFileName);
                if (System.IO.File.Exists(oldImageFullPath))
                {
                    System.IO.File.Delete(oldImageFullPath);
                }
            }

            productDto.ImageFileName = newFileName; 
        }
        else
        {
            productDto.ImageFileName = product.ImageFileName; 
        }

        product.Name = productDto.Name;
        product.Brand = productDto.Brand;
        product.Category = productDto.Category;
        product.Price = productDto.Price;
        product.Description = productDto.Description;
        product.ImageFileName = productDto.ImageFileName; 

        context.SaveChanges();

        TempData["SuccessMessage"] = "Product successfully updated.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        var product = context.MyProperty.Find(id);
        if (product == null)
        {
            return RedirectToAction("Index");
        }
        string imagePath = Path.Combine(environment.WebRootPath, "images", product.ImageFileName);


        System.IO.File.Delete(imagePath);

        context.MyProperty.Remove(product);
        context.SaveChanges(true);
        return RedirectToAction("Index", "Products");


        
    }

    

}
