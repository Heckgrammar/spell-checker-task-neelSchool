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

            // 1
            CheckSingleWord(dict);

            // 2
            CheckSentence(dict);

            // 3
            ComputeSpellingScore(dict);

            // 4
            SaveIncorrectWords(dict);

            // Challenge
            SuggestionSystem(dict);
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

        static void CheckSingleWord(HashSet<string> dict)
        {
            Console.Write("Enter a word to check: ");
            string word = Console.ReadLine()?.Trim().ToUpper();
            if (dict.Contains(word))
                Console.WriteLine("Spelled correctly.");
            else
                Console.WriteLine("Incorrect spelling.");
        }

        static void CheckSentence(HashSet<string> dict)
        {
            Console.Write("Enter a sentence to check: ");
            string input = Console.ReadLine()?.ToUpper();
            string[] words = input.Split(new[] { ' ', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                if (!dict.Contains(word))
                    Console.WriteLine($"Incorrect: {word}");
            }
        }

        static void ComputeSpellingScore(HashSet<string> dict)
        {
            Console.Write("Enter a sentence to score: ");
            string input = Console.ReadLine()?.ToUpper();
            string[] words = input.Split(new[] { ' ', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

            int correct = words.Count(dict.Contains);
            int total = words.Length;

            double score = (double)correct / total * 100;
            Console.WriteLine($"Spelling Score: {score:F2}%");
        }

        static void SaveIncorrectWords(HashSet<string> dict)
        {
            Console.Write("Enter a sentence to find and save incorrect words: ");
            string input = Console.ReadLine()?.ToUpper();
            string[] words = input.Split(new[] { ' ', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

            var incorrectWords = words.Where(w => !dict.Contains(w)).Distinct().ToList();
            File.WriteAllLines("IncorrectWords.txt", incorrectWords);
            Console.WriteLine("Incorrect words saved to IncorrectWords.txt.");
        }

        static void SuggestionSystem(HashSet<string> dict)
        {
            Console.Write("Enter a sentence to check and get suggestions: ");
            string input = Console.ReadLine()?.ToUpper();
            string[] words = input.Split(new[] { ' ', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> suggestions = new();

            foreach (string word in words)
            {
                if (!dict.Contains(word))
                {
                    Console.WriteLine($"'{word}' might be incorrect.");
                    string? suggestion = dict
                        .OrderBy(d => LevenshteinDistance(d, word))
                        .FirstOrDefault();

                    if (suggestion != null)
                    {
                        Console.WriteLine($"Did you mean '{suggestion}'? (y/n): ");
                        if (Console.ReadLine()?.ToLower() == "y")
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

        static int LevenshteinDistance(string s, string t)
        {
            int[,] d = new int[s.Length + 1, t.Length + 1];

            for (int i = 0; i <= s.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= t.Length; j++) d[0, j] = j;

            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost = s[i - 1] == t[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[s.Length, t.Length];
        }
    }
}
