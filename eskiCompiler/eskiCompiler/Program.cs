﻿using System;
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
            List<Expression> exprs = new List<Expression>();

            XDocument program1 = XDocument.Load("diana2.xml");
            var blocks = program1.Root.Elements(); 

            foreach (XElement item in blocks)
            {
                var codeSection = parse(item);
                exprs.Add(codeSection);
            }
                       

            var block = Expression.Block(variables, exprs);
            Expression<Action> lambda = Expression.Lambda<Action>(block);
            //Expression<Func<int>> lambda = Expression.Lambda<Func<int>>(block);

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

                case "equals":
                    result = parseEquals(input);
                    break; 

                case "lessThan":
                    result = parseLessThan(input);
                    break;

                case "ifThen":
                    result = parseIfThen(input);
                    break; 

                case "while":
                    result = parseWhile(input);
                    break;

                case "variable":
                    result = ParseVariable(input);
                    break;

                case "increment":
                    result = parseIncrement(input);
                    break;

                case "int": 
                case "string":
                case "bigin":
                case "single":
                case "double":
                case "boolean":
                    result = ParseContstantFromName(input);
                    break; 

                case "diana":
                case "Diana":
                case "d":
                    result = parseDiana(input);
                break;
            }

            return result;
        }

        


        public static Expression parseDiana(XElement input)
        {
            Type _type = typeof(Diana<string>);
            ConstantExpression message = Expression.Constant("amo a Diana<3", typeof(string));

            var wl = _type.GetMethod("print");

            return Expression.Call(wl, message);

        }

        public static ConstantExpression ParseConstant(XElement input)
        {
            string typeStr = input.Attribute("type").Value.ToString();
            Type type = parseType(typeStr);
            string val = input.Value.ToString();

            

            Type genericClass = typeof(FieldBuilder<>);
            Type ConstructedClass = genericClass.MakeGenericType(type); 

            dynamic created = Activator.CreateInstance(ConstructedClass);

            return created.GenerateConstant(val);
     

        }

        public static Expression parseIncrement(XElement input)
        {
            var variable = parse(input.Elements().ToArray()[0]);
            return Expression.PostIncrementAssign(variable);
        }

        public static ConstantExpression ParseContstantFromName(XElement input)
        {
            string typeStr = input.Name.ToString();
            Type type = parseType(typeStr);
            string val = input.Value.ToString();


            Type genericClass = typeof(FieldBuilder<>);
            Type ConstructedClass = genericClass.MakeGenericType(type);

            dynamic created = Activator.CreateInstance(ConstructedClass);

            return created.GenerateConstant(val);
        }

        public static ParameterExpression ParseVariable(XElement input)
        {
            string varName = input.Value;
            ParameterExpression var = variables.Single(v => v.Name == varName);

            return var;
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
            

            Type type = parseType((string)input.Attribute("format"));

            Type genericClass = typeof(Diana<>);
            Type ConstructedClass = genericClass.MakeGenericType(type);

            System.Reflection.MethodInfo writeLine = ConstructedClass.GetMethod("print");

            var expr = parse(input.Elements().ToArray()[0]);

            var wl = Expression.Call(writeLine, expr);

            return wl;

        }

        public static Expression parseAssign(XElement input)
        {
            Type type = parseType((string)input.Attribute("type"));
            string varName = input.Attribute("name").Value;

            var varExpr = variables.SingleOrDefault(v => v.Name == varName);

            if (varExpr == null)
            {
                varExpr = Expression.Parameter(type, varName);
                variables.Add(varExpr);
            }
            
            var assing = Expression.Assign(varExpr, parse(input.Elements().ToArray()[0]));

            return assing;
        }

        public static Expression parseRead(XElement input)
        {
            Type type = parseType((string)input.Attribute("type"));

            Type genericClass = typeof(Diana<>);
            Type ConstructedClass = genericClass.MakeGenericType(type);

            System.Reflection.MethodInfo readLine = ConstructedClass.GetMethod("read");

            var rl = Expression.Call(readLine, Expression.Constant(input.Value.ToString()));

            return rl;
        }

        public static Type parseType(string input)
        {
            Type type;
            switch (input)
            {
                case "int":
                    type = typeof(int);
                    break;
                case "bigint":
                    type = typeof(Int64);
                    break;
                case "single":
                    type = typeof(Single);
                    break;
                case "string":
                    type = typeof(string);
                    break;
                case "boolean":
                    type = typeof(bool);
                    break;
                case "double":
                    type = typeof(double);
                    break;
                default:
                    type = typeof(int);
                    break;
            }

            return type;
        }

        public static Expression parseEquals(XElement input)
        {
            var child = getChildren(input);

            var left = parse(child[0]);
            var right = parse(child[1]);

            return Expression.Equal(left, right);
        }

        public static Expression parseIfThen(XElement input)
        {

            var xCondition = input.Element("condition").Elements().ToArray()[0];

            var test = parse(xCondition);

            var xThen =
                input.Element("then");

            var then = Expression.Block(parseBody(xThen).ToList());

            return Expression.IfThen(test, then);
        }

        public static Expression parseLoopCondition(XElement input, LabelTarget breakExpression)
        {

            var xCondition = input;

            var test = parse(xCondition);
            var negateTest = Expression.Not(test);           

            var then = Expression.Break(breakExpression);

            return Expression.IfThen(negateTest, then);
        } 

        public static IEnumerable<Expression> parseBody(XElement input)
        {
            List<Expression> exprs = new List<Expression>();
            foreach (XElement item in input.Elements())
            {
                //exprs.Add(parse(item));
                yield return parse(item);
            }

            
        }

        public static Expression parseWhile(XElement input)
        {
            var breakExpression = Expression.Label();

            var xCondition = input.Element("condition").Elements().ToArray()[0];

            var test = parseLoopCondition(xCondition, breakExpression);

            var xbody =
                input.Element("body");

            var body = parseBody(xbody).ToList();

            var block = Expression.Block(test, Expression.Block(body)); 


            return Expression.Loop(block, breakExpression);
        }

        public static Expression parseLessThan(XElement input)
        {
            var child = getChildren(input); 
            
            var left = parse(child[0]); 
            var right = parse(child[1]); 

            return Expression.LessThan(left, right);

            
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

    


    public class Diana<T>
    {
        public static T read(string message)
        {
            Console.WriteLine(message);
            return (T)Convert.ChangeType(Console.ReadLine(), typeof(T));
        } 

        public static void print(T message)
        {
            Console.WriteLine(message);
        }
    }

    public class FieldBuilder<T>
    {
        public  ConstantExpression GenerateConstant(string value)
        {
            T _value = (T)Convert.ChangeType(value, typeof(T));
            return Expression.Constant(_value, typeof(T));
        }

        public static ParameterExpression GenerateVariable(string name, string value)
        {
            return Expression.Parameter(typeof(T), name);
        }
    }
}
