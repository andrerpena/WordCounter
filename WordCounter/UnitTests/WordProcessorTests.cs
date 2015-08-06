﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WordCounter;

namespace UnitTests
{
    [TestClass]
    public class WordProcessorTests
    {
        /// <summary>
        /// This tests the phrase "He is a strong man. He sure is". We're repeating this phrase over and over with edge cases. The test result must be always the same
        /// </summary>
        /// <param name="words"></param>
        private void TestTheStrongManPhrase(IDictionary<string, WordData> words)
        {
            Assert.AreEqual(6, words.Count);

            Assert.AreEqual(2, words["he"].SentenceIndexes.Count);
            Assert.AreEqual(1, words["he"].SentenceIndexes[0]);
            Assert.AreEqual(2, words["he"].SentenceIndexes[1]);

            Assert.AreEqual(2, words["is"].SentenceIndexes.Count);
            Assert.AreEqual(1, words["is"].SentenceIndexes[0]);
            Assert.AreEqual(2, words["is"].SentenceIndexes[1]);

            Assert.AreEqual(1, words["strong"].SentenceIndexes.Count);
            Assert.AreEqual(1, words["strong"].SentenceIndexes.Count);

            Assert.AreEqual(1, words["man"].SentenceIndexes.Count);
            Assert.AreEqual(1, words["man"].SentenceIndexes.Count);

            Assert.AreEqual(1, words["sure"].SentenceIndexes.Count);
            Assert.AreEqual(2, words["sure"].SentenceIndexes[0]);
        }

        [TestMethod]
        public void Process_BasicUsage()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("He is a strong man. He sure is"));
            var words = WordProcessor.Process(new StreamReader(stream));

            this.TestTheStrongManPhrase(words);
        }

        [TestMethod]
        public void Process_MultipleWordOnlyBreakers()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("He is a           , strong;; man.       ;:    He  sure is"));
            var words = WordProcessor.Process(new StreamReader(stream));

            this.TestTheStrongManPhrase(words);
        }

        [TestMethod]
        public void Process_MultipleConsecutiveSentenceBreakers()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("He is a strong man..... He sure is"));
            var words = WordProcessor.Process(new StreamReader(stream));

            this.TestTheStrongManPhrase(words);
        }

        [TestMethod]
        public void Process_StartingAndEndingWithSentenceBreakers()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(".......He is a strong man..... He sure is!?????"));
            var words = WordProcessor.Process(new StreamReader(stream));

            this.TestTheStrongManPhrase(words);
        }

        [TestMethod]
        public void Process_MultipleLineBreaking()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(@"He is a strong man



He sure is"));
            var words = WordProcessor.Process(new StreamReader(stream));

            this.TestTheStrongManPhrase(words);
        }
    }
}
