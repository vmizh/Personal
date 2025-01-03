using Microsoft.EntityFrameworkCore;
using Personal.Domain.Config;
using Personal.Domain.Entities;
using Personal.Domain.Entities.Base;

namespace Personal.Data.Repositories;

public class AuthorRepository(MongoDBContext dbContext) : BaseRepository<Author>(dbContext), IAuthorRepository
{
    public async Task UpdateReferencesAsync(Guid id)
    {
        var author = await dbContext.Authors.SingleAsync(_ => _._id == id);
        var inc = !string.IsNullOrWhiteSpace(author.FirstName) ? $" {author.FirstName.First()}." : null;
        if(inc != null)
            inc = !string.IsNullOrWhiteSpace(author.SecondName) ? $"{inc}{author.SecondName.First()}." : $"{inc}";
        var authName = $"{author.LastName} {inc}";

        try
        {
            // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataQuery
            var books = await dbContext.Books.Where(_ => _.AuthorList != null && _.AuthorList.Any(a => a.Id == id))
                .ToListAsync();
            foreach (var auth in books.Select(book => book.AuthorList.FirstOrDefault(_ => _.Id == id))
                         .OfType<RefName>())
            {
                auth.Name = authName;
            }
        }
        catch (Exception ex)
        {
        }

       
        await dbContext.SaveChangesAsync();
    }
}
