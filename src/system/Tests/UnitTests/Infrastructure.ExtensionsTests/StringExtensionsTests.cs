﻿using Infrastructure.Extensions;
using System.Threading.Tasks;

namespace Infrastructure.ExtensionsTests
{
    public class StringExtensionsTests
    {
        [Test]
        public async Task ReverseString_NullInput_ReturnsNull_Async()
        {
            const string? input = null;
            string? result = input!.ReverseString();
            await Assert.That(result).IsNull();
        }

        [Test]
        public async Task ReverseString_EmptyString_ReturnsEmptyString_Async()
        {
            string input = string.Empty;
            string result = input.ReverseString();
            await Assert.That(result).IsEqualTo(string.Empty);
        }

        [Test]
        public async Task ReverseString_SingleCharacter_ReturnsSameCharacter_Async()
        {
            const string input = "A";
            string result = input.ReverseString();
            await Assert.That(result).IsEqualTo("A");
        }

        [Test]
        public async Task ReverseString_TwoCharacters_ReturnsSwappedCharacters_Async()
        {
            const string input = "ab";
            string result = input.ReverseString();
            await Assert.That(result).IsEqualTo("ba");
        }

        [Test]
        public async Task ReverseString_Palindrome_ReturnsSamePalindrome_Async()
        {
            const string input = "racecar";
            string result = input.ReverseString();
            await Assert.That(result).IsEqualTo("racecar");
        }

        [Test]
        public async Task ReverseString_NormalString_ReturnsReversedString_Async()
        {
            const string input = "Hello, World!";
            const string expected = "!dlroW ,olleH";
            string result = input.ReverseString();
            await Assert.That(result).IsEqualTo(expected);
        }

        [Test]
        public async Task ReverseString_StringWithSpacesAndSymbols_ReturnsCorrectlyReversed_Async()
        {
            const string input = "  abc 123 !@# ";
            const string expected = " #@! 321 cba  ";
            string result = input.ReverseString();
            await Assert.That(result).IsEqualTo(expected);
        }
    }
}
