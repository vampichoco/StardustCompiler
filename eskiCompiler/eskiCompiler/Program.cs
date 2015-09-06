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
    class Program
    {

        public static List<ParameterExpression> variables;

        static void Main(string[] args)
        {

            variables = new List<ParameterExpression>();
            List<Expression> exprs = new List<Expression>();

            XDocument program1 = XDocument.Load(@"samples\cin.xml");
            var blocks = program1.Root.Elements();

            StardustCompiler compiler = new StardustCompiler();

            var lambda = compiler.ParseProgram<Action>(program1.Root.Elements());

            lambda.Compile().Invoke();

            Console.ReadLine();

        }

    }
}