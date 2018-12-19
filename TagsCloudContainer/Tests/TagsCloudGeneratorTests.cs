using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using FluentAssertions;
using TagsCloudContainer.Layouting;
using TagsCloudContainer.Sizing;
using TagsCloudContainer.TagsCloudGenerating;
using TagsCloudContainer.TagsClouds;


namespace TagsCloudContainer.Tests
{
    [TestFixture]
    public class TagsCloudGeneratorTests
    {
        private readonly Size minLetterSize = new Size(16, 20);
        private readonly IWordsSizer wordsSizer = new FrequencyWordsSizer();
        private readonly Point center = new Point(300, 300);
        private ITagsCloudLayouter layouter;
        private static readonly Random Random = new Random();
        private TagsCloudGenerator generator;

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        [SetUp]
        public void SetUp()
        {
            var layouter = new CircularCloudLayouter(new Point(300, 300), new TagsCloudFactory());
            generator = new TagsCloudGenerator(new FrequencyWordsSizer(), layouter);
        }

        [Test]
        public void CreateCloud_CreatesWordsSizesProportionalToFrequency()
        {
            var words = new ReadOnlyCollection<string>(new List<string>() {"aaa", "aaa", "aaa", "bb", "bb", "c"});
            var cloud = generator.CreateCloud(words, minLetterSize);
            var wordsSizes = new Dictionary<string, Size>();
            foreach (var tCWord in cloud.GetValueOrThrow().AddedWords)
            {
                wordsSizes[tCWord.Word] = tCWord.Rectangle.Size;
            }

            wordsSizes["aaa"].Should().BeEquivalentTo(new Size(minLetterSize.Width * 3 * 3, minLetterSize.Height * 3));
            wordsSizes["bb"].Should().BeEquivalentTo(new Size(minLetterSize.Width * 2 * 2, minLetterSize.Height * 2));
            wordsSizes["c"].Should().BeEquivalentTo(new Size(minLetterSize.Width * 1 * 1, minLetterSize.Height * 1));
        }

        [Test]
        public void CreateCloud_CreatesCloudOfWordsInscribedInCircle()
        {
            var words = new List<string>();
            for (var i = 0; i < 100; i++)
            {
                words.Add(RandomString(1));
                words.Add(RandomString(2));
                words.Add(RandomString(3));
                words.Add(RandomString(4));
            }


            var cloud = generator.CreateCloud(new ReadOnlyCollection<string>(words), minLetterSize);
            var totalSquare = cloud.GetValueOrThrow().AddedWords.Sum(x => x.Rectangle.Height * x.Rectangle.Width);
            foreach (var TCWord in cloud.GetValueOrThrow().AddedWords)
            {
                var r = Math.Sqrt(totalSquare / Math.PI);
                var dist = Math.Sqrt(Math.Pow((TCWord.Rectangle.X - center.X), 2) +
                                     Math.Pow((TCWord.Rectangle.Y - center.Y), 2));
                dist.Should().BeLessThan(r * 1.2);
            }
        }
    }
}