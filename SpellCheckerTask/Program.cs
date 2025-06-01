using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpellCheckerTask
{
    internal class Program
    {
        static void Main(string[] args)
        {
            HashSet<string> dict = CreateDictionary();

            while (true)
            {
                Console.WriteLine("\nSpellChecker Menu:");
                Console.WriteLine("1. Check single word spelling");
                Console.WriteLine("2. Check sentence spelling");
                Console.WriteLine("3. Calculate spelling score for a sentence");
                Console.WriteLine("4. Save misspelled words from a sentence to a file");
                Console.WriteLine("5. Challenge: Suggest spelling corrections");
                Console.WriteLine("0. Exit");
                Console.Write("Choose an option: ");

                string? choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1":
                        Console.Write("Enter a single word to check spelling: ");
                        string? singleWord = Console.ReadLine()?.ToUpper();
                        if (!string.IsNullOrEmpty(singleWord))
                        {
                            bool correct = CheckSingleWord(dict, singleWord);
                            Console.WriteLine(correct
                                ? $"'{singleWord}' is spelled correctly."
                                : $"'{singleWord}' is NOT spelled correctly.");
                        }
                        break;

                    case "2":
                        Console.Write("Enter a sentence to check spelling: ");
                        string? sentence1 = Console.ReadLine()?.ToUpper();
                        if (!string.IsNullOrEmpty(sentence1))
                        {
                            bool allCorrect = CheckSentence(dict, sentence1, out string[] misspelled);
                            if (allCorrect)
                                Console.WriteLine("All words spelled correctly.");
                            else
                            {
                                Console.WriteLine("Misspelled words:");
                                foreach (var w in misspelled) Console.WriteLine(w);
                            }
                        }
                        break;

                    case "3":
                        Console.Write("Enter a sentence to calculate spelling score: ");
                        string? sentence2 = Console.ReadLine()?.ToUpper();
                        if (!string.IsNullOrEmpty(sentence2))
                        {
                            double score = CalculateSpellingScore(dict, sentence2);
                            Console.WriteLine($"Spelling score: {score:P2}");
                        }
                        break;

                    case "4":
                        Console.Write("Enter a sentence to save misspelled words: ");
                        string? sentence3 = Console.ReadLine()?.ToUpper();
                        if (!string.IsNullOrEmpty(sentence3))
                        {
                            SaveMisspelledWords(dict, sentence3, "MisspelledWords.txt");
                            Console.WriteLine("Misspelled words saved to MisspelledWords.txt");
                        }
                        break;

                    case "5":
                        SuggestionSystem(dict);
                        break;

                    case "0":
            .            Console.WriteLine("Exiting program.");
                        return;

                    default:
                        Console.WriteLine("Invalid option, please try again.");
                        break;
                }
            }
        }

        static HashSet<string> CreateDictionary()
        {
            HashSet<string> dict = new();
            using StreamReader reader = new("WordsFile.txt");
            while (!reader.EndOfStream)
            {
                string? word = reader.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(word))
                    dict.Add(word);
            }
            return dict;
        }

        // 1
        static bool CheckSingleWord(HashSet<string> dict, string word)
        {
            return dict.Contains(word);
        }

        // 2
        static bool CheckSentence(HashSet<string> dict, string sentence, out string[] misspelled)
        {
            string[] words = sentence.Split(new[] { ' ', '.', ',', '!', '?', ';', ':', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> misspelledList = new();

            foreach (string word in words)
            {
                if (!dict.Contains(word))
                    misspelledList.Add(word);
            }
            misspelled = misspelledList.ToArray();
            return misspelled.Length == 0;
        }

        // 3
        static double CalculateSpellingScore(HashSet<string> dict, string sentence)
        {
            string[] words = sentence.Split(new[] { ' ', '.', ',', '!', '?', ';', ':', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0) return 1.0; // no words means 100%

            int correctCount = words.Count(word => dict.Contains(word));
            return (double)correctCount / words.Length;
        }

        // 4
        static void SaveMisspelledWords(HashSet<string> dict, string sentence, string filename)
        {
            CheckSentence(dict, sentence, out string[] misspelled);
            File.WriteAllLines(filename, misspelled.Distinct());
        }

        // Challenge
        static void SuggestionSystem(HashSet<string> dict)
        {
            Console.WriteLine("\nChallenge: Enter a sentence to check and get spelling suggestions:");
            string? input = Console.ReadLine()?.ToUpper();
            if (input == null) return;

            string[] words = input.Split(new[] { ' ', '.', ',', '!', '?', ';', ':', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> suggestions = new();

            foreach (string word in words)
            {
                if (!dict.Contains(word))
                {
                    Console.WriteLine($"'{word}' might be incorrect.");
                    // Finds best suggestions based on weighted Damerau-Levenshtein distance
                    string? suggestion = dict
                        .OrderBy(d => WeightedDamerauLevenshtein(d, word))
                        .FirstOrDefault();

                    if (suggestion != null)
                    {
                        Console.WriteLine($"Did you mean '{suggestion}'? (y/n): ");
                        if (Console.ReadLine()?.Trim().ToLower() == "y")
                        {
                            suggestions.Add(suggestion);
                        }
                    }
                }
            }

            if (suggestions.Count > 0)
            {
                File.WriteAllLines("SpellingSuggestions.txt", suggestions.Distinct());
                Console.WriteLine("Suggestions saved to SpellingSuggestions.txt.");
            }
        }

        static double WeightedDamerauLevenshtein(string s, string t)
        {
            int lenS = s.Length;
            int lenT = t.Length;

            double[,] dist = new double[lenS + 1, lenT + 1];

            for (int i = 0; i <= lenS; i++)
                dist[i, 0] = i; // deletion cost = 1

            for (int j = 0; j <= lenT; j++)
                dist[0, j] = j; // insertion cost = 1

            for (int i = 1; i <= lenS; i++)
            {
                for (int j = 1; j <= lenT; j++)
                {
                    double costSub = (s[i - 1] == t[j - 1]) ? 0 : GetSubstitutionCost(s[i - 1], t[j - 1]);

                    dist[i, j] = Math.Min(
                        Math.Min(dist[i - 1, j] + 1,            // deletion
                                 dist[i, j - 1] + 1),           // insertion
                        dist[i - 1, j - 1] + costSub);          // substitution

                    if (i > 1 && j > 1 &&
                        s[i - 1] == t[j - 2] &&
                        s[i - 2] == t[j - 1])
                    {
                        double transCost = GetTranspositionCost(s[i - 2], s[i - 1]);
                        dist[i, j] = Math.Min(dist[i, j], dist[i - 2, j - 2] + transCost);
                    }
                }
            }
            return dist[lenS, lenT];
        }

        static double GetSubstitutionCost(char a, char b)
        {
            // Lower cost for common vowel swaps
            if ((a == 'I' && b == 'E') || (a == 'E' && b == 'I') ||
                (a == 'A' && b == 'E') || (a == 'E' && b == 'A') ||
                (a == 'O' && b == 'U') || (a == 'U' && b == 'O'))
                return 0.5;

            return 1.0;
        }

        static double GetTranspositionCost(char a, char b)
        {
            // Common typo pairs with cheaper cost
            string pair = new string(new[] { a, b });
            string pairReversed = new string(new[] { b, a });

            HashSet<string> commonTypos = new HashSet<string>
            {
                "IE", "EI", "TH", "HT", "ER", "RE", "ON", "NO"
            };

            if (commonTypos.Contains(pair) || commonTypos.Contains(pairReversed))
                return 0.5;

            return 1.0; 
        }
    }
}
