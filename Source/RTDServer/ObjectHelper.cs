using System.Linq.Expressions;

namespace RTDServer
{
    class ObjectHelper
    {
        public static string GetMemberName(LambdaExpression expresstion)
        {
            if ((expresstion.Body is MemberExpression))
            {
                return (expresstion.Body as MemberExpression).Member.Name;
            }
            else if (expresstion.Body is UnaryExpression)
            {
                if ((expresstion.Body as UnaryExpression).Operand is MemberExpression)
                {
                    return ((expresstion.Body as UnaryExpression).Operand as MemberExpression).Member.Name;
                }
            }
            else if (expresstion.Body is System.Linq.Expressions.MethodCallExpression)
            {
                return (expresstion.Body as System.Linq.Expressions.MethodCallExpression).Method.Name;
            }
            return null;
        }
    }
}