﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WinForm = System.Windows.Forms;
using System.Windows.Forms.Integration;


namespace MySqlDataView.Logic {
    class WhereGenerator {
        public static void ClearFilterFormField( UIElement element) {
            if( element is TextBox ) {
                var txtbox = element as TextBox;
                txtbox.Text = "";
            } else if( element is ComboBox ) {
                var combox = element as ComboBox;
                combox.SelectedIndex = 0;
            } else if( element is DatePicker ) {
                var dp = element as DatePicker;
                dp.SelectedDate = null;
            } else if( element is WrapPanel ) {
                var childs = ( element as WrapPanel ).Children;
                foreach( UIElement item in childs ) {
                    ClearFilterFormField( item );
                }
            }
        }

        private static Dictionary<WindowItem, string> GetFilterFormFieldResult( FrameworkElement element ) {
            var result = new Dictionary<WindowItem, string>();
            var windowItem = element.Tag as WindowItem;
            if( element is TextBox ) {
                var txtbox = element as TextBox;
                if( !string.IsNullOrEmpty( txtbox.Text.Trim() ) ) {
                    result[ windowItem ] = txtbox.Text.Trim();
                }
            } else if( element is ComboBox ) {
                var combox = element as ComboBox;
                var selectedValue = combox.SelectedValue.ToString();
                if( !string.IsNullOrEmpty( selectedValue ) ) {
                    result[ windowItem ] = selectedValue;
                }
            } else if( element is DatePicker ) {
                var dp = element as DatePicker;
                if( dp.SelectedDate != null ) {
                    result[ windowItem ] = ( (DateTime)dp.SelectedDate ).ToString( "yyyy-MM-dd" );
                }
            } else if( element is WrapPanel ) {
                var childs = ( element as WrapPanel ).Children;
                foreach( FrameworkElement item in childs ) {
                    var itemResult = GetFilterFormFieldResult( item );
                    foreach( var key in itemResult.Keys ) {
                        result[ key ] = itemResult[ key ];
                    }
                }
            }
            return result;
        }


        private static string GetOperator( WindowItemMatchType matchType ) {
            switch( matchType ) {
                case WindowItemMatchType.Like:
                    return " like ";
                case WindowItemMatchType.Equals:
                    return " = ";
                case WindowItemMatchType.GT:
                    return " >= ";
                case WindowItemMatchType.LT:
                    return " < ";
                default:
                    return " = ";
            }
        }

        public static string GetWhere( FrameworkElement element ) {
            Dictionary<WindowItem, string> formFieldsData = GetFilterFormFieldResult(element);
            var where = "";
            foreach( var key in formFieldsData.Keys ) {
                if( key is WindowItemForDatabase ) {
                    continue;
                }
                var val = formFieldsData [key];
                var matchType = key.MatchType;
                if( matchType == WindowItemMatchType.FULLTEXT ) {
                    var fullTextCondition = "match(" + key.Name + ") AGAINST('" + val + "' in boolean  mode )";
                    if( string.IsNullOrEmpty( where ) ) {
                        where = fullTextCondition;
                    } else {
                        where += " and " + fullTextCondition;
                    }
                    continue;
                }

                if( matchType == WindowItemMatchType.Like ) {
                    var subOp = "";
                    string[] items = new string[] { val };
                    if( val.IndexOf( "&" ) != -1 ) {
                        items = val.Split( new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries );
                        subOp = " and ";
                    } else if( val.IndexOf( "|" ) != -1 ) {
                        items = val.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                        subOp = " or ";
                    }

                   
                    var likeWhereItems = new List<string>();

                    foreach( string item in items ) {
                        var likeWhereItem = key.Name + " like '%" + item + "%'";
                        likeWhereItems.Add( likeWhereItem );
                    }

                    var likeWhere = " (" + String.Join( subOp, likeWhereItems ) + ") ";
                    if( string.IsNullOrEmpty( where ) ) {
                        where = likeWhere;
                    } else {
                        where += " and " + likeWhere;
                    }
                    continue;
                }

                var op = GetOperator( matchType );
                var right =  "'" + val + "'";
                if( string.IsNullOrEmpty( where ) ) {
                    where = key.Name + op + right;
                } else {
                    where += " and " + key.Name + op + right;
                }
            }
            return where;
        }


        public static string GetConnectionString( FrameworkElement element ) {
            Dictionary<WindowItem, string> formFieldsData = GetFilterFormFieldResult( element );
            foreach( var key in formFieldsData.Keys ) {
                if( key is WindowItemForDatabase ) {
                    var val = formFieldsData[ key ];
                    return val;
                }
            }
            return string.Empty;
        }
    }
}
