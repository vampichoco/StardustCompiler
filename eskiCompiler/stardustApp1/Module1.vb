Module Module1

    Sub Main()
        Dim program1 = <program>
                           <equals>
                               <variable>x</variable>
                               <variable>y</variable>
                           </equals>
                       </program>


        Dim parser As New eskiCompiler.StardustCompiler()

        Dim vars As New Dictionary(Of String, Integer)
        vars.Add("x", 2)
        vars.Add("y", 2)
        Dim lambda = parser.ParseProgram(Of Func(Of Boolean), Integer)(program1, vars)

        Console.WriteLine(lambda.Compile.Invoke)

        Console.ReadLine()


    End Sub

End Module
