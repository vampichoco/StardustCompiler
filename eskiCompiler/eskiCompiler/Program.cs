using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting;
using System.Xml; 
using System.Xml.Linq;
using System.Linq.Expressions;

namespace eskiCompiler
{
    class Program 
    {

        public static List<ParameterExpression> variables; 

        static void Main(string[] args)
        {

            variables = new List<ParameterExpression>();

            XDocument program1 = XDocument.Load("program1.xml");

            var parsedProgram = parse(program1.Root);

            List<Expression> exprs = new List<Expression>();

            exprs.Add(parsedProgram);

                 

            var block = Expression.Block(variables, exprs);

            Expression<Action> lambda = Expression.Lambda<Action>(block);

            lambda.Compile().Invoke();
            
            Console.ReadLine();
            
        }

        public static Expression parse(XElement input)
        {
            Expression result = null;

            string exprName = input.Name.ToString();

            switch (exprName)
            {
                case "constant":
                    result = ParseConstant(input);

                    break;

                case "add":


                    result = parseAdd(input);
                    break;

                case "subtract":

                    result = parseSubtract(input);

                    break;
                case "multiply":

                    result = parseMultiply(input);

                    break;
                case "divide":

                    result = parseDivide(input);

                    break; 

                case "print":
                    result = parsePrint(input);

                    break;

                case "read":

                    result = parseRead(input);

                    break; 

                case "assign":

                    result = parseAssign(input);

                    break;


            }

            return result;
        }

        public static ConstantExpression ParseConstant(XElement input)
        {
            string type = input.Attribute("type").Value.ToString(); 

            if (type == "int")
            {
                return Expression.Constant(Convert.ToInt32(input.Value.ToString()), typeof(Int32));
            }else if (type == "string")
            {
                return Expression.Constant(input.Value.ToString(), typeof(string));
            }
            else if (type == "bigint")
            {
                return Expression.Constant(input.Value.ToString(), typeof(Int64));
            }
            else if (type == "single")
            {
                return Expression.Constant(input.Value.ToString(), typeof(Single));
            }else if (type == "dobule")
            {
                return Expression.Constant(input.Value.ToString(), typeof(Double));
            }

            else
            {
                return Expression.Constant(Convert.ToInt32(input.Value.ToString()), typeof(Int32));
            }

        }

        public static Expression parseAdd(XElement input)
        {
            var child = getChildren(input);

            var left = parse(child[0]);
            var right = parse(child[1]);

            return Expression.Add(left, right);
        }

        public static Expression parseSubtract(XElement input)
        {
            var child = getChildren(input);

            var left = parse(child[0]);
            var right = parse(child[1]);

            return Expression.Subtract(left, right);
        }

        public static Expression parseMultiply(XElement input)
        {
            var child = getChildren(input);

            var left = parse(child[0]);
            var right = parse(child[1]);

            return Expression.Multiply(left, right);
        }

        public static Expression parseDivide(XElement input)
        {
            var child = getChildren(input);

            var left = parse(child[0]);
            var right = parse(child[1]);

            return Expression.Divide(left, right);
        }

        public static Expression parsePrint(XElement input)
        {
            System.Reflection.MethodInfo writeLine;

            string format = input.Attribute("format").Value.ToString();

            if (format == "string")
            {
                writeLine = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) });
            }
            else if (format == "int") {
                writeLine = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(Int32) });
            }else if (format == "bigint")
            {
                writeLine = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(Int64) });
            }else if (format == "single")
            {
                writeLine = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(Single) });
            }
            else if (format == "duble")
            {
                writeLine = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(double) });
            }
            else
            {
                writeLine = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) });
            }

            var expr = parse(input.Elements().ToArray()[0]);

            var wl = Expression.Call(writeLine, expr);

            return wl;
        }

        public static Expression parseAssign(XElement input)
        {
            ParameterExpression varExpr =
                Expression.Parameter(typeof(Int32), input.Attribute("name").Value.ToString());

            variables.Add(varExpr);

            var assing = Expression.Assign(varExpr, parse(input.Elements().ToArray()[0]));

            return assing;
        }

        public static Expression parseRead(XElement input)
        {
            System.Reflection.MethodInfo readLine = typeof(internalMethod).GetMethod("_readLine");

            var rl = Expression.Call(readLine, Expression.Constant(input.Value.ToString()));

            return rl;
        }
            

        public static XElement[] getChildren(XElement input)
        {
            if (input.Elements().Count() < 2)
            {
                throw new Exception("Two elementes are required");
            }

            var left = input.Elements().ToArray()[0];
            var right = input.Elements().ToArray()[1];

            return new XElement[2] { left, right }; 

            
        }


    }

    class internalMethod
    {
        public static Int32 _readLine(string message)
        {
            Console.WriteLine(message);
            return Convert.ToInt32(
                Console.ReadLine());
        }
    }
}
