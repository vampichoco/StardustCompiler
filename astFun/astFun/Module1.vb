Imports Microsoft.Scripting.Hosting
Imports Microsoft.Scripting.Ast
Imports Microsoft.Scripting

Module Module1

    Public variables As List(Of Expressions.ParameterExpression)

    Sub Main()
        'Dim program As LambdaBuilder = Microsoft.Scripting.Ast.Utils.Lambda(GetType(Object), "AstTest")

        'Dim st As New List(Of Expressions.Expression)

        'Dim n As Expressions.Expression = program.Variable(GetType(Integer), "n")
        'Dim r As Expressions.Expression = program.Variable(GetType(Integer), "r")

        'st.Add(Expressions.Expression.Assign(n, Expressions.Expression.Constant(4)))
        'st.Add(Expressions.Expression.Assign(r, Expressions.Expression.Constant(2)))

        'Dim sum = Expressions.Expression.Add(n, r)

        'Dim writeLine As System.Reflection.MethodInfo = GetType(Console).GetMethod("WriteLine", New Type() {GetType(Integer)})

        'st.Add(sum)

        'Dim [call] = Expressions.Expression.Call(writeLine, sum)

        'st.Add([call])

        'Dim wow As Integer = program.M 

        variables = New List(Of Expressions.ParameterExpression)

        Dim expr As Expressions.Expression(Of Func(Of Integer)) = Expressions.Expression.Lambda(
            Expressions.Expression.Add(
                Expressions.Expression.Constant(3),
                Expressions.Expression.Constant(2)))

        Dim v = Expressions.Expression.Variable(GetType(Integer), "n")

        Dim expr2 As Expressions.Expression(Of Func(Of Integer)) = Expressions.Expression.Lambda(
            Expressions.Expression.Add(
                Expressions.Expression.Constant(4),
                Expressions.Expression.Assign(v,
                    Expressions.Expression.Constant(4)
                    )
                )
            )

        Dim program1 = <add>
                           <constant>0</constant>
                           <constant>0</constant>
                       </add>

        Dim program2 = <print>
                           <add>
                               <constant>2</constant>
                               <assign name="x">
                                   <read>Write a number</read>
                               </assign>
                           </add>
                       </print>

        'Dim lambda As Expressions.Expression(Of Func(Of Integer)) =
        '    Expressions.Expression.Lambda(
        '        Parse(program2))


        'Dim lambda As Expressions.Expression(Of Action) =
        '    Expressions.Expression.Lambda(Parse(program2))

        

        Dim expr1 = Parse(program2)
        Dim p = Parse(<print><constant>1</constant></print>)

        Dim exprs As New List(Of Expressions.Expression)

        exprs.AddRange({expr1, p})

        Dim fe = Expressions.Expression.Block(variables, exprs)

        Dim lambda As Expressions.Expression(Of Action) =
            Expressions.Expression.Lambda(fe)


        lambda.Compile.Invoke()





        'Dim result As Integer = lambda.Compile().Invoke

        'Console.WriteLine(result)
        'Console.ReadLine()

        Console.ReadLine()

    End Sub

    Public Function Parse(ByVal input As XElement) As Expressions.Expression
        Dim ExprName As String = input.Name.ToString

        Select Case ExprName
            Case "constant"
                Return Expressions.Expression.Constant(
                    Convert.ToInt32(input.Value.ToString), GetType(Integer)
                    )

            Case "add"

                Dim child = GetChildren(input)

                Dim left As Expressions.Expression =
                    Parse(child(0))

                Dim right As Expressions.Expression =
                    Parse(child(1))

                Return Expressions.Expression.Add(left, right)

            Case "subtract"
                Dim child = GetChildren(input)

                Dim left As Expressions.Expression =
                    Parse(child(0))

                Dim right As Expressions.Expression =
                    Parse(child(1))

                Return Expressions.Expression.Subtract(left, right)

            Case "multiply"
                Dim child = GetChildren(input)

                Dim left As Expressions.Expression =
                    Parse(child(0))

                Dim right As Expressions.Expression =
                    Parse(child(1))

                Return Expressions.Expression.Multiply(left, right)

            Case "divide"
                Dim child = GetChildren(input)

                Dim left As Expressions.Expression =
                    Parse(child(0))

                Dim right As Expressions.Expression =
                    Parse(child(1))

                Return Expressions.Expression.Divide(left, right)

            Case "print"

                Dim writeLine As System.Reflection.MethodInfo = GetType(Console).GetMethod("WriteLine", New Type() {GetType(Integer)})
                Dim expr = Parse(input.Elements.ToArray(0))
                Dim wl = Expressions.Expression.Call(writeLine, expr)

                Return wl

            Case "assign"

                Dim var = Expressions.Expression.Parameter(GetType(Integer), input.Attribute("name").Value)
                variables.Add(var)

                Dim assign As Expressions.Expression = Nothing

                If input.HasElements Then
                    assign = Expressions.Expression.Assign(var, Parse(input.Elements.ToArray()(0)))
                Else
                    Dim val As Integer = Convert.ToInt32(input.Value.ToString())
                    assign = Expressions.Expression.Assign(var, Expressions.Expression.Constant(val))
                End If



                Return assign

            Case "read"
                Dim readLine As System.Reflection.MethodInfo =
                    GetType(InternalMethods).GetMethod("_readLine", New Type() {GetType(String)})

                Dim wl = Expressions.Expression.Call(readLine, Expressions.Expression.Constant(input.Value.ToString, GetType(String)))


                Return wl

        End Select



    End Function

    Public Function GetChildren(ByVal input As XElement) As XElement()
        If input.Elements.Count > 2 Then
            Throw New Exception("two elements required at this operation")
        End If
        Return {input.Elements.ToArray(0), input.Elements.ToArray(1)}

    End Function

End Module

Public Class InternalMethods
    Public Shared Function _readLine(ByVal message As String) As Integer
        Console.WriteLine(message)
        Return Convert.ToInt32(Console.ReadLine())
    End Function
End Class

