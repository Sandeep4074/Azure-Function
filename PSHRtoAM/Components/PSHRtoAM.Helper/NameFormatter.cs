using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Resources;

namespace PSHRtoAM.Helper
{
    public class NameFormatter
    {
        public enum NameType
        {
            FirstName = 0,
            MiddleName = 1,
            LastName = 2,
            Suffix = 3
        }
        public string FormatName(string name, string nameTypeString)
        {
            string formattedName = name;
            //convert input string to NameType enum for conditional processing.  An invalid enum parameter will throw an argumentexception.
            NameType nameTypeEnum = (NameType)Enum.Parse(typeof(NameType), nameTypeString, true);

            //remove leading and trailing spaces
            formattedName.Trim();

            string suffix;
            suffix = RemoveSuffix(formattedName);
            if (suffix.Length > 0)
            {

                formattedName = formattedName.Replace(suffix, String.Empty);

                //remove leading and trailing spaces
                formattedName.Trim();
                suffix.Trim();
            }


            if (nameTypeEnum == NameType.Suffix)
            {
                return suffix;
            }

            //remove non alpha characters
            formattedName = System.Text.RegularExpressions.Regex.Replace(formattedName, "[^A-Za-z- ]", String.Empty);


            //remove the first character if it is a blank space or a hyphen
            if (formattedName.Length > 0)
            {
                if (formattedName.Substring(0, 1) == " " || formattedName.Substring(0, 1) == "-")
                {
                    formattedName = formattedName.Remove(0, 1);
                }
            }
            //if the last character is a hyphen, remove it
            if (formattedName.Length > 0)
            {
                if (formattedName.Substring(formattedName.Length - 1, 1) == "-")
                {
                    formattedName = formattedName.Remove(formattedName.Length - 1, 1);
                }
            }

            //add period to middle name initial
            if (nameTypeEnum == NameType.MiddleName)
            {
                if (formattedName.Length == 1)
                {
                    formattedName = formattedName + ".";
                }
            }


            formattedName = CapText(formattedName, "' ', '-'");

            //if the 6th character is a hyphen or space in last name, remove it
            if (nameTypeEnum == NameType.LastName)
            {
                if (formattedName.Length > 5)
                {
                    if (formattedName.Substring(5, 1) == "-" || formattedName.Substring(5, 1) == " ")
                    {
                        formattedName = formattedName.Remove(5, 1);
                    }
                }
            }


            return formattedName;
        }


        private string CapText(string sText, string sDelimiter)
        {
            try
            {
                if (sText.Length > 0)
                {
                    string sWord;
                    int iCharCount = 0;
                    string sReturnValue = String.Empty;

                    string[] sArray = sText.Split(sDelimiter.ToCharArray());
                    //for each word
                    for (int x = 0; x < sArray.Length; x++)
                    {
                        string word = sArray[x];
                        // 2007/04/25 BDR Added length check for side-by-side delimeters
                        if (word.Length == 0)
                            sWord = String.Empty;
                        else
                            //capitalize the first character
                            sWord = char.ToUpper(word[0]).ToString();

                        //for the rest of the letters, force lowercase
                        for (int i = 1; i < word.Length; i++)
                        {
                            sWord = sWord + char.ToLower(word[i]).ToString();
                        }
                        iCharCount = iCharCount + word.Length;
                        sReturnValue = sReturnValue + sWord;
                        if (iCharCount < sText.Length)
                        {
                            sReturnValue = sReturnValue + sText[iCharCount];
                            iCharCount++;
                        }
                    }
                    sText = sReturnValue;
                }
                return sText;
            }
            catch (Exception e)
            {
                throw new ApplicationException(sText + ": " + e.ToString());
            }
        }


        private string RemoveSuffix(string sName)
        {
            //Suffix string values were previously stored in biztalk configuration file
            //Need to move these to ESSO configuration store when more time permits
            string sSuffixes = "Jr.,Jr,Sr.,Sr,III,iii,3rd,II,ii,2nd,IV,4th,V,5th";
            string sSuffix = String.Empty;
            string sTemp;
            int iIndex = -1;

            // 2007/04/06 BDR Added null check
            if (sSuffixes != null)
            {
                //remove any suffix data from the name
                //to be done - requires database lookup
                string[] sSuffixArray = sSuffixes.Split(new char[] { ',' });

                foreach (string suffix in sSuffixArray)
                {
                    sTemp = sName.ToLower();
                    iIndex = sTemp.IndexOf(suffix.ToLower());
                    if (iIndex >= 0)
                    {
                        if (iIndex >= 1 && sName.Substring(iIndex - 1, 1) == " ")
                        {
                            //todo - validate suffix
                            sSuffix = sName.Substring(iIndex, suffix.Length);
                            break;
                        }
                    }
                }
                //Suffix = sSuffix;
            }
            return sSuffix;
        }
    }
}
