using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class BlogController : Controller
    {
        public IActionResult Index()
        {
            List<Post> posts = new List<Post>();
            using (MyBlogContext context = new MyBlogContext())
            {
                posts = context.Posts
                    .Include((p) => p.Author).ToList();
            }
            return View(posts);
        }

        public IActionResult Search(String title)
        {
            List<Post> posts = new List<Post>();
            using (MyBlogContext context = new MyBlogContext())
            {
                posts = context.Posts
                    .Where(p => p.Title.ToLower().Contains(title.ToLower()))
                    .Include((p) => p.Author).ToList();
            }
            return View("/Views/Blog/Index.cshtml", posts);
        }


        public IActionResult Post(int id = 0)
        {

            Post post = null;
            using (MyBlogContext context = new MyBlogContext())
            {
                post = context.Posts.FirstOrDefault((post) => post.Id == id);
            }

            return View(post);
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddPost(Post post)
        {
            var claims = User.Claims;
            String claim = claims.ElementAt(0).Subject.Name;
            using (MyBlogContext context = new MyBlogContext())
            {

               
                try
                {
                    User author = context.Users.FirstOrDefault((user) => user.Email.Equals(claim));
                    post.Author = author;
                    post.AuthorId = author.Id;
                    post.CreateAt = DateTime.Now;
                    context.Posts.Add(post);

                    if(context.SaveChanges() > 0)
                    {
                        Console.WriteLine("Save succefully");

                    } else
                    {
                        Console.WriteLine("Save failed");

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return View("Add");
        }



        [HttpPost]
        public async Task<JsonResult> Upload(IFormFile file)
        {

            if (file != null && file.Length > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                using (var fileSrteam = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileSrteam);
                }
                return Json(new { location = "/images/" + fileName });
            }
            return Json(new { });
        }


    }
}
