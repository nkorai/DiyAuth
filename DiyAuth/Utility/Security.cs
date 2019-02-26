using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DiyAuth.Utility
{
	public class Security
	{
		public static string GeneratePerUserSalt()
		{
			using (var rng = new RNGCryptoServiceProvider())
			{
				var buffer = new byte[128];
				rng.GetBytes(buffer);
				return Encoding.ASCII.GetString(buffer);
			}
		}

		public static string GeneratePasswordHash(string password, string salt)
		{
			return BCrypt.Net.BCrypt.HashPassword(password, salt);
		}

		public static bool PasswordMatches(string salt, string inputPassword, string dbPasswordHash)
		{
			var inputPasswordHash = GeneratePasswordHash(inputPassword, salt);
			return inputPasswordHash.Equals(dbPasswordHash);
		}

		public static string GenerateToken()
		{
			using (var rng = new RNGCryptoServiceProvider())
			{
				var token = new byte[199];
				rng.GetBytes(token);
				return Encoding.ASCII.GetString(token);
			}
		}
	}
}
