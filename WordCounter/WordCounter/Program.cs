using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordCounter
{
    /// <summary>
    /// The WordCounter program.
    /// </summary>
    /// <remarks>
    /// As the specs didn't specify anything about performance, so we're considering this app to be performance crictical. Because of that, we're reading
    /// the file stream only once and performing all the processing along the way. This program works with Gigabytes long files, as the file is never
    /// entirely kept in memory. If performance wasn't so important, we could have used some beautiful LINQ to Objects and Regexes to make things prettier :)
    /// </remarks>
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                throw new Exception("The file was not specified");
            }

            var fileName = args[0];
            if (!File.Exists(fileName))
            {
                throw new Exception(String.Format("The specified file name does not exist. File name: {0}", fileName));
            }

            // just so we can assess the performance.
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            // these are the characters that, besides letters and digits, are allowed to constitute a word
            char[] allowedSpecialCharacters = {'/', '\\', '*', '+', '-', '@', '&', '$', '%'};

            // what we're considering to be sentence-breakers
            char[] sentenceBreakers = { '.', '!', '?', '\r', '\n' };

            // what we're considering to be word-breakers
            char[] wordOnlyBreakers = {' ', ',', ':', ';'};

            // the "final" word breaker list contain both "sentenceBreakers" and "wordOnlyBreakers"
            // .NET doesn't provide any straight forward way of copying arrays without using  ToList() and Contact()
            // For the sake of performance, let's copy them the old fashion way :)
            char[] wordBreakers = new char[sentenceBreakers.Length + wordOnlyBreakers.Length];
            sentenceBreakers.CopyTo(wordBreakers, 0);
            wordOnlyBreakers.CopyTo(wordBreakers, wordOnlyBreakers.Length);

            // our word index and their data.
            var words = new SortedDictionary<string, WordData>();

            // this is the "temp" state of the current word. The reason we're not appending characters to strings is because String is 
            // an "immutable" type in C#. Meaning that everytime you append to an existing String, you are creating a new String.
            // For the sake of performance, let's use StringBuilder instead.
            var currentWordBuilder = new StringBuilder();
            
            var currentSentenceIndex = 1;
            var wordsInTheCurrentSentence = 0;

            using (var streamReader = new StreamReader(fileName))
            {
                CountWords(streamReader, wordBreakers, currentWordBuilder, words, currentSentenceIndex, wordsInTheCurrentSentence, sentenceBreakers, allowedSpecialCharacters);
            }

            var index = 0;
            foreach (var wordPair in words)
            {
                var word = wordPair.Key;
                var wordData = wordPair.Value;
                Console.WriteLine(string.Format("{0, -6} {1, -20} {2}", Helpers.ConvertNumericIndexToStringIndex(index), word, wordData ));
                index++;
            }

            stopWatch.Stop();

            Console.WriteLine("");
            Console.WriteLine(string.Format("Time ellapsed: {0}ms", stopWatch.ElapsedMilliseconds));

            Console.ReadKey(false);
        }






        private static void CountWords(StreamReader streamReader, char[] wordBreakers, StringBuilder currentWordBuilder,
            IDictionary<string, WordData> words, int currentSentenceIndex, int wordsInTheCurrentSentence, char[] sentenceBreakers,
            char[] allowedSpecialCharacters)
        {
            // let's iterate through every character from the file. streamReader.Peek() returns the next character from the stream without consuming
            // it, meaning that, if the next character is 0 (EOF), we should stop
            while (streamReader.Peek() >= 0)
            {
                var character = (char) streamReader.Read();
                if (wordBreakers.Contains(character) && currentWordBuilder.Length > 0)
                {
                    // Found a complete word. The reason we're checking 'currentWordBuilder.Length > 0' is because
                    // there can be 2 or more wordBreakers sequentially. In this case, we don't want to add a new word
                    WordData wordData;

                    // the challenge description seems to be case insensitive, so we're assuming it's case insensitive.
                    var word = currentWordBuilder.ToString().ToLower();
                    if (words.ContainsKey(word))
                    {
                        // in this case, the current word has been found already. Let's just update it.
                        wordData = words[word];
                        wordData.SentenceIndexes.Add(currentSentenceIndex);
                    }
                    else
                    {
                        // in this case, the current word has not been found already. Let's register it.
                        wordData = new WordData();
                        wordData.SentenceIndexes.Add(currentSentenceIndex);
                        words.Add(word, wordData);
                    }
                    // currentWordBuilder should be cleared and start again
                    currentWordBuilder.Clear();
                    // wordsInTheCurrentSentence should be incremented;
                    wordsInTheCurrentSentence++;
                }
                else if (sentenceBreakers.Contains(character))
                {
                    // we *probably* found a complete sentence. Needs further investigation.
                    if (wordsInTheCurrentSentence > 0)
                    {
                        // if currentSentenceIndex is 0 and we found a new sentence breaker, that means there were multiple word breakers sequentially.
                        // In this case, we don't want to increment currentSentenceIndex
                        currentSentenceIndex++;
                    }
                }
                else
                {
                    // we're still in the same word and sentense
                    if (char.IsLetterOrDigit(character) || allowedSpecialCharacters.Contains(character))
                    {
                        // we only append a character to the current word if it's allowed
                        currentWordBuilder.Append(character);
                    }
                }
            }
        }
    }
}
