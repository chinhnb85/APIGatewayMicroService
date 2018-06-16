using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Authorization;

namespace CustomersApiServices.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ImageController : Controller
    {        
        public IActionResult BannerImage()
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), "MyStaticFiles", "images", "banner1.svg");

            return PhysicalFile(file, "image/svg+xml");
        }
    }
}
