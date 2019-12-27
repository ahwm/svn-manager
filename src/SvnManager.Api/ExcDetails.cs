using System;
using System.Data.SqlClient;
using System.Reflection;
using System.Security.Cryptography;

namespace SvnManager.Api
{
    /// <summary>
    /// Class to parse out and retrieve full details of a given <see cref="System.Exception"/> object
    /// </summary>
	public static class ExcDetails
	{
		private static string _GetSqlDetails(SqlException ex)
		{
			string text = "";
			int num = 1;
			foreach (SqlError sqlError in ex.Errors)
			{
				object obj = text;
				text = string.Concat(new object[]
				{
					obj,
					"--SqlDetail#",
					num.ToString().PadLeft(2, '0'),
					"--\r\n ----Class-------\r\n ",
					sqlError.Class,
					"\r\n----LineNumber--\r\n ",
					sqlError.LineNumber,
					"\r\n----Message-----\r\n ",
					sqlError.Message,
					"\r\n----Number------\r\n ",
					sqlError.Number,
					"\r\n----Procedure---\r\n ",
					sqlError.Procedure,
					"\r\n----Source------\r\n ",
					sqlError.Source,
					"\r\n----State-------\r\n ",
					sqlError.State,
					"\r\n"
				});
				num++;
			}
			return text;
		}
        /// <summary>
        /// Retrieve the full details of the <see cref="System.Exception"/>
        /// </summary>
        /// <param name="ex">The <see cref="System.Exception"/> to iterate through</param>
        /// <returns>String value as a message</returns>
		public static string Get(Exception ex)
		{
			return string.Concat(new string[]
			{
				"--date------------\r\n ",
				DateTime.UtcNow.ToString("s"),
				"\r\n--type------------\r\n ",
				ex.GetType().FullName,
				"\r\n--Message---------\r\n ",
				ex.Message,
				"\r\n--Source----------\r\n ",
				ex.Source,
				"\r\n--StackTrace------\r\n ",
				ex.StackTrace,
				"\r\n",
				(ex is SqlException) ? ExcDetails._GetSqlDetails((SqlException)ex) : "",
				(ex.InnerException != null) ? ("--Inner-----------\r\n " + ExcDetails.Get(ex.InnerException)) : ""
			});
		}
        /// <summary>
        /// Checks to seeif the provided <see cref="System.Exception"/> is related to database connection
        /// </summary>
        /// <param name="ex">The Exception to check</param>
        /// <returns><see langword="true"/> or <see langword="false"/></returns>
		public static bool IsConnectivityWebException(Exception ex)
		{
			Exception ex2 = null;
			if (ex != null && ex.InnerException != null)
			{
				ex2 = ex.InnerException;
			}
			bool result;
			if (ex2 is SqlException)
			{
				if (ex2.Message.StartsWith("An error has occurred while establishing a connection to the server."))
				{
					result = true;
					return result;
				}
				if (ex2.Message.StartsWith("Cannot open database \""))
				{
					result = true;
					return result;
				}
				if (ex2.Message.StartsWith("A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible."))
				{
					result = true;
					return result;
				}
				if (ex2.Message.StartsWith("Timeout expired."))
				{
					result = true;
					return result;
				}
			}
			result = (ex2 is InvalidOperationException && ex2.Message.StartsWith("Timeout expired."));
			return result;
		}
        /// <summary>
        /// Checks to see if the provided <see cref="System.Exception"/> is infrastructure related
        /// </summary>
        /// <param name="ex">The exception to check</param>
        /// <returns><see langword="true"/> or <see langword="false"/></returns>
		public static bool IsInfrastructureWebException(Exception ex)
		{
			Exception ex2 = ex;
			if (ex != null && ex.InnerException != null)
			{
				ex2 = ex.InnerException;
			}
			return (ex2 is ArgumentException && ex2.Message.StartsWith("Invalid postback or callback argument.")) || (ex is TargetInvocationException && ex2 is FormatException && ex2.Message == "Invalid length for a Base-64 char array.") || (ex is TargetInvocationException && ex2 is FormatException && ex2.Message == "Invalid character in a Base-64 string.") || (ex is FormatException && ex.Message == "Invalid character in a Base-64 string.") || (ex is TargetInvocationException && ex2 is CryptographicException && ex2.Message == "Padding is invalid and cannot be removed.") || (ex is TargetInvocationException && ex2 is CryptographicException && ex2.Message == "Length of the data to decrypt is invalid.") || (ex is CryptographicException && ex.Message == "Padding is invalid and cannot be removed.");
		}
	}
}
