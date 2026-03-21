namespace itgsgroup.Areas.Identity.Data
{
	using Microsoft.AspNetCore.Identity;
	using Microsoft.Extensions.Options;
	using System;
	using System.Collections.Generic;
	using System.Security.Claims;
	using System.Threading.Tasks;
	using Microsoft.Extensions.Logging;
	using Microsoft.Extensions.DependencyInjection;
	using System.Linq;

	public class ApplicationUserManager : UserManager<ApplicationUser>
	{
		public ApplicationUserManager(IUserStore<ApplicationUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<ApplicationUser> passwordHasher,
			IEnumerable<IUserValidator<ApplicationUser>> userValidators, IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer,
			IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<ApplicationUser>> logger)
			: base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
		{
		}

		public async Task<string> GetNameAsync(ApplicationUser user)
		{
			return user.name;
		}

		public async Task<string> GetProfilePicAsync(ApplicationUser user)
		{
			return user.profile_pic;
		}
	}

}
