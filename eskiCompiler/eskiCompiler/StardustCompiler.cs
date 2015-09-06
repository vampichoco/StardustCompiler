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
using System.Text.RegularExpressions;

namespace eskiCompiler
{
    public class StardustCompiler
    {
        public List<ParameterExpression> variables;

        public StardustCompiler()
        {
            variables = new List<ParameterExpression>();
        }

        public Expression AddVariable(string name, object value)
        {
            ParameterExpression param = Expression.Parameter(value.GetType(), name);

            variables.Add(param);
            Expression assign = Expression.Assign(param, Expression.Constant(value));


            return assign;

        }

        public Expression<T> ParseProgram<T>(IEnumerable<XElement> program)
        {
            List<Expression> exprs = new List<Expression>();
           

            foreach (XElement item in program)
            {
                var codeSection = parse(item);
                exprs.Add(codeSection);
            }


            var block = Expression.Block(variables, exprs);
            Expression<T> lambda = Expression.Lambda<T>(block);

            return lambda;

            
        }

        public struct memberMeta
        {
           public string name;
           public Type type;
        }

        static memberMeta GetVariableName<T>(Expression<Func<T>> expr)
        {
            var body = (MemberExpression)expr.Body;

            Type type = body.Member.DeclaringType;
            string name = body.Member.Name;


            return new memberMeta { name = name, type = type };
        }

        public Expression<T> ParseProgram<T, U>(XElement program, Dictionary<string, U> vars)
        {
            List<Expression> exprs = new List<Expression>();

            foreach (KeyValuePair<string, U> v in vars)
            {
                exprs.Add(AddVariable(v.Key, v.Value));
            }

            
            var blocks = program.Elements();
            foreach (XElement item in blocks)
            {
                var codeSection = parse(item);
                exprs.Add(codeSection);
            }


            var block = Expression.Block(variables, exprs);
            Expression<T> lambda = Expression.Lambda<T>(block);

            return lambda;
        }



        public Expression parse(XElement input)
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

                case "invoke":
                    //result = parseInvoke(input);
                    break;

                case "int":
                case "string":
                case "bigint":
                case "single":
                case "double":
                case "boolean":
                    result = ParseContstantFromName(input);
                    break;

                case "xml":
                    result = parseXml(input);
                    break;

                case "write":
                    result = ParseWrite(input);
                    break;

                case "readFile":
                    result = parseReadFile(input);
                    break;
                                    
                case "cin":
                    result = parseCin(input);
                    break;
                case "new":
                    result = parseNew(input);
                    break;
            }

            Console.WriteLine(result.NodeType.ToString() + ": " + result.ToString());

            return result;
        } // parse 

        public Expression parseCin(XElement input)
        {
            Type _type = typeof(Console<string>);
            ConstantExpression message = Expression.Constant("amo a Cin <3", typeof(string));

            var wl = _type.GetMethod("printCin");

            return Expression.Call(wl, message);

        }

        //public static Expression parseInvoke(XElement input)
        //{
        //    string varName = input.Value;
        //    ParameterExpression var = variables.Single(v => v.Name == varName); 

        //    string methodName = (string)input.Attribute("method");

        //    var method = var.Type.GetMember(methodName);

        //    return Expression.Call(method[0], var);
        //}

        public ConstantExpression ParseConstant(XElement input)
        {
            string typeStr = input.Attribute("type").Value.ToString();
            Type type = parseType(typeStr);
            string val = input.Value.ToString();



            Type genericClass = typeof(FieldBuilder<>);
            Type ConstructedClass = genericClass.MakeGenericType(type);

            dynamic created = Activator.CreateInstance(ConstructedClass);

            return created.GenerateConstant(val);


        }

        public Expression parseIncrement(XElement input)
        {
            var variable = parse(input.Elements().ToArray()[0]);
            return Expression.PostIncrementAssign(variable);
        }

        public ConstantExpression ParseContstantFromName(XElement input)
        {
            string typeStr = input.Name.ToString();
            Type type = parseType(typeStr);
            string val = input.Value.ToString();


            Type genericClass = typeof(FieldBuilder<>);
            Type ConstructedClass = genericClass.MakeGenericType(type);

            dynamic created = Activator.CreateInstance(ConstructedClass);

            return created.GenerateConstant(val);
        }

        public ParameterExpression ParseVariable(XElement input)
        {
            string varName = input.Value;
            ParameterExpression var = variables.Single(v => v.Name == varName);

            return var;
        } 



        public Expression parseAdd(XElement input)
        {
            var child = getChildren(input);

            var left = parse(child[0]);
            var right = parse(child[1]);

            return Expression.Add(left, right);
        }

        public Expression parseSubtract(XElement input)
        {
            var child = getChildren(input);

            var left = parse(child[0]);
            var right = parse(child[1]);

            return Expression.Subtract(left, right);
        }

        public Expression parseMultiply(XElement input)
        {
            var child = getChildren(input);

            var left = parse(child[0]);
            var right = parse(child[1]);

            return Expression.Multiply(left, right);
        }

        public Expression parseDivide(XElement input)
        {
            var child = getChildren(input);

            var left = parse(child[0]);
            var right = parse(child[1]);

            return Expression.Divide(left, right);
        }

        public Expression ParseWrite(XElement input)
        {
            Type type = parseType((string)input.Attribute("format"));
            string path = (string)input.Attribute("path");

            Type genericClass = typeof(FileManager<>);
            Type constructedClass = genericClass.MakeGenericType(type);

            System.Reflection.MethodInfo writeFile = constructedClass.GetMethod("write");

            var expr = parse(input.Elements().ToArray()[0]);

            var wf = Expression.Call(writeFile, Expression.Constant(path, typeof(string)), expr);

            return wf;

        }

        public Expression parseReadFile(XElement input)
        {
            Type type = parseType((string)input.Attribute("type"));

            Type genericClass = typeof(FileManager<>);
            Type constructedClass = genericClass.MakeGenericType(type);

            String pathStr = (string)input.Attribute("path");

            Expression path = parse(input.Element("path").Elements().ToArray()[0]);

            System.Reflection.MethodInfo readFile = constructedClass.GetMethod("read");

            return Expression.Call(readFile, path);
        }

        public Expression parsePrint(XElement input)
        {


            Type type = parseType((string)input.Attribute("format"));

            Type genericClass = typeof(Console<>);
            Type ConstructedClass = genericClass.MakeGenericType(type);

            System.Reflection.MethodInfo writeLine = ConstructedClass.GetMethod("print");

            var expr = parse(input.Elements().ToArray()[0]);

            var wl = Expression.Call(writeLine, expr);

            return wl;

        }

        public Expression parseAssign(XElement input)
        {
            Type type = parseType((string)input.Attribute("type"));
            string varName = input.Attribute("name").Value;
            string propertyName = "";
            bool isProperty = false;
            Expression assign = Expression.Empty();
            Expression result = Expression.Empty();

            if (Regex.IsMatch(varName, @"(\w+|[0-9])(\.\w+)+"))
            {
                var matches = Regex.Matches(varName, @"\w+|[0-9]+");
                varName = matches[0].Value;
                propertyName = matches[1].Value;
                isProperty = true;
            }

            var varExpr = variables.SingleOrDefault(v => v.Name == varName);

            if (varExpr == null)
            {

                varExpr = Expression.Parameter(type, varName);
                variables.Add(varExpr);

            }

            if (isProperty)
            {
                //Expression property = Expression.Property(varExpr, propertyName);

                System.Linq.Expressions.MemberExpression member =
                    Expression.Field(varExpr, propertyName);

                assign = Expression.Assign(member, parse(input.Elements().ToArray()[0]));
            }
            else
            {
                assign = Expression.Assign(varExpr, parse(input.Elements().ToArray()[0]));
            }

            result = assign;
            return result;
        }

      

        public Expression parseRead(XElement input)
        {
            Type type = parseType((string)input.Attribute("type"));

            Type genericClass = typeof(Console<>);
            Type ConstructedClass = genericClass.MakeGenericType(type);

            System.Reflection.MethodInfo readLine = ConstructedClass.GetMethod("read");

            var rl = Expression.Call(readLine, Expression.Constant(input.Value.ToString()));

            return rl;
        }

        public Type parseType(string input)
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
                case "xml":
                    type = typeof(XElement);
                    break;
                case "hello":
                    type = typeof(sampleClass);
                    break;
                default:
                    type = typeof(int);
                    break;
            }

            return type;
        }

        public Expression parseEquals(XElement input)
        {
            var child = getChildren(input);

            var left = parse(child[0]);
            var right = parse(child[1]);

            return Expression.Equal(left, right);
        }

        public Expression parseAnd(XElement input)
        {
            var child = getChildren(input);

            var left = parse(child[0]);
            var right = parse(child[1]);

            return Expression.And(left, right);
        }

        public Expression parseOr(XElement input)
        {
            var child = getChildren(input);

            var left = parse(child[0]);
            var right = parse(child[1]);

            return Expression.Or(left, right);
        }

        public Expression parseNot(XElement input)
        {

            var item = input.Elements().ToArray()[0];
            var parsed = parse(item);

            return Expression.Not(parsed);
        }

        public Expression parseIfThen(XElement input)
        {

            var xCondition = input.Element("condition").Elements().ToArray()[0];

            var test = parse(xCondition);

            var xThen =
                input.Element("then");

            var then = Expression.Block(parseBody(xThen).ToList());

            return Expression.IfThen(test, then);
        }

        public Expression parseLoopCondition(XElement input, LabelTarget breakExpression)
        {

            var xCondition = input;

            var test = parse(xCondition);
            var negateTest = Expression.Not(test);

            var then = Expression.Break(breakExpression);

            return Expression.IfThen(negateTest, then);
        }

        public IEnumerable<Expression> parseBody(XElement input)
        {
            List<Expression> exprs = new List<Expression>();
            foreach (XElement item in input.Elements())
            {
                //exprs.Add(parse(item));
                yield return parse(item);
            }


        }

        public Expression parseWhile(XElement input)
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

        public Expression parseLessThan(XElement input)
        {
            var child = getChildren(input);

            var left = parse(child[0]);
            var right = parse(child[1]);

            return Expression.LessThan(left, right);


        }

        public XElement[] getChildren(XElement input)
        {
            if (input.Elements().Count() < 2)
            {
                throw new Exception("Two elementes are required");
            }

            var left = input.Elements().ToArray()[0];
            var right = input.Elements().ToArray()[1];

            return new XElement[2] { left, right };


        }

        public ConstantExpression parseXml(XElement input)
        {
            XElement xml = input.Elements().ToArray()[0];
            return Expression.Constant(xml, typeof(XElement));
        }

        public Expression parseNew(XElement input)
        {
            Type t = parseType(input.Value);

            string name = input.Attribute("name").Value;
            var _new = Expression.New(t);

            var param = Expression.Parameter(t, name);
            variables.Add(param);

            var assign = Expression.Assign(param, _new);

            return assign;

        } //parse New
    } // stardust compiler

    public class Console<T>
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

        public static void printCin(T message)
        {
            ConsoleColor prevColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Magenta;

            Console.WriteLine(message);

            Console.ForegroundColor = prevColor;
        }
    }

    public class FieldBuilder<T>
    {
        public ConstantExpression GenerateConstant(string value)
        {
            T _value = (T)Convert.ChangeType(value, typeof(T));
            return Expression.Constant(_value, typeof(T));
        }

        public static ParameterExpression GenerateVariable(string name, string value)
        {
            return Expression.Parameter(typeof(T), name);
        }


    }

    public class FieldConverter<T, U>
    {
        public static ConstantExpression ConvertType(T input, U output)
        {
            U _value = (U)Convert.ChangeType(input, typeof(U));

            return Expression.Constant(_value, typeof(U));
        }
    }

    public class FileManager<T>
    {
        public static void write(string file, T data)
        {
            System.IO.StreamWriter writer = new System.IO.StreamWriter(file);
            writer.Write(data);
            writer.Close();
        }

        public static T read(string file)
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(file);
            string strData = reader.ReadToEnd();
            reader.Close();

            return (T)Convert.ChangeType(strData, typeof(T));

        }
    }

    public class sampleClass
    {
        public string hello;

        public override string ToString()
        {
            return hello;
        }
    }
    
}
