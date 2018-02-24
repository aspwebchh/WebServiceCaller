﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace MySqlDataView.Common {
    class Common {
        public static string GetString( string str,  int len ) {
            if( string.IsNullOrEmpty( str ) ) {
                return "";
            } else {
                if( str.Length <= len ) {
                    return str;
                } else {
                    return str.Substring( 0, len ) + "…";
                }
            }
        }

        public static String FilterHtml( string htmlText ) {
            var result = Regex.Replace( htmlText, "<[^>]+>", "" );
            result = Regex.Replace( result, "&[^;]+;", "" );
            return result;
        }
    }
}
