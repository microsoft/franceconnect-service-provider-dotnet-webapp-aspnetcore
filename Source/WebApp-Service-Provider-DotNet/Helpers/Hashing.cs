﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security.Cryptography;
using System.Text;

namespace WebApp_Service_Provider_DotNet.Helpers
{
    public static class Hashing
    {
        public static string HashString(string stringToHash)
        {
            using SHA512 sha512 = SHA512.Create();
            var hash = sha512.ComputeHash(Encoding.UTF8.GetBytes(stringToHash));

            StringBuilder stringHashBuilder = new(hash.Length * 2);
            foreach (byte b in hash)
            {
                stringHashBuilder.Append(b.ToString("X2").ToLower());
            }
            return stringHashBuilder.ToString();
        }

    }
}
