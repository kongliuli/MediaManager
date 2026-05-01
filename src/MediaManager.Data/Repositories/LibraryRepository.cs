using MediaManager.Core.Interfaces.Repositories;
using MediaManager.Core.Models;
using MediaManager.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MediaManager.Data.Repositories;

public class LibraryRepository(MediaDbContext dbContext) : ILibraryRepository
{
    public async Task<List<Library>> GetAllAsync(CancellationToken ct = default)
    {
        return await dbContext.Libraries.ToListAsync(ct);
    }

    public async Task<Library?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await dbContext.Libraries.FindAsync([id], ct);
    }

    public async Task<Library?> GetByPathAsync(string path, CancellationToken ct = default)
    {
        return await dbContext.Libraries.FirstOrDefaultAsync(l => l.ScanPath == path, ct);
    }

    public async Task<Library> CreateAsync(Library library, CancellationToken ct = default)
    {
        dbContext.Libraries.Add(library);
        await dbContext.SaveChangesAsync(ct);
        return library;
    }

    public async Task<Library> UpdateAsync(Library library, CancellationToken ct = default)
    {
        dbContext.Entry(library).State = EntityState.Modified;
        await dbContext.SaveChangesAsync(ct);
        return library;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var library = await GetByIdAsync(id, ct);
        if (library != null)
        {
            dbContext.Libraries.Remove(library);
            await dbContext.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> ExistsByPathAsync(string path, CancellationToken ct = default)
    {
        return await dbContext.Libraries.AnyAsync(l => l.ScanPath == path, ct);
    }
}