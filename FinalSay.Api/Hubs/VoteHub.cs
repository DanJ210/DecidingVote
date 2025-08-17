using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FinalSay.Api.Hubs;

[Authorize]
public class VoteHub : Hub
{
    private const string QuestionGroupPrefix = "question-";

    public async Task JoinQuestion(int questionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, QuestionGroupPrefix + questionId);
    }

    public async Task LeaveQuestion(int questionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, QuestionGroupPrefix + questionId);
    }

    public static string GetGroupName(int questionId) => QuestionGroupPrefix + questionId;
}
