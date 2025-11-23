<img src="https://r2cdn.perplexity.ai/pplx-full-logo-primary-dark%402x.png" style="height:64px;margin-right:32px"/>

# When examining what kind of expression `telerikFilter.CreateFilterExpression()`generates, it's a LogicalBinaryExpression. So Expression<Func<A, bool>> wouldn't work

You're correct! `CreateFilterExpression` returns an `Expression` (specifically a `BinaryExpression` or `LogicalBinaryExpression`), not a lambda expression. Here's the corrected code:

```csharp
using Telerik.Blazor.Components;
using Telerik.DataSource;
using System.Linq.Expressions;

public static class FilterExpressionCombiner
{
    /// <summary>
    /// Combines Telerik filter expression with custom C.Name filter using OR logic
    /// </summary>
    public static Expression<Func<A, bool>> CombineWithTelerikFilter(
        IFilterDescriptor telerikFilter, 
        string cNameValue)
    {
        // Create parameter for expression tree (shared across both expressions)
        var parameter = Expression.Parameter(typeof(A), "a");
        
        Expression telerikExpression = null;
        
        if (telerikFilter != null)
        {
            // CreateFilterExpression returns Expression (body only), not a lambda
            telerikExpression = telerikFilter.CreateFilterExpression(parameter);
        }
        
        // Create the C.Name filter expression
        var cNameFilterLambda = ExpressionHelper.CreateCNameFilter(cNameValue);
        
        // Extract the body and replace parameter to match our shared parameter
        var cNameBody = new ParameterReplacer(cNameFilterLambda.Parameters[0], parameter)
            .Visit(cNameFilterLambda.Body);
        
        // Combine expressions
        Expression finalBody;
        
        if (telerikExpression == null)
        {
            finalBody = cNameBody;
        }
        else
        {
            // Combine with OR: telerikFilter || cNameFilter
            finalBody = Expression.OrElse(telerikExpression, cNameBody);
        }
        
        // Wrap in lambda
        return Expression.Lambda<Func<A, bool>>(finalBody, parameter);
    }
    
    /// <summary>
    /// Combines two expression bodies with OR logic, using a shared parameter
    /// </summary>
    public static Expression<Func<T, bool>> CombineExpressionsWithOr<T>(
        Expression leftBody,
        Expression rightBody,
        ParameterExpression parameter)
    {
        // Combine with OR
        var combined = Expression.OrElse(leftBody, rightBody);
        
        // Create the final lambda
        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }
    
    /// <summary>
    /// Helper class to replace parameters in expression trees
    /// </summary>
    private class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParameter;
        private readonly ParameterExpression _newParameter;
        
        public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            _oldParameter = oldParameter;
            _newParameter = newParameter;
        }
        
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParameter ? _newParameter : base.VisitParameter(node);
        }
    }
}
```


## Alternative: More Flexible Approach

If you want more control over how you build the expressions:

```csharp
public static class FilterExpressionCombiner
{
    /// <summary>
    /// Combines Telerik filter expression with custom C.Name filter using OR logic
    /// </summary>
    public static Expression<Func<A, bool>> CombineWithTelerikFilter(
        IFilterDescriptor telerikFilter, 
        string cNameValue)
    {
        // Create shared parameter
        var parameter = Expression.Parameter(typeof(A), "a");
        
        // Build list of expression bodies to combine
        var expressions = new List<Expression>();
        
        // Add Telerik filter if present
        if (telerikFilter != null)
        {
            var telerikBody = telerikFilter.CreateFilterExpression(parameter);
            if (telerikBody != null)
            {
                expressions.Add(telerikBody);
            }
        }
        
        // Add C.Name filter
        var cNameBody = CreateCNameFilterBody(parameter, cNameValue);
        expressions.Add(cNameBody);
        
        // Combine all expressions with OR
        Expression combinedBody;
        if (expressions.Count == 1)
        {
            combinedBody = expressions[0];
        }
        else
        {
            // Chain OR operations: expr1 || expr2 || expr3...
            combinedBody = expressions[0];
            for (int i = 1; i < expressions.Count; i++)
            {
                combinedBody = Expression.OrElse(combinedBody, expressions[i]);
            }
        }
        
        // Wrap in lambda
        return Expression.Lambda<Func<A, bool>>(combinedBody, parameter);
    }
    
    /// <summary>
    /// Creates expression body for: a.Bs.Any(b => b.C.Name == value)
    /// </summary>
    private static Expression CreateCNameFilterBody(ParameterExpression aParam, string value)
    {
        // Parameter for inner lambda: b
        var bParam = Expression.Parameter(typeof(B), "b");
        
        // b.C.Name
        var bC = Expression.Property(bParam, nameof(B.C));
        var cName = Expression.Property(bC, nameof(C.Name));
        
        // b.C.Name == value
        var nameValue = Expression.Constant(value, typeof(string));
        var nameEquals = Expression.Equal(cName, nameValue);
        
        // b => b.C.Name == value
        var innerLambda = Expression.Lambda<Func<B, bool>>(nameEquals, bParam);
        
        // a.Bs
        var aBs = Expression.Property(aParam, nameof(A.Bs));
        
        // Enumerable.Any method
        var anyMethod = typeof(Enumerable).GetMethods()
            .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(B));
        
        // a.Bs.Any(b => b.C.Name == value)
        return Expression.Call(null, anyMethod, aBs, innerLambda);
    }
}
```

The key differences:

1. **CreateFilterExpression returns Expression body**: Not a complete lambda, just the expression tree body
2. **Shared parameter**: Both Telerik and your custom filter use the same `ParameterExpression` instance
3. **Combine bodies**: Use `Expression.OrElse` on the expression bodies directly
4. **Wrap at the end**: Only create the `Expression<Func<A, bool>>` lambda at the very end

This approach is cleaner since you don't need to extract and replace parameters - everything uses the same parameter from the start.

