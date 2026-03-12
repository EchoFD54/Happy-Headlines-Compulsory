using Microsoft.EntityFrameworkCore;

public class ProfanityService{
    private readonly ProfanityDbContext _dbContext;

    public ProfanityService(ProfanityDbContext dbContext){
        _dbContext = dbContext;
    }

    public async Task<bool> ContainsProfanity(string text){
        var words = await _dbContext.ProfanityWords
            .Select(w => w.Word.ToLower())
            .ToListAsync();

        text = text.ToLower();

        foreach (var word in words){
            if (text.Contains(word))
                return true;
        }

        return false;
    }

    public async Task<List<ProfanityWord>> GetAllWords(){
        return await _dbContext.ProfanityWords.ToListAsync();
    }

    public async Task<ProfanityWord> AddWord(ProfanityWord word){
        _dbContext.ProfanityWords.Add(word);
        await _dbContext.SaveChangesAsync();

        return word;
    }
}