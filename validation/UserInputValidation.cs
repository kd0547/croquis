using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace croquis.validation
{
    static class UserInputValidation
    {
        static string numberPattern = @"^\d+$";

        /*
         * 
         * 
         */
        static bool IsNumberValid(string userInput)
        {
            return Regex.IsMatch(userInput, numberPattern);
        }
    }
}
