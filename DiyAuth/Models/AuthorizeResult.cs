﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DiyAuth.Models
{
	public class AuthorizeResult
	{
		public bool Success { get; internal set; }
		public string Token { get; internal set; }
	}
}
