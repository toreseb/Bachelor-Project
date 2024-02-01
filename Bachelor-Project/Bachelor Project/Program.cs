// See https://aka.ms/new-console-template for more information

using System.Collections;

class Program
{
    static void Main(string[] args)
    {
        ArrayList tests = [];

        tests.Add(new Test(1));
        tests.Add(new Test());
        //tests.Add(new SpecialTest(7, "hello"));
        //tests.Add(new SpecialTest(8));
        tests.Add(new SpecialTest(special:"special2"));
        tests.Add(new SpecialTest());
        tests.Add(4);
        foreach (var item in tests)
        {
            if (item is Test test)
            {
                test.Run();
            }else
            {
                Console.WriteLine("not test");
            }
        }

    }
}

class Test(int number = -1)
{
    protected int number = number;
    protected string testType = "test";

    virtual public void Run()
    {
        Console.WriteLine("Hello World from" + testType + " number: " + number);
    }
}

class SpecialTest : Test
{
    readonly string special;

    public SpecialTest(int number = -1, string special = "normal") : base(number)
    {
        this.special = special;
        testType = "special test";
    }

    override public void Run()
    {
        base.Run();
        Console.WriteLine("From: " + special);
    }
}
