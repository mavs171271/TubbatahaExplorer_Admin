using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VisualHead.Areas.Identity.Data;
using VisualHead.Models;
using VisualHead.Areas.Identity.Data;
using VisualHead.Models;
using Microsoft.AspNetCore.Authorization;

namespace VisualHead.Controllers
{
    [Authorize]
    public class UserProfilesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserProfilesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UserProfiles
        public async Task<IActionResult> Index(string searchQuery, int page = 1, int pageSize = 10)
        {
            // Fetch user profiles
            var userProfilesQuery = _context.UserProfiles
                .Include(u => u.ApplicationUser)
                .AsQueryable();

            // Apply search filter if searchQuery is provided
            if (!string.IsNullOrEmpty(searchQuery))
            {
                userProfilesQuery = userProfilesQuery.Where(u =>
                    u.ApplicationUser.FirstName.Contains(searchQuery) ||
                    u.ApplicationUser.LastName.Contains(searchQuery));
            }

            // Count the total number of items (for pagination)
            var totalItems = await userProfilesQuery.CountAsync();

            // Fetch the specific page of data
            var userProfiles = await userProfilesQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Pass data to ViewBag for pagination
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.SearchQuery = searchQuery;

            return View(userProfiles);
        }


        // GET: UserProfiles/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userProfile = await _context.UserProfiles
                .Include(u => u.ApplicationUser)
                .FirstOrDefaultAsync(m => m.UniqueUserName == id);
            if (userProfile == null)
            {
                return NotFound();
            }

            return View(userProfile);
        }

        // GET: UserProfiles/Create
        public IActionResult Create()
        {
            ViewData["ApplicationUserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: UserProfiles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UniqueUserName,ApplicationUserId,Type")] UserProfile userProfile)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userProfile);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ApplicationUserId"] = new SelectList(_context.Users, "Id", "Id", userProfile.ApplicationUserId);
            return View(userProfile);
        }

        // GET: UserProfiles/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userProfile = await _context.UserProfiles.FindAsync(id);
            if (userProfile == null)
            {
                return NotFound();
            }
            ViewData["ApplicationUserId"] = new SelectList(_context.Users, "Id", "Id", userProfile.ApplicationUserId);
            return View(userProfile);
        }

        // POST: UserProfiles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: UserProfiles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserProfile userProfile)
        {
            if (id != userProfile.UniqueUserName)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Retrieve the existing UserProfile with its associated ApplicationUser
                    var existingUserProfile = await _context.UserProfiles
                        .Include(up => up.ApplicationUser)
                        .FirstOrDefaultAsync(up => up.UniqueUserName == id);

                    if (existingUserProfile == null)
                    {
                        return NotFound();
                    }

                    // Update ApplicationUser properties
                    existingUserProfile.ApplicationUser.FirstName = userProfile.ApplicationUser.FirstName;
                    existingUserProfile.ApplicationUser.LastName = userProfile.ApplicationUser.LastName;
                    existingUserProfile.ApplicationUser.PhoneNumber = userProfile.ApplicationUser.PhoneNumber;

                    // Update UserProfile properties
                    existingUserProfile.Type = userProfile.Type;
                    existingUserProfile.ApplicationUserId = userProfile.ApplicationUserId;

                    // Save changes to the database
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserProfileExists(userProfile.UniqueUserName))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ApplicationUserId"] = new SelectList(_context.Users, "Id", "Id", userProfile.ApplicationUserId);
            return View(userProfile);
        }


        // GET: UserProfiles/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userProfile = await _context.UserProfiles
                .Include(u => u.ApplicationUser)
                .FirstOrDefaultAsync(m => m.UniqueUserName == id);
            if (userProfile == null)
            {
                return NotFound();
            }

            return View(userProfile);
        }

        // POST: UserProfiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            // Find the UserProfile based on the unique user name (id).
            var userProfile = await _context.UserProfiles
                .Include(u => u.ApplicationUser) // Include the ApplicationUser relationship
                .FirstOrDefaultAsync(m => m.UniqueUserName == id);

            if (userProfile == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // Retrieve the associated ApplicationUser.
            var user = await _context.Users.FindAsync(userProfile.ApplicationUserId);

            // Remove the UserProfile.
            _context.UserProfiles.Remove(userProfile);

            // Remove the ApplicationUser if found.
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            // Save changes to the database.
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool UserProfileExists(string id)
        {
            return _context.UserProfiles.Any(e => e.UniqueUserName == id);
        }
    }
}
