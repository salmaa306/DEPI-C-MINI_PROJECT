using System;
using System.Collections.Generic;
using System.IO;

#region Enums
public enum ExamMode
{
    Starting,
    Queued,
    Finished
}
#endregion

#region Answers
public class Answer
{
    public string Text { get; set; }
    public bool IsCorrect { get; set; }

    public override string ToString() => Text;
}

public class AnswerList : List<Answer> { }
#endregion

#region Questions
public abstract class Question : ICloneable, IComparable<Question>
{
    public string Header { get; set; }
    public string Body { get; set; }
    public int Marks { get; set; }
    public AnswerList Answers { get; set; }

    protected Question(string header, string body, int marks)
    {
        Header = header;
        Body = body;
        Marks = marks;
        Answers = new AnswerList();
    }

    public abstract void Display();

    public object Clone() => MemberwiseClone();

    public int CompareTo(Question other)
        => Marks.CompareTo(other.Marks);

    public override string ToString()
        => $"{Header} | {Body} | Marks: {Marks}";

    public override bool Equals(object obj)
        => obj is Question q && Body == q.Body;

    public override int GetHashCode()
        => Body.GetHashCode();
}

public class TrueFalseQuestion : Question
{
    public TrueFalseQuestion(string h, string b, int m)
        : base(h, b, m) { }

    public override void Display()
        => Console.WriteLine($"{Body} (True / False)");
}

public class ChooseOneQuestion : Question
{
    public ChooseOneQuestion(string h, string b, int m)
        : base(h, b, m) { }

    public override void Display()
        => Console.WriteLine($"{Body} (Choose One)");
}

public class ChooseAllQuestion : Question
{
    public ChooseAllQuestion(string h, string b, int m)
        : base(h, b, m) { }

    public override void Display()
        => Console.WriteLine($"{Body} (Choose All)");
}
#endregion

#region Question List
public class QuestionList : List<Question>
{
    private readonly string filePath;

    public QuestionList(string path)
    {
        filePath = path;
    }

    public new void Add(Question q)
    {
        base.Add(q);

        using StreamWriter writer = new StreamWriter(filePath, true);
        writer.WriteLine(q.ToString());
    }
}
#endregion

#region Subject
public class Subject
{
    public string Name { get; set; }

    public Subject(string name)
    {
        Name = name;
    }
}
#endregion

#region Exam Base
public abstract class Exam
{
    public TimeSpan Time { get; set; }
    public Subject Subject { get; set; }
    public QuestionList Questions { get; set; }
    public Dictionary<Question, Answer> QuestionAnswerMap { get; set; }

    public ExamMode Mode { get; private set; }

    public event Action<string> ExamStarted;

    protected Exam(Subject subject, TimeSpan time)
    {
        Subject = subject;
        Time = time;
        QuestionAnswerMap = new Dictionary<Question, Answer>();
    }

    public void StartExam()
    {
        Mode = ExamMode.Starting;
        ExamStarted?.Invoke($"Exam for {Subject.Name} has started.");
        ShowExam();
        Mode = ExamMode.Finished;
    }

    public abstract void ShowExam();
}
#endregion

#region Exam Types
public class PracticeExam : Exam
{
    public PracticeExam(Subject s, TimeSpan t)
        : base(s, t) { }

    public override void ShowExam()
    {
        Console.WriteLine("Practice Exam:\n");
        foreach (var q in Questions)
        {
            q.Display();
            Console.WriteLine("Correct answer will be shown after finishing.\n");
        }
    }
}

public class FinalExam : Exam
{
    public FinalExam(Subject s, TimeSpan t)
        : base(s, t) { }

    public override void ShowExam()
    {
        Console.WriteLine("Final Exam:\n");
        foreach (var q in Questions)
        {
            q.Display();
            Console.WriteLine();
        }
    }
}
#endregion

#region Students (Event Listener)
public class Student
{
    public string Name { get; set; }

    public void OnExamStarted(string msg)
    {
        Console.WriteLine($"{Name} notified → {msg}");
    }
}
#endregion

#region Main Program
class Program
{
    static void Main()
    {
        Subject subject = new Subject("OOP");

        Student s1 = new Student { Name = "Ahmed" };
        Student s2 = new Student { Name = "Mona" };

        Console.WriteLine("Select Exam Type:");
        Console.WriteLine("1 - Practice Exam");
        Console.WriteLine("2 - Final Exam");

        string choice = Console.ReadLine();

        Exam exam = choice == "1"
            ? new PracticeExam(subject, TimeSpan.FromMinutes(60))
            : new FinalExam(subject, TimeSpan.FromMinutes(60));

        exam.Questions = new QuestionList("QuestionsLog.txt");

        exam.Questions.Add(new TrueFalseQuestion("Q1", "C# supports OOP?", 5));
        exam.Questions.Add(new ChooseOneQuestion("Q2", "Which is a value type?", 5));
        exam.Questions.Add(new ChooseAllQuestion("Q3", "Which are OOP principles?", 10));

        exam.ExamStarted += s1.OnExamStarted;
        exam.ExamStarted += s2.OnExamStarted;

        exam.StartExam();
    }
}
#endregion