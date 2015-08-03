using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using Google.GData.Client;
using Google.GData.Extensions;

namespace PicasaWebSync
{
	public class GDataAuthenticator
	{
		public GDataRequestFactory requestFactory { get; set; }
			 
		public GDataAuthenticator (string accessToken)
		{
			requestFactory = new GDataRequestFactory ("PicasaWebSync");
			requestFactory.CustomHeaders.Add (string.Format ("Authorization: Bearer {0}", accessToken));
		}		
	}
}
