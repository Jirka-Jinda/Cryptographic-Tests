using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kryptografie_ukoly
{
    public class affine_cipher
    {
        private Tuple<int, int> key { get; set; }
        private char[] alphabet_upper = Enumerable.Range('A', 26).Select(x => (char)x).ToArray();
        private char[] alphabet_lower = Enumerable.Range('a', 26).Select(x => (char)x).ToArray();
        private char[] alphabet_upper_reverse = Enumerable.Range(0, 26).Select(x => (char)('Z' - x)).ToArray();
        private char[] alphabet_lower_reverse = Enumerable.Range(0, 26).Select(x => (char)('z' - x)).ToArray();

        public affine_cipher(int a, int b)
        {
            a = a % 26;
            if (gcd(a, 26) == 1)
                key = Tuple.Create(a, b % 26);
            else
                throw new Exception("Funkce neni injektivni.");
        }

        private int gcd(int a, int b)
        {
            while (a != 0 && b != 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }
            return a | b;
        }

        public string cipher(string plain_text)
        {
            var res = new StringBuilder(plain_text.Length);
            foreach (char letter in plain_text)
            {
                if (char.IsAsciiLetterOrDigit(letter))
                {
                    if (char.IsLower(letter))
                    {
                        int index_lower = key.Item1 * (letter - 'a') + key.Item2;
                        res.Append(alphabet_lower[index_lower % alphabet_lower.Length]);
                    }
                    else
                    {
                        int index_upper = key.Item1 * (letter - 'A') + key.Item2;
                        res.Append(alphabet_upper[index_upper % alphabet_upper.Length]);
                    }
                }
                else if (char.IsWhiteSpace(letter))
                    res.Append(letter);
                else
                    throw new Exception("Please use only ascii letters.");
            }
            return res.ToString();
        }

        public string decipher(string cipher_text)
        {
            var res = new StringBuilder(cipher_text.Length);
            bool lower = false;
            char temp;
            foreach (char letter in cipher_text)
            {
                if (char.IsAsciiLetterOrDigit(letter))
                {
                    if (char.IsLower(letter))
                    {
                        lower = true;
                        temp = char.ToUpper(letter);
                    }
                    else
                        temp = letter;

                    int index = (alphabet_upper.Length - key.Item1) * ((temp - 'A') - key.Item2) % alphabet_upper.Length;
                    while (index < 0)
                        index += alphabet_upper.Length;

                    if (lower)
                        res.Append(alphabet_lower[index]);
                    else
                        res.Append(alphabet_upper[index]);
                }
                else if (char.IsWhiteSpace(letter))
                    res.Append(letter);
                else
                    throw new Exception("Please use only ascii letters.");
            }
            return res.ToString();
        }
    }
}

