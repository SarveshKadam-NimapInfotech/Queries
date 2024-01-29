using System;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using System.Xml.Linq;

namespace Queries
{
    class Program
    {
        static void Main(string[] args)
        {
            var context = new PlutoContext();

            //LINQ Syntax

            var query = 
                from c in context.Courses
                where c.Name.Contains("c#")
                orderby c.Name
                select c;


            foreach (var course in query)
            {
                Console.WriteLine(course.Name);
            }

            //Extension methods

            var courses = context.Courses
                .Where(c => c.Name.Contains("c#"))
                .OrderBy(c => c.Name);

            foreach (var course in courses)
            {
                Console.WriteLine(course.Name);
            }

            //Restriction
            var query1 = 
                from c in context.Courses
                where c.Level == 1 && c.Author.Id == 1
                select c;

            //Ordering
            var query2 =
                from c in context.Courses
                where c.Author.Id == 1
                orderby c.Level, c.Name
                //orderby c.Level descending, c.Name
                select c;

            //Projection
            var query3 =
                from c in context.Courses
                where c.Author.Id == 1
                orderby c.Level descending, c.Name
                select new { Name = c.Name, Author = c.Author.Name };

            //Grouping
            var query4 =
                from c in context.Courses
                group c by c.Level
                into g
                select g;

            foreach (var group in query4)
            {
                Console.WriteLine(group.Key);

                foreach (var course in group)
                {
                    Console.WriteLine("\t{0}", course.Name);
                }

            }

            //now using agregate func // to count the number of courses in each level
            foreach (var group in query4)
            {
                Console.WriteLine("{0} ({1})", group.Key, group.Count());
            }

            //Joining
            //Inner Join
            var query5 = 
                from c in context.Courses
                join a  in context.Authors on c.AuthorId equals a.Id
                select new {CourseName = c.Name, AuthorName = a.Name};

            //groupjoin (leftjoin in sql)
            var query6 =
                from a in context.Authors
                join c in context.Courses on a.Id equals c.AuthorId into g
                select new { AuthorName = a.Name, CoursesCount = g.Count() };

            foreach (var x in query6)
                Console.WriteLine("{0} ({1})", x.AuthorName, x.CoursesCount);

            //crossjoin
            var query7 =
                from a in context.Authors
                from c in context.Courses
                select new { AuthorName = a.Name, CourseName = c.Name };

            foreach (var x in query7)
                Console.WriteLine("{0} - {1}", x.AuthorName, x.CourseName);


            //--------------------------------------------------------------------//

            //Entension Method

            //Restriction
            var courses1 = context.Courses.Where(c => c.Level == 1);

            //Ordering
            var courses2 = context.Courses.Where(c => c.Level == 1).
                OrderBy(c => c.Name).
                ThenBy(c => c.Level);
            //OrderByDescending(c => c.Name).
            //ThenByDescending(c => c.Level);

            //Projection
            var courses3 = context.Courses.Where(c => c.Level == 1).
                OrderBy(c => c.Name).
                ThenBy(c => c.Level).Select(c => new {CourseName = c.Name, AuthorName = c.Author.Name});
            //If we have a list of list objects use SelectMany()
            var tags = context.Courses.Where(c => c.Level == 1).
                OrderBy(c => c.Name).
                ThenBy(c => c.Level).
                SelectMany(c => c.Tags);

            foreach (var t in tags)
            {
                Console.WriteLine(t.Name);
            }

            //Set Operators
            //to make a variable unique we use distint method

            var tag = context.Courses.Where(c => c.Level == 1).
                OrderBy(c => c.Name).
                ThenBy(c => c.Level).
                SelectMany(c => c.Tags)
                .Distinct();

            //Grouping
            var groups = context.Courses.GroupBy(c => c.Level);

            foreach (var group in groups)
            {
                Console.WriteLine("Key: " + group.Key);
                foreach(var course in group)
                {
                    Console.WriteLine("\t" + course.Name);
                }
            }

            //Joining
            //Inner join

            context.Courses.Join(context.Authors,
                c => c.AuthorId,
                a => a.Id,
                (course , author) => new { courseName = course.Name, authorName = author.Name});

            //Group Join
            context.Authors.GroupJoin(context.Courses, a => a.Id, c => c.AuthorId, (author, course) => new { Author = author, Courses = course.Count() });

            //cross Join
            context.Authors.SelectMany(a => context.Courses, (author, course) => new
            {
                AuthorName = author.Name,
                CourseName = course.Name,
            });


            //Partitioning
            //This is usefull when you want to return a page of records, imagine you want to display courses in pages and size of each page is 10.
            var courses4 = context.Courses.Skip(10).Take(10);

            //Element Operators
            context.Courses.OrderBy(c => c.Level).First();
            context.Courses.OrderBy(c => c.Level).FirstOrDefault();
            context.Courses.OrderBy(c => c.Level).Last();
            context.Courses.OrderBy(c => c.Level).LastOrDefault();


            //Quantifying
            var allAbove10Dollars = context.Courses.All(c => c.FullPrice > 10);
            context.Courses.Any(c => c.Level == 1); // Do we have any courses in level 1

            //Aggregating
            var count = context.Courses.Count();
            context.Courses.Where(c => c.Level == 1).Count();
            context.Courses.Max(c => c.FullPrice);
            context.Courses.Min(c => c.FullPrice);
            context.Courses.Average(c => c.FullPrice);


            //Deferred Excecution
            var courses5 = context.Courses.Where(c => c.IsBeginnerCourse == true);
            // now this code will throw an exception because we are using custom property and
            // to use such properties we first have to load the database objects into the query it can be done by tolist.
            context.Courses.ToList().Where(c => c.IsBeginnerCourse == true); // now this code will work

            foreach (var c in courses5)
                Console.WriteLine(c.Name);



        }
    }
}
