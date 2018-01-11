using System;
using System.IO;
using System.Text.RegularExpressions;

namespace FortranToCs {
	class Application {
		public static void Main1 (string[] args) {
			while (true) {
				String text;
				using (var reader = File.OpenText(@"C:\Users\Kailang\Desktop\vars.js")) {
					text = reader.ReadToEnd();
				}

				var vars = text.Trim().Split(new [] { ',', ';', '=', '0', ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
	
				var str = "Cmd.WriteLine(\"";
				for (int i = 0; i < vars.Length; i++) {
					str += string.Format("{1}: {{{0}}}, ", i, vars[i]);
				}
				str = str.Remove(str.Length - 2) + "\", ";

				for (int i = 0; i < vars.Length; i++) {
					str += string.Format("{0}, ", vars[i]);
				}
				str = str.Remove(str.Length - 2) + ");";

				Console.WriteLine(str);

				str = "print *, ";
				foreach (var v in vars) {
					if (v != "int" && v != "double")
						str += string.Format("\"{0}\", {0}, ", v);
				}
				str = str.Remove(str.Length - 2);
				Console.WriteLine(str);

				Console.ReadLine();
			}
		}

		public static void Main (string[] args) {
			while (true) {
				String text;
				using (var reader = File.OpenText(@"C:\Users\Kailang\Desktop\temp.f90")) {
					text = reader.ReadToEnd();
				}

				String[] arrs;
				using (var reader = File.OpenText(@"C:\Users\Kailang\Desktop\temp.js")) {
					arrs = reader.ReadToEnd().Trim().Split(new [] { ',', ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
				}

				// &
				text = Regex.Replace(text, @"[ \n\r\t]*\&[ \n\r\t]*", " ");

				// Comments
				text = Regex.Replace(text, @"[ \n\r\t]*\! *(.+)", match => string.Format(
					"\n// {0}{1}", match.Groups[1].ToString().Substring(0, 1).ToUpper(), match.Groups[1].ToString().Substring(1)));

				// Logical expressions;
				text = text
					.Replace(".and.", "&&").Replace(".or.", "||")
					.Replace(".true.", "true").Replace(".false.", "false")
					.Replace("/=", "!=").Replace(".not.", "!").Replace(".eqv.", "==");

				// Power
				text = text.Replace("**", "Math.Pow");

				// Simple braces;
				text = text
					.Replace("then", "{").Replace("endif", "}").Replace("enddo", "}");

				// If statements;
				text = Regex.Replace(text, "else(?!if)", "} else {").Replace("elseif", "} else if");

				// Do while;
				text = Regex.Replace(text, @"do while *\((.+)\)", match => string.Format(
					"while ({0}) {{", match.Groups[1]));

				// Do loop step down;
				text = Regex.Replace(text, @"do ([^ ]+) *= *([^,]+), *([^,\n\r]+), *-([^!\n\r\t]+)", match => string.Format(
					"for ({0} = {1}; {0} >= {2}; {0} -= {3}) {{", match.Groups[1], match.Groups[2], match.Groups[3], match.Groups[4]));
				// Do loop step normal;
				text = Regex.Replace(text, @"do ([^ ]+) *= *([^,]+), *([^,\n\r]+), *([^!\n\r\t]+)", match => string.Format(
					"for ({0} = {1}; {0} <= {2}; {0} += {3}) {{", match.Groups[1], match.Groups[2], match.Groups[3], match.Groups[4]));
				// Do loop normal;
				text = Regex.Replace(text, @"do ([^ ]+) *= *([^,]+), *([^!\n\r\t]+)", match => string.Format(
					"for ({0} = {1}; {0} <= {2}; {0}++) {{", match.Groups[1], match.Groups[2], match.Groups[3]));
			
				// Arraies;
				var last = "";
				while (last != text) {
					last = text;
					foreach (var arr in arrs) {
						if (!string.IsNullOrEmpty(arr))
							text = Regex.Replace(text, "([^a-z]|^)" + arr + @" *\(([^\(\)]+)\)", match => string.Format(
								"{0}{1}[{2}]", match.Groups[1], arr.Trim(), match.Groups[2]));
					}
				}

				// Semicones;
				text = Regex.Replace(text, @"([^{} \t\n\r])([\r\n]+)", match => string.Format(
					"{0};{1}", match.Groups[1], match.Groups[2]));

				// Calls;
				text = Regex.Replace(text, @"call ([a-z])", match => string.Format(
					"{0}", match.Groups[1].ToString().ToUpper()));

				Console.Write("{0} chars converted. {1}", text.Length, DateTime.Now);
				File.WriteAllText(@"C:\Users\Kailang\Desktop\temp.cs", text);

				Console.ReadLine();
			}
		}
	}
}
