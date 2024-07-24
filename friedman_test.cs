using System.ComponentModel;
using System.Diagnostics.SymbolStore;
using System.Reflection.Metadata;
using System.Text;

namespace Kryptografie_ukoly
{
    public class friedman_test
    {
        private double cz_index_of_coincidence = 0.06027;
        private double eng_index_of_coincidence = 0.06689;
        private double de_index_of_coincidence = 0.07667;
        private double rand_index_of_coincidence = (double)1 / (double)26;
        public string cipher_text { get; set; }

        public friedman_test(string cipher_text) 
        {
            this.cipher_text = cipher_text.ToLower();
        }

        public enum language {
            czech = 0, 
            english = 1,
            german = 2,
        }

        public void run(language lang)
        {
            int max_password_length = 15;
            double acceptable_index_mistake = 0.005;
            double index;
                       
            switch (lang)
            {
                case language.czech:
                    index = cz_index_of_coincidence; break;
                case language.german:
                    index = de_index_of_coincidence; break;
                default:
                    index = eng_index_of_coincidence; break;
            }

            int res = analyze_dataset(max_password_length, acceptable_index_mistake, index);

            double text_len = 0;
            foreach (char c in cipher_text)
                if (char.IsAsciiLetter(c))
                    text_len++;

            // Pouziti vzorce pro vypocet, velmi nepresne, prilis zalezi na textu.
            double res_eq = ((index - rand_index_of_coincidence) * text_len) / ((text_len - 1) * index_of_coincidence(cipher_text) - rand_index_of_coincidence * text_len + index);


            if (res == 0)
                Console.WriteLine("Text was probably ciphered with monosylabic cipher.");
            else
                Console.WriteLine($"Probable length of key is {res}.");
        }

        public double index_of_coincidence(string text)
        {
            text = text.ToLower();

            double text_length = 0;
            double[] letter_occur = new double[26];
            foreach (char c in text)
                if (char.IsAsciiLetter(c))
                {
                    letter_occur[c - 'a']++;
                    text_length++;
                }

            double sum = 0;
            foreach (double c in letter_occur)
                sum += c * (c - 1);
            double result = sum / (text_length * (text_length - 1));

            return result;
        }

        private string[] divide_text(int chunk_size)
        {
            int chunk_count = (cipher_text.Length - (cipher_text.Length % chunk_size)) / chunk_size;
            string[] res = new string[chunk_count];
            string text = cipher_text.Substring(0, chunk_size*chunk_count);

            for (int k = 0; k < chunk_count; k++)
                res[k] = text.Substring(k * chunk_size, chunk_size);

            return res;
        }

        private bool is_equal_within_mistake(double cmp1, double cmp2, double mistake)
        {
            if (cmp1 == cmp2) return true;
            else if (cmp1 < cmp2)
                return cmp1 + mistake >= cmp2;
            else
                return cmp1 - mistake <= cmp2;
        }

        private int analyze_dataset(int max_password_length, double acceptable_index_mistake, double language_index)
        {
            if (is_equal_within_mistake(index_of_coincidence(cipher_text), language_index, acceptable_index_mistake))
                return 0;
            else
            {
                int chunk_size = 2;
                int probable_password_length = 0;
                double iteration_index = 0;

                while (chunk_size <= max_password_length)
                {
                    string[] chunks = divide_text(chunk_size);
                    double[] indexes= new double[chunks.Length];

                    string[] chunks_by_columns = new string[chunk_size];
                    for (int k = 0; k < chunk_size; k++)
                    {
                        var sb = new StringBuilder(chunks.Length);
                        foreach (string chunk in chunks)
                            sb.Append(chunk[k]);
                        chunks_by_columns[k] = sb.ToString();
                    }
                    double average_index = 0;
                    foreach (string column in chunks_by_columns)
                        average_index += index_of_coincidence(column);
                    iteration_index = average_index / chunks_by_columns.Length;

                    if (is_equal_within_mistake(iteration_index, language_index, acceptable_index_mistake))
                    {
                        probable_password_length = chunk_size;
                        break;
                    }
                        
                    else
                        chunk_size++;
                }
                return probable_password_length;
            }
        }
    }
}
