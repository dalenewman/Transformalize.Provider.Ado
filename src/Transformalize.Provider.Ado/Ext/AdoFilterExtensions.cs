#region license
// Transformalize
// Configurable Extract, Transform, and Load
// Copyright 2013-2017 Dale Newman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Transformalize.Configuration;
using Transformalize.Context;
using Transformalize.Contracts;

namespace Transformalize.Providers.Ado.Ext {

   class ExpressionContinuation {
      public string Expression { get; set; }
      public string Continuation { get; set; }
   }
   public static class SqlFilterExtensions {

      public static char TextQualifier = '\'';
      public static HashSet<string> ListOperators = new HashSet<string>() { "in", "notin" };

      public static string ResolveFilter(this IContext c, IConnectionFactory factory) {

         var builder = new StringBuilder();
         var filtered = new List<ExpressionContinuation>();

         foreach (var filter in c.Entity.Filter) {
            if (filter.Value == filter.IgnoreValue || filter.Value == $"{TextQualifier}{filter.IgnoreValue}{TextQualifier}") {
               continue;  // ignore this filter
            } else {
               var tried = new ExpressionContinuation { Expression = ResolveExpression(c, filter, factory) };
               if (tried.Expression != string.Empty) {
                  filtered.Add(tried);
               }
            }
         }

         var last = filtered.Count - 1;

         for (var i = 0; i < filtered.Count; i++) {
            var filter = filtered[i];

            builder.Append(filter.Expression);

            if (i >= last) {
               continue;
            }

            builder.Append(" ");
            builder.Append(filter.Continuation);
            builder.Append(" ");
         }

         var result = builder.ToString().Trim(' ');

         return result == string.Empty ? string.Empty : $"({builder})";
      }

      private static string ResolveExpression(this IContext c, Filter filter, IConnectionFactory factory) {

         if (!string.IsNullOrEmpty(filter.Expression))
            return filter.Expression;

         var builder = new StringBuilder();
         var leftSide = ResolveSide(filter, "left", factory);
         var op = ResolveOperator(filter.Operator);
         var rightSide = ResolveSide(filter, "right", factory);

         builder.Append(leftSide);
         builder.Append(" ");
         builder.Append(op);
         builder.Append(" ");
         builder.Append(rightSide);

         return builder.ToString();
      }

      private static string ResolveSide(Filter filter, string side, IConnectionFactory factory) {

         bool isField;
         string value;
         bool otherIsField;
         Field otherField;

         if (side == "left") {
            isField = filter.IsField;
            value = filter.Field;
            otherIsField = filter.ValueIsField;
            otherField = filter.ValueField;
         } else {
            isField = filter.ValueIsField;
            value = filter.Value;
            otherIsField = filter.IsField;
            otherField = filter.LeftField;
         }

         if (isField)
            return factory.Enclose(value);

         if (value.Equals("null", StringComparison.OrdinalIgnoreCase))
            return "NULL";

         if (!otherIsField) {
            if (double.TryParse(value, out double number)) {
               return number.ToString(CultureInfo.InvariantCulture);
            }
            return TextQualifier + value + TextQualifier;
         }

         if (ListOperators.Contains(filter.Operator)) {
            var items = new List<string>();
            foreach (var item in value.Split(filter.Delimiter.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)) {
               if (AdoConstants.StringTypes.Any(st => st == otherField.Type)) {
                  items.Add(TextQualifier + item + TextQualifier);
               } else {
                  items.Add(item);
               }
            }
            return "(" + string.Join(",", items) + ")";
         } else {
            if (AdoConstants.StringTypes.Any(st => st == otherField.Type)) {
               return TextQualifier + value + TextQualifier;
            } else {
               return value;
            }
         }
      }

      private static string ResolveOperator(string op) {
         switch (op) {
            case "==":
            case "equal":
            case "equals":
               return "=";
            case "gt":
            case "greaterthan":
               return ">";
            case "gte":
            case "greaterthanequal":
               return ">=";
            case "lt":
            case "lessthan":
               return "<";
            case "lte":
            case "lessthanequal":
               return "<=";
            case "notequal":
            case "notequals":
               return "!=";
            case "in":
               return "IN";
            case "notin":
               return "NOT IN";
            default:
               return "=";
         }

      }

      public static string ResolveOrder(this InputContext context, IConnectionFactory cf) {
         if (!context.Entity.Order.Any())
            return string.Empty;
         var orderBy = string.Join(", ", context.Entity.Order.Select(o => $"{cf.Enclose(o.Field)} {o.Sort.ToUpper()}"));
         context.Debug(() => $"ORDER: {orderBy}");
         return $" ORDER BY {orderBy}";
      }
   }
}