using Flourish.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;

namespace Flourish.Services;

public class GoogleCalendarService(IConfiguration config, ILogger<GoogleCalendarService> logger)
{
    public async Task<(string EventId, string? MeetLink)> CreateReviewEventAsync(
        ReviewEvent reviewEvent,
        User reviewee,
        User reviewer,
        User teamLead)
    {
        try
        {
            var credential = await GetCredentialAsync();
            var service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Flourish"
            });

            var calendarEvent = new Event
            {
                Summary = $"Performance Review — {reviewee.Name}",
                Description = $"Performance review for {reviewee.Name} with {reviewer.Name}.\n\nReview period: {reviewEvent.ReviewPeriod?.Name}",
                Start = new EventDateTime { DateTimeDateTimeOffset = reviewEvent.ScheduledAt },
                End = new EventDateTime { DateTimeDateTimeOffset = reviewEvent.ScheduledAt.AddHours(1) },
                Attendees =
                [
                    new EventAttendee { Email = reviewee.Email, DisplayName = reviewee.Name },
                    new EventAttendee { Email = reviewer.Email, DisplayName = reviewer.Name },
                    new EventAttendee { Email = teamLead.Email, DisplayName = teamLead.Name }
                ],
                ConferenceData = new ConferenceData
                {
                    CreateRequest = new CreateConferenceRequest
                    {
                        RequestId = Guid.NewGuid().ToString(),
                        ConferenceSolutionKey = new ConferenceSolutionKey { Type = "hangoutsMeet" }
                    }
                }
            };

            var request = service.Events.Insert(calendarEvent, "primary");
            request.ConferenceDataVersion = 1;
            var created = await request.ExecuteAsync();

            var meetLink = created.ConferenceData?.EntryPoints?
                .FirstOrDefault(ep => ep.EntryPointType == "video")?.Uri;

            return (created.Id, meetLink);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create Google Calendar event for review {ReviewId}", reviewEvent.Id);
            return (string.Empty, null);
        }
    }

    private Task<GoogleCredential> GetCredentialAsync()
    {
        var serviceAccountJson = config["Google:ServiceAccountJson"]
            ?? throw new InvalidOperationException("Google:ServiceAccountJson not configured.");

        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(serviceAccountJson));
        var credential = GoogleCredential
            .FromStream(stream)
            .CreateScoped(CalendarService.Scope.Calendar);
        return Task.FromResult(credential);
    }
}
