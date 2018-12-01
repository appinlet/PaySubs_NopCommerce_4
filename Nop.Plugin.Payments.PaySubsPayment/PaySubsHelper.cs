using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Nop.Plugin.Payments.PaySubs
{
    public static class PaySubsHelper
    {
        public static string CalculateSigningRequest(FormCollection paCollection, string paHashParameter)
        {
            //List<string> list = new List<string>();
            List<string> list = Enumerable.ToList<string>(Enumerable.Select<string, string>(paCollection.Keys.Except( new string[] { "ApprovedUrl", "DeclinedUrl", "URLSProvided" }), (string x) => paCollection[x].First()));
            list.Add(paHashParameter);
            return PaySubsHelper.CalcMD5Hash(string.Join(string.Empty, list));
        }


        public static bool VerifySigningRequest(FormCollection paParams, string paHashParameter)
        {
            if (paParams["p5"].First() != null && Convert.ToString(paParams["p5"].First()).StartsWith("SID"))
            {
                //if it is a SID transaction we can safely ignore the hash check
                return true;
            }
            List<string> list = new List<string>();
            list.Add("p1");
            list.Add("p2");
            list.Add("p3");
            list.Add("p4");
            list.Add("p5");
            list.Add("p6");
            list.Add("p7");
            list.Add("p8");
            list.Add("p9");
            list.Add("p10");
            list.Add("p11");
            list.Add("p12");
            list.Add("pam");
            list.Add("m_1");
            list.Add("m_2");
            list.Add("m_3");
            list.Add("m_4");
            list.Add("m_5");
            list.Add("m_6");
            list.Add("m_7");
            list.Add("m_8");
            list.Add("m_9");
            list.Add("m_10");
            list.Add("CardHolderIpAddr");
            list.Add("MaskedCardNumber");
            list.Add("TransactionType");
            List<string> list2 = list;
            string text = paParams.ContainsKey("Hash") ? paParams["Hash"].First() : "";

            var dict = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>();

            using (List<string>.Enumerator enumerator = Enumerable.ToList<string>(Enumerable.OfType<string>(paParams.Keys)).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    string loKey = enumerator.Current;
                    if (Enumerable.Count<string>(Enumerable.Where<string>(list2, (string x) => x.Equals(loKey))) > 0)
                    {
                        dict.Add(loKey, paParams[loKey]);
                        //paParams.Remove(loKey);
                    }
                }
            }
            var newParams = new FormCollection(dict);
            string text2 = PaySubsHelper.CalculateSigningRequest(newParams, paHashParameter);
            return text.Equals(text2);
        }

        private static string CalcMD5Hash(string paStringToEnrypt)
        {
            string result;
            string stripControlChars = new string(paStringToEnrypt.Where(c => !char.IsControl(c)).ToArray());
            string stripWhitespace = System.Text.RegularExpressions.Regex.Replace(stripControlChars, @"[^\u0009^\u000A^\u000D^\u0020-\u007E]", "*");
            using (MD5 mD = MD5.Create())
            {
                StringBuilder stringBuilder = new StringBuilder();
                byte[] array = mD.ComputeHash(Encoding.UTF8.GetBytes(stripWhitespace));
                byte[] array2 = array;
                for (int i = 0; i < array2.Length; i++)
                {
                    byte b = array2[i];
                    stringBuilder.Append(b.ToString("x2"));
                }
                result = stringBuilder.ToString();
            }
            return result;
        }
    }
}
