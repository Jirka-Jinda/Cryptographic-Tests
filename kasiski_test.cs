using System.Data;
using System.Text.RegularExpressions;

namespace Kryptografie_ukoly
{
    public class kasiski_test
    {
        public string cipher_text { get; set; }
        private match_tree matches { get; set; }

        public kasiski_test(string cipher_text)
        {
            this.cipher_text = cipher_text.ToLower();
            matches = new match_tree(cipher_text);
        }

        public void run(int expected_maximum_password_length, bool print_patterns)
        {
            int probable_result = gather_patterns(3, expected_maximum_password_length, print_patterns);
            int k = 1;
            string res = $"Probable password length is: {probable_result}";
            if (probable_result * 2 < expected_maximum_password_length)
                res += $" or its multiple up to {expected_maximum_password_length}";
            Console.WriteLine(res);
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

        private List<int> prime_factorization(int number, int max_len)
        {
            List<int> primes_up_to_max = new List<int>();
            
            for (int k = 3; k < max_len; k++)
            {
                bool is_prime = true;
                foreach (int prime in primes_up_to_max)
                {
                    if (gcd(prime, k) != 1)
                    {
                        is_prime = false;
                        break;
                    }
                }
                if (is_prime)
                    primes_up_to_max.Add(k);
            }
            int[] primes = primes_up_to_max.ToArray();
            primes.Reverse();

            List<int> find_factors(int[] primes, int number, int iteration)
            {
                List<int> factors = new List<int>();
                int keep_number = number;
                int prev_size = 0;
                int act_size = 0;
                do
                {
                    prev_size = factors.Count;
                    foreach (int prime in primes)
                    {
                        if (number % prime == 0)
                        {
                            number /= prime;
                            factors.Add(prime);
                        }
                    }
                    act_size = factors.Count;
                } while (prev_size < act_size);

                if (number == 1)
                    return factors;
                else if (iteration > 10)
                    return factors;
                else
                    return find_factors(shuffle_array(primes), keep_number, ++iteration);
            }

            return find_factors(primes, number, 0);
        }

        private int[] shuffle_array(int[] arr)
        {
            int n = arr.Length;
            while (n > 1)
            {
                var rng = new Random();
                int k = rng.Next(n--);
                int temp = arr[n];
                arr[n] = arr[k];
                arr[k] = temp;
            }
            return arr;
        }

        private int gather_patterns(int minimal_length, int password_len, bool print_patterns)
        {
            List<node> nodes = new List<node>();

            Console.WriteLine("Gathering viable nodes...");
            tree_traversal(matches.root, minimal_length, nodes);
            Console.WriteLine("Done.");
            Console.WriteLine("Cleaning up duplicate substrings...");
            cleanup_substrings(nodes);
            Console.WriteLine("Done.");

            Console.WriteLine("Evaluating distance and length of remaining nodes...");
            int size = 0;
            foreach (node n in nodes)
                size += n.match_count;

            // id, string begininng index = distance, string length
            int[,] dataset = new int[3, size];
            string[] data_names = new string[size];
            int id_counter = 0;
            foreach (node node in nodes)
            {
                int prev_start = 0;
                for (int k = 0; k < node.match_count; k++)
                {
                    prev_start = cipher_text.IndexOf(node.substring, prev_start+1);
                    data_names[id_counter] = node.substring;
                    dataset[0, id_counter] = id_counter;
                    dataset[1, id_counter] = prev_start;
                    dataset[2, id_counter] = node.substring.Length;
                    id_counter++;
                }
            }
            if (print_patterns)
            {
                for (int i = 0; i < size; i++)
                {
                    Console.Write($"{dataset[0, i]}.\t\"{data_names[i]}\"\t");
                    if (data_names[i].Length < 6)
                        Console.Write("\t");
                    Console.Write($"Index: {dataset[1, i]}\t Length: {dataset[2, i]}\n");
                }
            }

            return analyze_data(dataset, password_len, minimal_length);
        }

        private int analyze_data(int[,] dataset, int password_len, int minimal_length)
        {
            Console.WriteLine("Filtering segments...");
            int probable_password_length = 0;
            int segment_max_len = 0;
            bool end = false;

            while (!end)
            {
                List<int> biggest_segments = new List<int>();

                for (int k = 0; k < dataset.GetLength(1); k++)
                {
                    if (dataset[2, k] > minimal_length)
                        biggest_segments.Add(dataset[1, k]);
                }

                // approx. by gcd
                int[] result_factors_count = new int[password_len];
                List<int> gcd_candidates = new List<int>();
                for (int k = 0; k < biggest_segments.Count - 1; k++)
                {
                    int res = gcd(biggest_segments[k], biggest_segments[k + 1]);
                    if (res != 1)
                        gcd_candidates.Add(res);
                }
                if (!gcd_candidates.All(x => x == 1))
                {
                    foreach(int potential_gcd in gcd_candidates)
                    {
                        if (potential_gcd == 1) continue;
                        else
                        {
                            List<int> factors = prime_factorization(potential_gcd, password_len);
                            List<int> unique_factors = factors.Distinct().ToList();

                            foreach(int factor in unique_factors)
                                result_factors_count[factor]++;
                        }
                    }
                    int max_val = 0;
                    for (int k = 0; k < result_factors_count.Length; k++)
                        if (result_factors_count[k] > max_val && k > 2)
                            max_val = result_factors_count[k];

                    for (int k = 0; k < result_factors_count.Length; k++)
                        if (result_factors_count[k] == max_val)
                            probable_password_length = k;
                }

                if (probable_password_length != 0)
                    end = true;

                segment_max_len--;
            }
            return probable_password_length;
        }

        private void tree_traversal(node node, int minimal_length, List<node> viable_nodes)
        {
            if (node.substring.Length >= minimal_length)
                viable_nodes.Add(node);
            foreach (node child in node.children)
                tree_traversal(child, minimal_length, viable_nodes);
        }

        private void cleanup_substrings(List<node> nodes)
        {
            int k = 0;
            while (k < nodes.Count)
            {
                node act = nodes[k];
                foreach (node n in nodes)
                {
                    if (act == n)
                        continue;
                    else if (n.substring.Contains(act.substring))
                    {
                        nodes.Remove(act);
                        k--;
                        break;
                    }
                }
                k++;
            }
        }
    }

    public class match_tree
    {
        public node root { get; set; }
        public string text { get; set; }

        public match_tree(string text)
        {
            this.text = text;
            root = new node(null, 0, "");
            build_init();
        }

        private void build_tree(node node)
        {
            string[] alpha = Enumerable.Range('a', 26).Select(x => ((char)x).ToString()).ToArray();
            alpha.Append(" ");
            string pat = "";
            foreach (char c in node.substring) 
            {
                pat += c;
                //pat += @"\s?";
            }
            foreach (string c in alpha)
            {
                var temp = pat + c;
                int count = Regex.Matches(text, temp).Count();
                if (count >= 2) 
                {
                    var child = new node(node, count, node.substring + c);
                    node.add_child(child);
                }
            }
            foreach (node n in node.children) 
            {
                build_tree(n);
            }
        }

        private void build_init()
        {
            Console.WriteLine("Building prefix tree...");
            string[] alpha = Enumerable.Range('a', 26).Select(x => ((char)x).ToString()).ToArray();
            foreach (string s in alpha)
            {
                int count = Regex.Matches(text, s).Count;
                if (count > 1)
                {
                    var child = new node(root, count, s);
                    root.add_child(child);
                }
            }
            foreach (node n in root.children)
            {
                build_tree(n);
            }
            Console.WriteLine("Done.");
        }
    }
    public class node
    {
        public node parent { get; set; }
        public int match_count { get; set; }
        public string substring { get; set; }
        public List<node> children { get; set; }

        public node(node parent, int match_count, string substring)
        {
            this.parent = parent;
            this.match_count = match_count;
            this.substring = substring;
            this.children = new List<node>();
        }

        public void add_child(node child)
        {
            children.Add(child);
        }
    }
}