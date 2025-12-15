using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SUPFLY.Data;
using SUPFLY.Models;
using SUPFLY.Areas.Identity.Data; // REQUIRED: For SUPFLYUser

namespace SUPFLY.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        // CHANGED: Generic IdentityUser to SUPFLYUser
        private readonly UserManager<SUPFLYUser> _userManager;
        private readonly SignInManager<SUPFLYUser> _signInManager;
        private readonly ApplicationDbContext _context;

        // CHANGED: Generic IdentityUser to SUPFLYUser
        public IndexModel(
            UserManager<SUPFLYUser> userManager,
            SignInManager<SUPFLYUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Required]
            [StringLength(100)]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [StringLength(100)]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [StringLength(50)]
            [Display(Name = "Passport Number")]
            public string PassportNumber { get; set; }
        }

        [BindProperty]
        public InputModel Input { get; set; }

        // CHANGED: Generic IdentityUser to SUPFLYUser
        private async Task LoadAsync(SUPFLYUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var userId = await _userManager.GetUserIdAsync(user);

            Username = userName;

            var passenger = await _context.Passengers.FirstOrDefaultAsync(p => p.UserId == userId);

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                FirstName = passenger?.FirstName,
                LastName = passenger?.LastName,
                PassportNumber = passenger?.PassportNumber
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // CHANGED: Generic IdentityUser to SUPFLYUser (in the GetUserAsync call)
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // CHANGED: Generic IdentityUser to SUPFLYUser (in the GetUserAsync call)
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            // CORE LOGIC ADDED: Update or Create Passenger Record
            var userId = await _userManager.GetUserIdAsync(user);
            var passenger = await _context.Passengers.FirstOrDefaultAsync(p => p.UserId == userId);

            if (passenger == null)
            {
                passenger = new Passenger
                {
                    UserId = userId,
                    Email = user.Email,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    PassportNumber = Input.PassportNumber
                };
                _context.Passengers.Add(passenger);
            }
            else
            {
                passenger.FirstName = Input.FirstName;
                passenger.LastName = Input.LastName;
                passenger.PassportNumber = Input.PassportNumber;
                _context.Passengers.Update(passenger);
            }

            await _context.SaveChangesAsync();

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}