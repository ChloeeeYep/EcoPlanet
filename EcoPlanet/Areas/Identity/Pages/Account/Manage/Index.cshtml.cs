// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using EcoPlanet.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CodeFixes;

namespace EcoPlanet.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<EcoPlanetUser> _userManager;
        private readonly SignInManager<EcoPlanetUser> _signInManager;

        public IndexModel(
            UserManager<EcoPlanetUser> userManager,
            SignInManager<EcoPlanetUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Phone]
            [StringLength(11, ErrorMessage = "Please Enter Your Phone Number", MinimumLength = 10)]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "Please Provide Your Full Name Before Submitting The Form")]
            [StringLength(256,ErrorMessage = "Please Enter Your Name Between 5 - 30 Characters", MinimumLength = 5)]
            [Display(Name = "Your Full Name")] //label

            public string fullname { get; set; }


            [Required]
            [Display(Name = "Your DOB")]
            [DataType(DataType.Date)]

            public DateTime DoB { get; set; }


            [DataType(DataType.MultilineText)]
            [Display(Name = "Your Address")]

            public string Address { get; set; }

            public char UserType { get; set; }

            public string Id { get; set; }
        }

        private async Task LoadAsync(EcoPlanetUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                Id = user.Id,
                PhoneNumber = phoneNumber,
                fullname = user.FullName,
                DoB = user.DOB,
                Address = user.Address,
                UserType = user.UserType
            };
        }

        public async Task<IActionResult> OnGetAsync(string userId = null)
        {
            EcoPlanetUser userToEdit = null;

            // Load the user specified by userId, or the current logged-in user if userId is not provided
            if (!string.IsNullOrEmpty(userId))
            {
                userToEdit = await _userManager.FindByIdAsync(userId);
            }
            else
            {
                userToEdit = await _userManager.GetUserAsync(User);
            }

            if (userToEdit == null)
            {
                return NotFound($"Unable to load user with ID '{userId ?? _userManager.GetUserId(User)}'.");
            }

            await LoadAsync(userToEdit);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string userId = null)
        {
            // Get the ID of the user being edited or the current logged-in user if no ID is provided
            var userToEdit = !string.IsNullOrEmpty(userId) ? await _userManager.FindByIdAsync(userId) : await _userManager.GetUserAsync(User);

            if (userToEdit == null)
            {
                return NotFound($"Unable to load user with ID '{userId ?? _userManager.GetUserId(User)}'.");
            }

            // Retrieve the UserType of the currently logged-in user
            var currentUserType = (await _userManager.GetUserAsync(User)).UserType;

            // Check if an admin is editing another user's profile
            var isEditingAnotherUser = currentUserType == 'A' && userToEdit.Id != _userManager.GetUserId(User);

            if (!ModelState.IsValid)
            {
                await LoadAsync(userToEdit);
                return Page();
            }

            // Update the user details
            var phoneNumber = await _userManager.GetPhoneNumberAsync(userToEdit);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(userToEdit, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            // Other properties
            if (Input.fullname != userToEdit.FullName)
            {
                userToEdit.FullName = Input.fullname;
            }
            if (Input.DoB != userToEdit.DOB)
            {
                userToEdit.DOB = Input.DoB;
            }
            if (Input.Address != userToEdit.Address)
            {
                userToEdit.Address = Input.Address;
            }
            if (Input.UserType != userToEdit.UserType)
            {
                userToEdit.UserType = Input.UserType;
            }

            // Save the changes
            var result = await _userManager.UpdateAsync(userToEdit);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            // If an admin edited another user's profile, redirect to the admin index
            if (isEditingAnotherUser)
            {
                return RedirectToAction("Index", "Admin");
            }

            // If the current user updated their own profile, just refresh sign-in and stay on the page
            await _signInManager.RefreshSignInAsync(userToEdit);
            StatusMessage = "Your Profile Has Been Updated";
            return RedirectToPage();
        }
    }
}
