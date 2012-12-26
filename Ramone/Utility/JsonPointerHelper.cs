﻿using System;
using System.Linq.Expressions;
using System.Globalization;


namespace Ramone.Utility
{
  public class JsonPointerHelper<TObject>
  {
    private string Separator;


    public JsonPointerHelper(string separator)
    {
      Separator = separator;
    }


    public string GetPath<TProperty>(Expression<Func<TObject, TProperty>> expr)
    {
      return GetPath(expr.Body);
    }


    private string GetPath(Expression expr)
    {
      Console.WriteLine(expr.GetType());
      Console.WriteLine(expr.NodeType);
      if (expr.NodeType == ExpressionType.MemberAccess)
      {
        MemberExpression m = expr as MemberExpression;
        string left = GetPath(m.Expression);
        if (left != null)
          return left + Separator + m.Member.Name;
        else
          return m.Member.Name;
      }
      else if (expr.NodeType == ExpressionType.Call)
      {
        MethodCallExpression m = (MethodCallExpression)expr;
        string left = GetPath(m.Object);
        if (left != null)
          return left + Separator + GetIndexerInvocation(m.Arguments[0]);
        else
          return GetIndexerInvocation(m.Arguments[0]);
      }
      else if (expr.NodeType == ExpressionType.ArrayIndex)
      {
        BinaryExpression b = (BinaryExpression)expr;
        string left = GetPath(b.Left);
        if (left != null)
          return left + Separator + b.Right.ToString();
        else
          return b.Right.ToString();
      }
      else
        return null;
    }


    private static string GetIndexerInvocation(Expression expression)
    {
      Expression converted = Expression.Convert(expression, typeof(object));
      ParameterExpression fakeParameter = Expression.Parameter(typeof(object), null);
      Expression<Func<object, object>> lambda = Expression.Lambda<Func<object, object>>(converted, fakeParameter);
      Func<object, object> func;

      func = lambda.Compile();

      return Convert.ToString(func(null), CultureInfo.InvariantCulture);
    }
  }
}
