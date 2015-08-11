<Query Kind="Program">
  <NuGetReference Version="3.0.3660">RavenDB.Client</NuGetReference>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Raven.Client.Document</Namespace>
</Query>

void Main()
{
	using (var store = new DocumentStore { Url = "http://localhost:8080/", DefaultDatabase = "ddd"})
	{
		store.Initialize();	
		using (var session = store.OpenSession())
		{
			var author = new Author { 
				Name = "George", Surname = "Martin",
				Books = new[] {
					new Book {Title = "A Game of Thrones", Genre = "Fantasy", Price = 12.34m, PublishedOn = new DateTime(1996,8,6)},
					new Book {Title = "A Clash of Kings", Genre = "Fantasy", Price = 23.45m, PublishedOn = new DateTime(1998,11,16)}
				}
			};
			
			session.Store(author);
			session.SaveChanges();
		}
		
		using (var bulkInsert = store.BulkInsert())
			{
				TestData
					.Create()
					.ForEach(x => bulkInsert.Store(x));
			}
		
	}
}



// Define other methods and classes here
public class Author {
	public string Name { get; set; }
	public string Surname { get; set; }
	public Book[] Books { get; set; }
}
public class Book {
	public string Title { get; set; }
	public string Genre { get; set; }
	public DateTime? PublishedOn { get; set; }
	public decimal? Price { get; set; }
}


public static class TestData {
	public static List<Author> Create() {
		return new List<Author> {
			new Author { Name = "Dan", Surname = "Simmons",
				Books = new[] {
					new Book {Title = "Hyperion", Genre = "Science Fiction", Price = 34.56m, PublishedOn = new DateTime(1989,1,1)},
					new Book {Title = "Terror", Genre = "Thriller", Price = 23.45m, PublishedOn = new DateTime(2007,1,8)}
				}
			},
			new Author { Name = "John", Surname = "Tolkien",
				Books = new[] {
					new Book {Title = "Hobbit", Genre = "Fantasy", Price = 12.34m, PublishedOn = new DateTime(1937,9,21)}
				}
			},
			new Author { Name = "Stephen", Surname = "King",
				Books = new[] {
					new Book {Title = "The Stand", Genre = "Horror", Price = 12.34m, PublishedOn = new DateTime(1978,9,1)}
				}
			}		
		};
	}
}