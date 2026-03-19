using Flourish.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Http;
using Google.Apis.Services;

namespace Flourish.Services;

public class GoogleCalendarService(IConfiguration config, ILogger<GoogleCalendarService> logger)
{
    /// <summary>
    /// Creates a Google Calendar event using the reviewer's OAuth tokens so the event
    /// appears directly on their calendar. Uses UserCredential for auto-refresh when possible.
    /// </summary>
    public async Task<(string EventId, string? MeetLink)> CreateReviewEventAsync(
        ReviewEvent reviewEvent,
        User reviewee,
        User reviewer,
        User teamLead,
        string? reviewerAccessToken = null,
        string? reviewerRefreshToken = null)
    {
        try
        {
            var service = BuildCalendarService(reviewerAccessToken, reviewerRefreshToken);

            // Build unique attendee list (team lead may be the same person as reviewer)
            var attendees = new List<EventAttendee>
            {
                new() { Email = reviewee.Email, DisplayName = reviewee.Name },
            };
            if (!string.Equals(reviewer.Email, teamLead.Email, StringComparison.OrdinalIgnoreCase))
                attendees.Add(new EventAttendee { Email = teamLead.Email, DisplayName = teamLead.Name });

            var calendarEvent = new Event
            {
                Summary = $"Performance Review — {reviewee.Name}",
                Description = $"Performance review for {reviewee.Name} with {reviewer.Name}.\n\nReview period: {reviewEvent.ReviewPeriod?.Name}",
                Start = new EventDateTime { DateTimeDateTimeOffset = reviewEvent.ScheduledAt },
                End   = new EventDateTime { DateTimeDateTimeOffset = reviewEvent.ScheduledAt.AddHours(1) },
                Attendees = attendees,
                GuestsCanSeeOtherGuests = true,
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
            // Send email invites to all attendees
            request.SendUpdates = EventsResource.InsertRequest.SendUpdatesEnum.All;

            var created = await request.ExecuteAsync();

            var meetLink = created.ConferenceData?.EntryPoints?
                .FirstOrDefault(ep => ep.EntryPointType == "video")?.Uri;

            logger.LogInformation("Created Calendar event {EventId} (Meet: {MeetLink}) for review of {Reviewee}",
                created.Id, meetLink, reviewee.Name);

            return (created.Id, meetLink);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create Google Calendar event for review of {Reviewee}", reviewee.Name);
            return (string.Empty, null);
        }
    }

    private CalendarService BuildCalendarService(string? accessToken, string? refreshToken)
    {
        IConfigurableHttpClientInitializer credential;

        if (!string.IsNullOrEmpty(refreshToken))
        {
            // Use UserCredential with refresh token — auto-refreshes when the access token expires.
            var clientId = config["Google:ClientId"]!;
            var clientSecret = config["Google:ClientSecret"]!;

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
                Scopes = [CalendarService.Scope.Calendar]
            });

            credential = new UserCredential(flow, "user", new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }
        else if (!string.IsNullOrEmpty(accessToken))
        {
            // Fall back to raw access token (no auto-refresh, may expire after 1 hour).
            credential = GoogleCredential.FromAccessToken(accessToken);
        }
        else
        {
            // Fall back to service account (requires Google:ServiceAccountJson to be configured)
            var serviceAccountJson = config["Google:ServiceAccountJson"];
            if (string.IsNullOrEmpty(serviceAccountJson) || serviceAccountJson == "REPLACE_ME")
                throw new InvalidOperationException(
                    "No reviewer OAuth token provided and Google:ServiceAccountJson is not configured.");

            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(serviceAccountJson));
            credential = GoogleCredential.FromStream(stream)
                .CreateScoped(CalendarService.Scope.Calendar);
        }

        return new CalendarService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "Flourish"
        });
    }
}
