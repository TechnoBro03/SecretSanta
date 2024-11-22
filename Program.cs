// Copyright (c) 2024 TechnoBro03. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace SecretSanta
{
	using System.Text.RegularExpressions;

	/// <summary>
	/// A simple console application to generate Secret Santa assignments.
	/// </summary>
	public class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		/// <remarks>args[0] = namesFile, args[1] = assignmentsFile, args[2] = checkPreviousAssignments, args[3] = allowReciprocal, (optional) args[4] = verboseOutput</remarks>
		public static void Main(string[] args)
		{
			if (args.Length is not 4 and not 5)
			{
				Console.WriteLine("Usage: <namesFile> <assignmentsFile> <checkPreviousAssignments> <allowReciprocal> [verboseOutput]");
				Console.WriteLine("Example: names.txt previousAssignments.txt true false");
				return;
			}

			try
			{
				// Parse command line arguments
				string namesFile = args[0];
				string assignmentsFile = args[1];
				bool checkPreviousAssignments = bool.Parse(args[2]);
				bool allowReciprocal = bool.Parse(args[3]);
				bool verboseOutput = args.Length == 5 && bool.TryParse(args[4], out bool result) && result;

				if (verboseOutput)
				{
					Console.WriteLine($"Names File: {namesFile}");
					Console.WriteLine($"Previous Assignments File: {assignmentsFile}");
					Console.WriteLine($"Check Previous Assignments: {checkPreviousAssignments}");
					Console.WriteLine($"Allow Reciprocal: {allowReciprocal}");
					Console.WriteLine($"Verbose Output: {verboseOutput}");
				}

				var participants = GetParticipants(namesFile);
				var groups = GetGroups(namesFile);
				var previousAssignments = checkPreviousAssignments ? GetPreviousPairs(assignmentsFile) : [];

				// Shuffle the participants to ensure a random order
				Shuffle(participants);

				var pairs = Pair(participants, groups, previousAssignments, allowReciprocal, verboseOutput);

				WriteAssignments("assignments.txt", pairs);

				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine($"Assignments have been generated successfully ({Path.GetFullPath(assignmentsFile)})");
				Console.ResetColor();
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(ex.Message);
				Console.ResetColor();
			}
		}

		/// <summary>
		/// Gets all participants from a file.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns>A list of all .</returns>
		public static List<string> GetParticipants(string path)
		{
			if (!File.Exists(path))
			{
				throw new FileNotFoundException("File not found.", path);
			}

			List<string> participants = [];

			// Each line represents a participant, or a group of participants
			foreach (string line in File.ReadAllLines(path))
			{
				participants.AddRange(line.Split(',').Select(participant => participant.Trim()));
			}

			return participants;
		}

		/// <summary>
		/// Gets all groups from a file.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns>A list of groups.</returns>
		public static List<List<string>> GetGroups(string path)
		{
			if (!File.Exists(path))
			{
				throw new FileNotFoundException("File not found.", path);
			}

			List<List<string>> groups = [];

			// Each line represents a group of participants
			foreach (string line in File.ReadAllLines(path))
			{
				groups.Add(line.Split(',').Select(participant => participant.Trim()).ToList());
			}

			return groups;
		}

		/// <summary>
		/// Gets previous assignments from a file.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns>A dictionary of previous assignments. (Key = Giver, Value = [Recipient 1, ..., Recipient N]).</returns>
		public static Dictionary<string, List<string>> GetPreviousPairs(string path)
		{
			if (!File.Exists(path))
			{
				throw new FileNotFoundException("File not found.", path);
			}

			Dictionary<string, List<string>> previous = [];

			// Get only lines that contain a "giver -> recipient" pattern
			string pattern = @"^.*->.*$";
			var matches = Regex.Matches(File.ReadAllText(path), pattern, RegexOptions.Multiline);

			// Store the givers and recipients in a dictionary
			foreach (Match match in matches)
			{
				var kvp = match.Value.Split("->").Select(participant => participant.Trim());

				if (!previous.ContainsKey(kvp.First()))
				{
					previous[kvp.First()] = [];
				}

				previous[kvp.First()].Add(kvp.Last());
			}

			return previous;
		}

		/// <summary>
		/// Randomly shuffles a list.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">List to shuffle.</param>
		/// <remarks>In-place algorithm.</remarks>
		public static void Shuffle<T>(List<T> list)
		{
			Random random = new();

			for (int i = list.Count - 1; i > 0; i--)
			{
				int j = random.Next(i + 1); // Generate a random index from 0 to i
				(list[i], list[j]) = (list[j], list[i]); // Swap elements
			}
		}

		/// <summary>
		/// Pairs participants together.
		/// </summary>
		/// <param name="participants">List of all participants.</param>
		/// <param name="groups">List of all groups.</param>
		/// <param name="previousPairings">Dictionary of previous pairings.</param>
		/// <param name="allowReciprocal">Whether a giver and recipient can be reciprocal.</param>
		/// <returns>A list of (giver, recipient).</returns>
		/// <remarks>This algorithm is deterministic for a given set of inputs. To randomize, shuffle the <see cref="participants"/> parameter.</remarks>
		public static List<(string, string)> Pair(List<string> participants, List<List<string>> groups, Dictionary<string, List<string>> previousPairings, bool allowReciprocal, bool verboseOutput)
		{
			// Store the list of pairings
			List<(string, string)> pairings = [];

			// Store the list of available recipients. Note: This must be ordered to ensure deterministic results
			SortedList<int, string> available = [];

			// Initialize the available list
			participants.ForEach(participant => available.Add(participants.IndexOf(participant), participant));

			// Store the last selected recipient for each giver
			Dictionary<string, int> validIndex = [];

			// Initialize the valid index
			foreach (string giver in participants)
			{
				validIndex[giver] = 0;
			}

			// Iterate through each giver
			for (int g = 0; g < participants.Count;)
			{
				string giver = participants[g];

				// Find the group that the giver belongs to
				var group = groups.Find(group => group.Contains(giver)) ?? [];

				// Find the previous recipients of the giver
				var previousRecipients = previousPairings.TryGetValue(giver, out var recipients) ? recipients : [];

				// Filter valid recipients (again, this will be ordered and thus deterministic)
				var validRecipients = available.Values.ToList().FindAll(recipient =>
					recipient != giver &&                                                                         // No self-matching
					!previousRecipients.Contains(recipient) &&                                                    // No historical recipients
					(allowReciprocal || !pairings.Exists(pair => pair.Item2 == giver && pair.Item1 == recipient)) // Reciprocal
				);

				// If there are valid recipients
				if (validIndex[giver] < validRecipients.Count)
				{
					// Select a recipient
					string recipient = validRecipients[validIndex[giver]];

					// Increment the valid index (for backtracking)
					validIndex[giver]++;

					// Add the pair to the list of pairings
					pairings.Add((giver, recipient));

					// Print verbose output
					if (verboseOutput)
					{
						Console.Write($"Giver: {giver}. Recipients: ");
						foreach (string rec in available.Values)
						{
							if (!validRecipients.Contains(rec))
							{
								Console.ForegroundColor = ConsoleColor.Red;
							}
							if (rec == recipient)
							{
								Console.ForegroundColor = ConsoleColor.Green;
							}

							Console.Write($"{rec}");
							Console.ResetColor();
							Console.Write(", ");
						}
						Console.WriteLine();
					}

					// Remove the recipient from the available list
					available.Remove(participants.IndexOf(recipient));

					// Move on to the next giver
					g++;
				}

				// If there are no valid recipients
				else
				{
					// If there are no pairs left, then there are no valid pairings
					if (pairings.Count == 0)
					{
						throw new InvalidOperationException("No valid pairings found.");
					}

					// Move back one giver
					g--;

					// Add the last recipient back to the available list
					available.Add(participants.IndexOf(pairings[g].Item2), pairings[g].Item2);

					// Remove the last pair
					pairings.RemoveAt(g);

					// Reset the valid index
					validIndex[giver] = 0;

					// Print verbose output
					if (verboseOutput)
					{
						Console.Write($"Giver: {giver}. Recipients: ");
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine($"{string.Join(", ", available.Values)}");
						Console.ResetColor();
					}
				}
			}

			return pairings;
		}

		/// <summary>
		/// Writes the assignments to a file.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="assignments">A list of pairs.</param>
		public static void WriteAssignments(string path, List<(string, string)> pairs)
		{
			// Sort pairs alphabetically by giver
			pairs = [.. pairs.OrderBy(pair => pair.Item1)];

			// Get the longest length of any participant (for padding)
			int maxLength = pairs.Max(pair => Math.Max(pair.Item1.Length, pair.Item2.Length));

			using StreamWriter streamWriter = new(path, true);

			// Write the formatted assignments to the file
			streamWriter.WriteLine($"Generated on {DateTime.Now:MMMM d, yyyy 'at' h:mm tt}\n");
			streamWriter.WriteLine($"\t{string.Join("\n\t", pairs.Select(pair => $"{pair.Item1.PadRight(maxLength)} -> {pair.Item2}"))}\n");
		}
	}
}