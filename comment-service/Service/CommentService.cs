using Microsoft.EntityFrameworkCore;

public class CommentService{
    private readonly CommentDbContext _dbContext;
    private readonly ProfanityServiceClient _profanityClient;

    public CommentService(CommentDbContext dbContext, ProfanityServiceClient profanityClient){
        _dbContext = dbContext;
        _profanityClient = profanityClient;
    }

    public async Task<Comment> CreateComment(Comment comment){
        comment.CreatedAt = DateTime.UtcNow;
        try{
            var hasProfanity = await _profanityClient.ContainsProfanity(comment.Content);
            if(hasProfanity)
                comment.IsApproved = false;
                else
                comment.IsApproved = true;
                
            
        } catch{
            comment.IsApproved = false;
        }
        
        _dbContext.Comments.Add(comment);
        await _dbContext.SaveChangesAsync();

        return comment;
    }

    public async Task<List<Comment>> GetCommentsByArticle(Guid articleId){
        return await _dbContext.Comments
            .Where(c => c.ArticleId == articleId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }
}