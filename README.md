# ObjectComparator

**This tool allows comparing objects furthermore provide distinctions. What is more, this tool can set compare rule for certain properties or fields.**

[![NuGet.org](https://img.shields.io/nuget/v/ObjectComparator.svg?style=flat-square&label=NuGet.org)](https://www.nuget.org/packages/ObjectComparator/)
[![Build status](https://ci.appveyor.com/api/projects/status/1i6lq6mft1jy94vx/branch/master?svg=true)](https://ci.appveyor.com/project/valeraf23/objectcomparator/branch/master)
## Installation

#### Install with NuGet Package Manager Console
```
Install-Package ObjectComparator
```
#### Install with .NET CLI
```
dotnet add package ObjectComparator
```

## Example:

```csharp
         var actual = new Student
            {
                Name = "Alex",
                Age = 20,
                Vehicle = new Vehicle
                {
                    Model = "Audi"
                },
                Courses = new[]
                {
                    new Course
                    {
                        Name = "Math",
                        Duration = TimeSpan.FromHours(4)
                    },
                    new Course
                    {
                        Name = "Liter",
                        Duration = TimeSpan.FromHours(4)
                    }
                }
            };

            var expected = new Student
            {
                Name = "Bob",
                Age = 20,
                Vehicle = new Vehicle
                {
                    Model = "Opel"
                },
                Courses = new[]
                {
                    new Course
                    {
                        Name = "Math",
                        Duration = TimeSpan.FromHours(3)
                    },
                    new Course
                    {
                        Name = "Literature",
                        Duration = TimeSpan.FromHours(4)
                    }
                }
            };
                
               var result = actual.DeeplyEquals(expected); 
	       
	/*   
	    Path: "Student.Name":
	    Expected Value :Alex
	    Actually Value :Bob
    
	    Path: "Student.Vehicle.Model":
	    Expected Value :Audi
	    Actually Value :Opel
    
	    Path: "Student.Courses[0].Duration":
	    Expected Value :04:00:00
	    Actually Value :03:00:00
    
	    Path: "Student.Courses[1].Name":
	    Expected Value :Liter
	    Actually Value :Literature 
	*/
	    
```
   ## Set strategies for certain properties/fields
   
```csharp
         var result = actual.DeeplyEquals(expected,
                strategy => strategy
                    .Set(x => x.Vehicle.Model, (act, exp) => act.Length == exp.Length)
                    .Set(x => x.Courses[1].Name, (act, exp) => act.StartsWith('L') && exp.StartsWith('L')));  
		    
        /* 
            Path: "Student.Name":
            Expected Value :Alex
            Actually Value :Bob
            
            Path: "Student.Courses[0].Duration":
            Expected Value :04:00:00
            Actually Value :03:00:00
        */
    
  ```

## Set Ignore list for properties/fields

```csharp

    var ignore = new[] {"Name", "Courses", "Vehicle" };
    var result = actual.DeeplyEquals(expected,ignore);
   
     /*
     	Objects are deeply equal
    */
    
```

## Display distinctions for properties/fields which have the custom strategy

```csharp

     var result = actual.DeeplyEquals(expected,
                strategy => strategy
                    .Set(x => x.Vehicle.Model, (act, exp) => act.StartsWith('A') && exp.StartsWith('A')), "Name", "Courses");
		    
    /*
		Path: "Student.Vehicle.Model":
		Expected Value :Audi
		Actually Value :Opel
		Details : (act:(Audi), exp:(Opel)) => (act:(Audi).StartsWith(A) AndAlso exp:(Opel).StartsWith(A))
    */
    
    var skip = new[] {"Vehicle", "Name", "Courses[1].Name"};
            var result = expected.DeeplyEquals(actual,
                str => str.Set(x => x.Courses[0].Duration, (act, exp) => act > TimeSpan.FromHours(3),
                    new Display {Expected = "Expected that Duration should be more that 3 hours"}), skip);
		    
    /*	    
		Path: "Student.Courses[0].Duration":
		Expected Value :Expected that Duration should be more that 3 hours
		Actually Value :04:00:00
		Details : (act:(03:00:00), exp:(04:00:00)) => (act:(03:00:00) > 03:00:00)
   */
  
```

## Display distinctions for Dictionary type

```csharp

    var expected = new Library
            {
                Books = new Dictionary<string, Book>
                {
                    ["hobbit"] = new Book {Pages = 1000, Text = "hobbit Text"},
                    ["murder in orient express"] = new Book {Pages = 500, Text = "murder in orient express Text"},
                    ["Shantaram"] = new Book {Pages = 500, Text = "Shantaram Text"}
                }
            };

            var actual = new Library
            {
                Books = new Dictionary<string, Book>
                {
                    ["hobbit"] = new Book {Pages = 1, Text = "hobbit Text"},
                    ["murder in orient express"] = new Book {Pages = 500, Text = "murder in orient express Text1"},
                    ["Shantaram"] = new Book {Pages = 500, Text = "Shantaram Text"}
                }
            };

            var result = expected.DeeplyEquals(actual);
	    
    /*
		Path: "Library.Books[hobbit].Pages":
		Expected Value :1000
		Actually Value :1

		Path: "Library.Books[murder in orient express].Text":
		Expected Value :murder in orient express Text
		Actually Value :murder in orient express Text1
   */
  
```

## Set ignore Strategy

```csharp

     var act = new Student
            {
                Name = "StudentName",
                Age = 1,
                Courses = new[]
                {
                    new Course
                    {
                        Name = "CourseName"
                    }
                }
            };

      var exp = new Student
            {
                Name = "StudentName1",
                Age = 1,
                Courses = new[]
                {
                    new Course
                    {
                        Name = "CourseName1"
                    }
                }
            };

            var distinctions = act.DeeplyEquals(exp, propName => propName.EndsWith("Name"));
    /*
     	Objects are deeply equal
    */
    
```	

## DeeplyEquals if type(not primities and not Anonymous Type) has Overridden Equals method

```csharp

            var actual = new SomeTest("A");
            var expected = new SomeTest("B");
			
			var result = exp.DeeplyEquals(act);
			
	/*
		Path: "SomeTest":
		Expected Value :ObjectsComparator.Tests.SomeTest
		Actually Value :ObjectsComparator.Tests.SomeTest
		Details : Was used override 'Equals()'
	*/
	
```	

## DeeplyEquals if type has Overridden Equality  method

```csharp		
	/*	
		Path: "SomeTest":
		Expected Value :ObjectsComparator.Tests.SomeTest
		Actually Value :ObjectsComparator.Tests.SomeTest
		Details : == (Equality Operator)
	*/
```	

## Display distinctions for Dictionary type
	
```csharp
		var firstDictionary = new Dictionary<string, string>
            {
                {"Key", "Value"},
                {"AnotherKey", "Value"},
            };

        var secondDictionary = new Dictionary<string, string>
            {
                {"Key", "Value"},
                {"AnotherKey", "AnotherValue"},
            };
			
		var result = firstDictionary.DeeplyEquals(secondDictionary)
			 
			 
	/*	
		Path: "Dictionary<String, String>[AnotherKey]":
		Expected Value :Value
		Actually Value :AnotherValue
	*/
	
```		

## Display distinctions for Anonymous type
		
```csharp
            var actual = new {Integer = 1, String = "Test", Nested = new byte[] {1, 2, 3}};
			var expected = new {Integer = 1, String = "Test", Nested = new byte[] {1, 2, 4}};
			
			var result = exp.DeeplyEquals(act);
			
	/*
		Path: "AnonymousType<Int32, String, Byte[]>.Nested[2]":
		Expected Value :3
		Actually Value :4
	*/
                
```
