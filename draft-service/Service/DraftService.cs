using Microsoft.EntityFrameworkCore;

public class DraftService{
    private readonly DraftDbContext _db;
    private readonly ILogger<DraftService> _logger;

    public DraftService(DraftDbContext db, ILogger<DraftService> logger){
        _db = db;
        _logger = logger;
    }

    public async Task<Draft> CreateDraft(Draft draft){
        _logger.LogInformation("Creating draft {DraftId} for author {AuthorId}", draft.Id, draft.AuthorId);

        try{
            _db.Drafts.Add(draft);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Draft {DraftId} successfully stored", draft.Id);

            return draft;
        }
        catch (Exception ex){
            _logger.LogError(ex, "Failed to create draft {DraftId}", draft.Id);
            throw;
        }
    }

    public async Task<List<Draft>> GetDrafts(Guid authorId){
        _logger.LogInformation("Fetching drafts for author {AuthorId}", authorId);

        return await _db.Drafts
            .Where(d => d.AuthorId == authorId)
            .ToListAsync();
    }

    public async Task<Draft?> UpdateDraft(Guid id, Draft updated){
        _logger.LogInformation("Updating draft {DraftId}", id);

        var draft = await _db.Drafts.FindAsync(id);

        if (draft == null){
            _logger.LogWarning("Draft {DraftId} not found", id);
            return null;
        }

        draft.Title = updated.Title;
        draft.Content = updated.Content;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Draft {DraftId} updated", id);

        return draft;
    }

    public async Task<bool> DeleteDraft(Guid id){
        _logger.LogInformation("Deleting draft {DraftId}", id);

        var draft = await _db.Drafts.FindAsync(id);

        if (draft == null){
            _logger.LogWarning("Attempted to delete missing draft {DraftId}", id);
            return false;
        }

        _db.Drafts.Remove(draft);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Draft {DraftId} deleted", id);

        return true;
    }
}