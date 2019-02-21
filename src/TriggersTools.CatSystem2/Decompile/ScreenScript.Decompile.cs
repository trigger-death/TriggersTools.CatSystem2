﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TriggersTools.CatSystem2 {
	partial class ScreenScript {
		#region Constants

		/// <summary>
		///  Gets the header types that should always have a indent level of zero.
		/// </summary>
		private static readonly IReadOnlyList<string> FlatHeaders = new string[] {
			"OBJECT",
			"KEYBLOCK",
		};
		private static readonly string LinePattern =
			$@"^(?'hdr'#(?'flathdr'(?:{string.Join("|", FlatHeaders)})(?:\s|$))?)" +
			@"|(?'if'if\b)" +
			@"|(?'endif'endif\b)";
		private static readonly Regex LineRegex = new Regex(LinePattern, RegexOptions.IgnoreCase);

		#endregion

		#region Decompile (From File)

		/// <summary>
		///  Loads and decompiles the FES script script file and returns the script as a string.
		/// </summary>
		/// <param name="fesFile">The file path to the FES script script file to extract.</param>
		/// <returns>The decompiled script.</returns>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="fesFile"/> is null.
		/// </exception>
		public static string Decompile(string fesFile) {
			return Extract(fesFile).Decompile();
		}
		/// <summary>
		///  Loads and decompiles the FES script script file and outputs it to the specified file.
		/// </summary>
		/// <param name="fesFile">The file path to the FES script script file to extract.</param>
		/// <param name="outFile">The output file to write the decompiled script to.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="fesFile"/> or <paramref name="outFile"/> is null.
		/// </exception>
		public static void DecompileToFile(string fesFile, string outFile) {
			Extract(fesFile).DecompileToFile(outFile);
		}
		/// <summary>
		///  Loads and decompiles the FES script script file and outputs it to the specified stream.
		/// </summary>
		/// <param name="fesFile">The file path to the FES script script file to extract.</param>
		/// <param name="outStream">The output stream to write the decompiled script to.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="fesFile"/> or <paramref name="outStream"/> is null.
		/// </exception>
		public static void DecompileToStream(string fesFile, Stream outStream) {
			Extract(fesFile).DecompileToStream(outStream);
		}

		#endregion

		#region Decompile (From Stream)

		/// <summary>
		///  Loads and decompiles the FES script script stream and returns the script as a string.
		/// </summary>
		/// <param name="stream">The stream to extract the script script from.</param>
		/// <param name="fileName">The path or name of the script script file being extracted.</param>
		/// <returns>The decompiled script.</returns>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="stream"/> or <paramref name="fileName"/> is null.
		/// </exception>
		public static string Decompile(Stream stream, string fileName) {
			return Extract(stream, fileName).Decompile();
		}
		/// <summary>
		///  Loads and decompiles the FES script script stream and outputs it to the specified file.
		/// </summary>
		/// <param name="stream">The stream to extract the script script from.</param>
		/// <param name="fileName">The path or name of the script script file being extracted.</param>
		/// <param name="outFile">The output file to write the decompiled script to.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="stream"/>, <paramref name="fileName"/>, or <paramref name="outFile"/> is null.
		/// </exception>
		public static void DecompileToFile(Stream inStream, string fileName, string outFile) {
			Extract(inStream, fileName).DecompileToFile(outFile);
		}
		/// <summary>
		///  Loads and decompiles the FES script script stream and outputs it to the specified stream.
		/// </summary>
		/// <param name="stream">The stream to extract the script script from.</param>
		/// <param name="fileName">The path or name of the script script file being extracted.</param>
		/// <param name="outStream">The output stream to write the decompiled script to.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="stream"/>, <paramref name="fileName"/>, or <paramref name="outStream"/> is null.
		/// </exception>
		public static void DecompileToStream(Stream inStream, string fileName, Stream outStream) {
			Extract(inStream, fileName).DecompileToStream(outStream);
		}

		#endregion

		#region Decompile (Instance)

		/// <summary>
		///  Decompiles the FES script script and returns the script as a string.
		/// </summary>
		/// <returns>The decompiled script.</returns>
		public string Decompile() {
			using (StringWriter writer = new StringWriter()) {
				DecompileInternal(writer);
				return writer.ToString();
			}
		}
		/// <summary>
		///  Decompiles the FES script script and outputs it to the specified file.
		/// </summary>
		/// <param name="outFile">The output file to write the decompiled script to.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="outFile"/> is null.
		/// </exception>
		public void DecompileToFile(string outFile) {
			using (StreamWriter writer = new StreamWriter(outFile, false, Encoding.UTF8))
				DecompileInternal(writer);
		}
		/// <summary>
		///  Decompiles the FES script script and outputs it to the specified stream.
		/// </summary>
		/// <param name="outStream">The output stream to write the decompiled script to.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="outStream"/> is null.
		/// </exception>
		public void DecompileToStream(Stream outStream) {
			using (StreamWriter writer = new StreamWriter(outStream, Encoding.UTF8))
				DecompileInternal(writer);
		}

		#endregion

		#region Decompile (Internal)

		/// <summary>
		///  Decompiles the FES screen script and writes it to the text writer.
		/// </summary>
		/// <param name="writer">The text writer to write the decompiled script to.</param>
		private void DecompileInternal(TextWriter writer) {
			bool isFlat = false; // Forces the tag level to be zero
			int level = 1; // The indent level of the text

			for (int i = 0; i < Count; i++) {
				string line = Lines[i];
				Match match = LineRegex.Match(line);
				if (match.Groups["hdr"].Success) {
					if (i != 0)
						writer.WriteLine();
					writer.WriteLine(line);
					isFlat = match.Groups["flathdr"].Success;
				}
				else {
					if (match.Groups["endif"].Success)
						level = Math.Max(1, level - 1);

					if (!isFlat)
						writer.Write(new string('\t', level));
					writer.WriteLine(line);

					if (match.Groups["if"].Success)
						level++;
				}
			}
			writer.Flush();
		}

		#endregion
	}
	partial class KifintEntry {
		#region DecompileScreen

		/// <summary>
		///  Loads and decompiles the FES screen script entry and returns the script as a string.
		/// </summary>
		/// <returns>The decompiled script.</returns>
		public string DecompileScreen() {
			using (MemoryStream stream = ExtractToStream())
				return ScreenScript.Decompile(stream, FileName);
		}
		/// <summary>
		///  Loads and decompiles the FES screen script entry and outputs it to the specified file.
		/// </summary>
		/// <param name="outFile">The output file to write the decompiled script to.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="outFile"/> is null.
		/// </exception>
		public void DecompileScreenToFile(string outFile) {
			using (MemoryStream stream = ExtractToStream())
				ScreenScript.DecompileToFile(stream, FileName, outFile);
		}
		/// <summary>
		///  Loads and decompiles the FES screen script entry and outputs it to the specified stream.
		/// </summary>
		/// <param name="outStream">The output stream to write the decompiled script to.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="outStream"/> is null.
		/// </exception>
		public void DecompileScreenToStream(Stream outStream) {
			using (MemoryStream stream = ExtractToStream())
				ScreenScript.DecompileToStream(stream, FileName, outStream);
		}

		#endregion

		#region DecompileScreen (KifintStream)

		/// <summary>
		///  Loads and decompiles the FES screen script entry and returns the script as a string.
		/// </summary>
		/// <param name="kifintStream">The stream to the open KIFINT archive.</param>
		/// <returns>The decompiled script.</returns>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="kifintStream"/> is null.
		/// </exception>
		public string DecompileScreen(KifintStream kifintStream) {
			using (MemoryStream stream = ExtractToStream(kifintStream))
				return ScreenScript.Decompile(stream, FileName);
		}
		/// <summary>
		///  Loads and decompiles the FES screen script entry and outputs it to the specified file.
		/// </summary>
		/// <param name="kifintStream">The stream to the open KIFINT archive.</param>
		/// <param name="outFile">The output file to write the decompiled script to.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="kifintStream"/> or <paramref name="outFile"/> is null.
		/// </exception>
		public void DecompileScreenToFile(KifintStream kifintStream, string outFile) {
			using (MemoryStream stream = ExtractToStream())
				ScreenScript.DecompileToFile(stream, FileName, outFile);
		}
		/// <summary>
		///  Loads and decompiles the FES screen script entry and outputs it to the specified stream.
		/// </summary>
		/// <param name="outStream">The output stream to write the decompiled script to.</param>
		/// 
		/// <param name="kifintStream">The stream to the open KIFINT archive.</param>
		/// <exception cref="ArgumentNullException">
		///  <paramref name="kifintStream"/> or <paramref name="outStream"/> is null.
		/// </exception>
		public void DecompileScreenToStream(KifintStream kifintStream, Stream outStream) {
			using (MemoryStream stream = ExtractToStream())
				ScreenScript.DecompileToStream(stream, FileName, outStream);
		}

		#endregion
	}
}
