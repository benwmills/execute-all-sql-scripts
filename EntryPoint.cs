using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SqlClient;

namespace ExecuteAllSqlScripts
{
	/// <summary>
	/// Execute all SQL scripts in a folder.
	/// </summary>
	class EntryPoint
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static int Main(string[] args)
		{

	        // Parse the command line arguments.
		    string[] CmdArgs;
			string Server;
			string Database;

			CmdArgs = Environment.GetCommandLineArgs();

			if (CmdArgs.Length != 3)
			{
				Console.WriteLine("Three arguments must be specified:");
				Console.WriteLine();
				Console.WriteLine("1: Server");
				Console.WriteLine("2: Database");
				Console.WriteLine();
				Console.WriteLine("Example:  ExecuteAllSqlScripts.exe \"localhost\" \"Northwind\"");
				Console.WriteLine();
				Console.WriteLine("Press any key to exit.");
				Console.Read();
				return 1;
			}

	        Server = CmdArgs[1];
			Database = CmdArgs[2];

			// Setup the ADO objects.
			SqlConnection con;
			SqlCommand cmd;

			con = new SqlConnection("Server=" + Server + "; Integrated Security=SSPI; Database=" + Database);
			con.Open();

			cmd = new SqlCommand();
			cmd.CommandType = CommandType.Text;
			cmd.Connection = con;
			cmd.CommandTimeout = 180;

			// Loop through the files.
			StreamReader sr;
			string FileContent;
			string[] Batches;

			DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);

			foreach (FileInfo fi in di.GetFiles("*.sql"))
			{
				Console.WriteLine("Executing file " + fi.Name + "...");

				// Read the file.
				sr = new StreamReader(fi.FullName);
				FileContent = sr.ReadToEnd();

				// Break the file up into batches between GO statements.
				Batches = Regex.Split(FileContent, "\r\nGO\r\n");

				// Execute each batch.
				foreach (string Batch in Batches)
				{
					cmd.CommandText = Batch;
					try
					{
						cmd.ExecuteNonQuery();
					}
					catch (SqlException ex)
					{
						Console.WriteLine();
						Console.WriteLine("Error occured when executing batch");
						Console.WriteLine();
						Console.WriteLine(ex.Message);
						return 1;
					}
				}
			}

			// Dispose of ADO objects.
			cmd.Dispose();
			con.Dispose();

			return 0;
		}
	}
}
