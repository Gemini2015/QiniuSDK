﻿using System;
using System.IO;
using Qiniu.Conf;
using Qiniu.Util;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace Qiniu.Auth.digest
{
	/// <summary>
	/// 七牛消息认证(Message Authentication)
	/// </summary>
	public class Mac
	{

		private string accessKey;

		/// <summary>
		/// Gets or sets the access key.
		/// </summary>
		/// <value>The access key.</value>
		public string AccessKey
		{
			get { return accessKey; }
			set { accessKey = value; }
		}

		private byte[] secretKey;

		/// <summary>
		/// Gets the secret key.
		/// </summary>
		/// <value>The secret key.</value>
		public byte[] SecretKey
		{
			get { return secretKey; }
		}

		public Mac()
		{
			this.accessKey = Conf.Config.ACCESS_KEY;
			this.secretKey = Config.Encoding.GetBytes(Config.SECRET_KEY);
		}

		public Mac(string access, byte[] secretKey)
		{
			this.accessKey = access;
			this.secretKey = secretKey;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		private string _sign(byte[] data)
		{
			return GetSHA1Key(SecretKey, Config.Encoding.GetString(data));
		}

		/// <summary>
		/// Sign
		/// </summary>
		/// <param name="b"></param>
		/// <returns></returns>
		public string Sign(byte[] b)
		{
			return string.Format("{0}:{1}", this.accessKey, _sign(b));
		}

		/// <summary>
		/// SignWithData
		/// </summary>
		/// <param name="b"></param>
		/// <returns></returns>
		public string SignWithData(byte[] b)
		{
			string data = Base64URLSafe.Encode(b);
			return string.Format("{0}:{1}:{2}", this.accessKey, _sign(Config.Encoding.GetBytes(data)), data);
		}

		/// <summary>
		/// SignRequest
		/// </summary>
		/// <param name="request"></param>
		/// <param name="body"></param>
		/// <returns></returns>
		public string SignRequest(System.Net.HttpWebRequest request, byte[] body)
		{
			Uri u = request.RequestUri;

			string pathAndQuery = request.RequestUri.PathAndQuery;
			byte[] pathAndQueryBytes = Config.Encoding.GetBytes(pathAndQuery);
			using (MemoryStream buffer = new MemoryStream())
			{
				buffer.Write(pathAndQueryBytes, 0, pathAndQueryBytes.Length);
				buffer.WriteByte((byte)'\n');
				if (body.Length > 0)
				{
					buffer.Write(body, 0, body.Length);
				}
				string digestBase64 = GetSHA1Key(SecretKey, buffer.ToString());
				return this.accessKey + ":" + digestBase64;
			}
		}

		private string GetSHA1Key(byte[] secretKey, string value)
		{
			var objMacProv = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha1);
			var hash = objMacProv.CreateHash(secretKey.AsBuffer());
			hash.Append(CryptographicBuffer.ConvertStringToBinary(value, BinaryStringEncoding.Utf8));
			return CryptographicBuffer.EncodeToBase64String(hash.GetValueAndReset()).Replace('+', '-').Replace('/', '_');
		}
	}
}