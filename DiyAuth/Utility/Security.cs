using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace DiyAuth.Utility
{
	public class Security
	{
		private const int SaltSize = 22;
		public static string GeneratePerUserSalt()
		{
			var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
			var saltArray = new byte[SaltSize];
			using (var rng = new RNGCryptoServiceProvider())
			{
				rng.GetBytes(saltArray);
			}

			var result = new StringBuilder(SaltSize);
			foreach (var b in saltArray)
			{
				result.Append(chars[b % (chars.Length)]);
			}

			var salt = "$2a$04$" + result.ToString(); // Mandatory to get correct salt format
			return salt;
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
				var tokenArray = new byte[199];
				rng.GetBytes(tokenArray);
				var token = Convert.ToBase64String(tokenArray);
				token = Regex.Replace(token, "[\\/#?]+", ""); // Removing any characters not allowed in Azure TableStorage RowKeys
				return token;
			}
		}
	}
}
