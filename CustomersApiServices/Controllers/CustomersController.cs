using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CustomersApiServices.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Customers")]
    public class CustomersController : Controller
    {
        // GET: api/Customers
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "Catcher Wong", "James Li" };
        }

        // GET: api/Customers/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return $"Catcher Wong - {id}";
        }
        
        // POST: api/Customers
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }
        
        // PUT: api/Customers/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
