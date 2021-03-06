﻿extern alias dstu2;
extern alias stu3;
// https://github.com/NuGet/Home/issues/4989#issuecomment-311042085

using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Model.Primitives;
// using Hl7.Fhir.FluentPath;
//using Hl7.Fhir.Model;
using Hl7.FhirPath;
using Hl7.FhirPath.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FhirPathTester
{
    public class CustomFluentPathFunctions
    {
        static private SymbolTable _st;
        static public SymbolTable Scope
        {
            get
            {
                if (_st == null)
                {
                    _st = new SymbolTable().AddStandardFP();
                    // _st.Add("rand", (object f) => { return "slim"; });

                    // Custom function that returns the name of the property, rather than its value
                    _st.Add("propname", (object f) =>
                    {
                        if (f is IEnumerable<IElementNavigator>)
                        {
                            object[] bits = (f as IEnumerable<IElementNavigator>).Select(i =>
                            {
                                if (i is stu3.Hl7.Fhir.ElementModel.PocoNavigator)
                                {
                                    return (i as stu3.Hl7.Fhir.ElementModel.PocoNavigator).Name;
                                }
                                if (i is dstu2.Hl7.Fhir.ElementModel.PocoNavigator)
                                {
                                    return (i as dstu2.Hl7.Fhir.ElementModel.PocoNavigator).Name;
                                }
                                return "?";
                            }).ToArray();
                            return FhirValueList.Create(bits);
                        }
                        return FhirValueList.Create(new object[] { "?" } );
                    });
                    _st.Add("pathname", (object f) =>
                    {
                        if (f is IEnumerable<IElementNavigator>)
                        {
                            object[] bits = (f as IEnumerable<IElementNavigator>).Select(i =>
                            {
                                if (i is stu3.Hl7.Fhir.ElementModel.PocoNavigator)
                                {
                                    return (i as stu3.Hl7.Fhir.ElementModel.PocoNavigator).Location;
                                }
                                if (i is dstu2.Hl7.Fhir.ElementModel.PocoNavigator)
                                {
                                    return (i as dstu2.Hl7.Fhir.ElementModel.PocoNavigator).Location;
                                }
                                return "?";
                            }).ToArray();
                            return FhirValueList.Create(bits);
                        }
                        return FhirValueList.Create(new object[] { "?" });
                    });
                    _st.Add("shortpathname", (object f) =>
                    {
                        if (f is IEnumerable<IElementNavigator>)
                        {
                            object[] bits = (f as IEnumerable<IElementNavigator>).Select(i =>
                            {
                                if (i is stu3.Hl7.Fhir.ElementModel.PocoNavigator)
                                {
                                    return (i as stu3.Hl7.Fhir.ElementModel.PocoNavigator).ShortPath;
                                }
                                if (i is dstu2.Hl7.Fhir.ElementModel.PocoNavigator)
                                {
                                    return (i as dstu2.Hl7.Fhir.ElementModel.PocoNavigator).ShortPath;
                                }
                                return "?";
                            }).ToArray();
                            return FhirValueList.Create(bits);
                        }
                        return FhirValueList.Create(new object[] { "?" });
                    });
                    _st.Add("commonpathname", (object f) =>
                    {
                        if (f is IEnumerable<IElementNavigator>)
                        {
                            object[] bits = (f as IEnumerable<IElementNavigator>).Select(i =>
                            {
                                if (i is stu3.Hl7.Fhir.ElementModel.PocoNavigator)
                                {
                                    return (i as stu3.Hl7.Fhir.ElementModel.PocoNavigator).CommonPath;
                                }
                                if (i is dstu2.Hl7.Fhir.ElementModel.PocoNavigator)
                                {
                                    return (i as dstu2.Hl7.Fhir.ElementModel.PocoNavigator).CommonPath;
                                }
                                return "?";
                            }).ToArray();
                            return FhirValueList.Create(bits);
                        }
                        return FhirValueList.Create(new object[] { "?" });
                    });

                    // Custom function for evaluating the date operation (custom Healthconnex)
                    _st.Add("dateadd", (PartialDateTime f, string field, long amount) =>
                    {
                        DateTimeOffset dto = f.ToUniversalTime();
                        int value = (int)amount;

                        // Need to convert the amount and field to compensate for partials
                        //TimeSpan ts = new TimeSpan();

                        switch (field)
                        {
                            case "yy": dto = dto.AddYears(value); break;
                            case "mm": dto = dto.AddMonths(value); break;
                            case "dd": dto = dto.AddDays(value); break;
                            case "hh": dto = dto.AddHours(value); break;
                            case "mi": dto = dto.AddMinutes(value); break;
                            case "ss": dto = dto.AddSeconds(value); break;
                        }
                        PartialDateTime changedDate = PartialDateTime.Parse(PartialDateTime.FromDateTime(dto).ToString().Substring(0, f.ToString().Length));
                        return changedDate;
                    });

                    // Custom LUHN Checksum algorithm
                    // https://rosettacode.org/wiki/Luhn_test_of_credit_card_numbers#C.23
                    _st.Add("LuhnTest", (string str) =>
                    {
                        return str.LuhnCheck();
                    });
                }
                return _st;
            }
        }

    }

    public static class Luhn
    {
        public static bool LuhnCheck(this string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber))
                return true;
            return LuhnCheck(cardNumber.Select(c => c - '0').ToArray());
        }

        private static bool LuhnCheck(this int[] digits)
        {
            return GetCheckValue(digits) == 0;
        }

        private static int GetCheckValue(int[] digits)
        {
            return digits.Select((d, i) => i % 2 == digits.Length % 2 ? ((2 * d) % 10) + d / 5 : d).Sum() % 10;
        }
    }
}
