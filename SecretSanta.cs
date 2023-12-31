class SecretSanta
{
    static Random random = new();

    static void Main(string[] args)
    {
        string namesFile = "names.txt";
        string previousAssignmentsFile = "assignments.txt";

        bool check = true, swaps = true;

        try
        {
            Console.WriteLine("Check previous assignments: ");
            check = bool.Parse(Console.ReadLine() ?? "");

            Console.WriteLine("Prevent direct swaps: ");
            swaps = bool.Parse(Console.ReadLine() ?? "");
        }
        catch
        {
            Console.WriteLine("Please enter a valid option (true or false).");
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
            return;
        }

        Dictionary<string,List<string>>? previousAssignments = null;
        List<List<string>> names = new();

        try
        {
            names = GetNames(namesFile);
            if(names.Count < 2)
                throw new Exception("Must have at least 2 names");
            if(check)
                previousAssignments = GetPreviousAssignments(previousAssignmentsFile);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
            return;
        }

        Dictionary<string, string> assignments = new();

        // May pick "incorrectly", so we need to loop multiple times
        for(int i = 0; i < 1000; i++)
        {
            try
            {
                assignments = Assign(names, previousAssignments, swaps);
                WriteAssignments(assignments, previousAssignmentsFile);
                Console.WriteLine($"Complete! Check '{previousAssignmentsFile}' for assignments");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                return;
            }
            catch {}
        }
        Console.WriteLine("No valid Secret Santa assignments possible.");
        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
        return;
    }

    // Returns a 2D list
    static List<List<string>> GetNames(string path)
    {
        List<List<string>> names = new();
        foreach (var line in File.ReadAllLines(path))
        {
            names.Add(line.Split(',').Select(name => name.Trim()).ToList());
        }
        return names;
    }

    // Returns a dictionary where the key is the giver, and the value is the recipients from before
    static Dictionary<string, List<string>> GetPreviousAssignments(string path)
    {
        Dictionary<string, List<string>> previous = new();

        foreach (string line in File.ReadAllLines(path))
        {
            if(line.Contains("Generated") || string.IsNullOrWhiteSpace(line))
                continue;
            List<string> names = line.Split("->").Select(name => name.Trim()).ToList();
        
            string giver = names[0];
            string recipient = names[1];

            if (!previous.ContainsKey(giver))
                previous[giver] = new List<string>();

            previous[giver].Add(recipient);
        }
        return previous;
    }

    // Returns a simple dictionary of assigned givers and recipients
    static Dictionary<string, string> Assign(List<List<string>> names, Dictionary<string, List<string>>? previousAssignments, bool preventDirectSwaps)
    {
        // Flatten 2D list
        List<string> available = names.SelectMany(group => group).ToList();

        Dictionary<string, string> assignments = new();
        
        // Keep track of all direct swaps, so if Person A gets Person B, B will not get A
        HashSet<(string, string)> directSwaps = new();

        foreach (List<string> group in names)
        {
            foreach (string giver in group)
            {
                List<string> possible = new(available);

                // Remove groups
                possible.RemoveAll(name => group.Contains(name));

                // Remove previously assigned names
                if(previousAssignments != null)
                    possible.RemoveAll(name => previousAssignments.ContainsKey(giver) && previousAssignments[giver].Contains(name));

                // Remove direct swaps
                if(preventDirectSwaps)
                    possible.RemoveAll(name => directSwaps.Contains((name, giver)));

                if (possible.Count == 0)
                    throw new InvalidOperationException("No valid Secret Santa assignments possible.");

                // Choose random recipient
                string recipient = possible[random.Next(possible.Count)];
                assignments[giver] = recipient;

                // Track for direct swaps
                directSwaps.Add((giver, recipient));

                // Make sure they are not picked again
                available.Remove(recipient);
            }
        }
        return assignments;
    }

    static void WriteAssignments(Dictionary<string, string> assignments, string path)
    {
        int maxLength = assignments.Keys.Max(key => key.Length);
        using StreamWriter sW = new(path, true);
        sW.WriteLine($"\nGenerated on {DateTime.Now.ToString("MMMM d, yyyy 'at' h:mm tt")}\n");
        sW.WriteLine($"\t{string.Join("\n\t", assignments.Select(kvp => $"{kvp.Key.PadRight(maxLength + 1)}-> {kvp.Value}"))}");
    }
}
