using System;
using Newtonsoft.Json;
using Scheduling.SharedPackage.Enums;
using Scheduling.SharedPackage.Extensions;

namespace Scheduling.SharedPackage.Models
{
    public class Job : ModelLogicBase
    {
        // NOTE: A JobIdentifier must be unique only within the context of a SubscriptionName
        public string JobIdentifier { get; private set; }
        public string SubscriptionName { get; private set; }

        [JsonProperty]
        public string DomainName { get; private set; }

        [JsonProperty]
        public bool IsActive { get; private set; }

        [JsonProperty]
        public RepeatEndStrategy RepeatEndStrategy { get; private set; }

        [JsonProperty]
        public RepeatInterval RepeatInterval { get; private set; }

        [JsonProperty]
        public DateTime StartAt { get; private set; }

        [JsonProperty]
        public DateTime? EndAt { get; private set; }

        [JsonProperty]
        public int RepeatOccurrenceNumber { get; private set; }

        // When used, the schedule will be driven off this cron expression.
        // https://www.freeformatter.com/cron-expression-generator-quartz.html
        [JsonProperty]
        public string CronExpressionOverride { get; private set; }

        public string CreatedBy { get; private set; }

        [JsonProperty]
        public string UpdatedBy { get; private set; }

        [JsonProperty]
        public DateTime CreatedAt { get; private set; }

        [JsonProperty]
        public DateTime UpdatedAt { get; private set; }

        public Job(string subscriptionName, string jobIdentifier, string createdBy)
        {
            AssertArguments(subscriptionName.HasValue(), $"The {nameof(SubscriptionName)} is required. This is the messaging topic subscription consumers listen to for their jobs");
            AssertArguments(jobIdentifier.HasValue(), $"A {nameof(JobIdentifier)} is required in order to create a scheduled job");
            AssertArguments(createdBy.HasValue(), "The Id of the user creating this job must be provided");

            JobIdentifier = jobIdentifier;
            SubscriptionName = subscriptionName;
            CreatedBy = createdBy;
            CreatedAt = DateTime.UtcNow;
            SetUpdatedBy(createdBy);
        }

        public void Update(string domainName, DateTime startDate, DateTime? endDate, RepeatEndStrategy repeatEndStrategy,
                           RepeatInterval repeatInterval, int repeatOccurrenceNumber, string updatedBy, string cronExpressionOverride = null)
        {
            AssertAllUpdateArguments(startDate, endDate, repeatEndStrategy, repeatInterval, repeatOccurrenceNumber, cronExpressionOverride);

            StartAt = startDate;
            EndAt = endDate;
            RepeatEndStrategy = repeatEndStrategy;
            RepeatInterval = repeatInterval;
            RepeatOccurrenceNumber = repeatOccurrenceNumber;
            DomainName = domainName;
            CronExpressionOverride = cronExpressionOverride;
            SetUpdatedBy(updatedBy);
        }

        public void SetActivationStatus(bool isActivated, string updatedBy)
        {
            if (IsActive && isActivated || !IsActive && !isActivated)
            {
                return;
            }

            SetUpdatedBy(updatedBy);
            IsActive = isActivated;
        }

        private void SetUpdatedBy(string updatedBy)
        {
            AssertArguments(updatedBy.HasValue(), "The Id of the user updating this schedule is required");

            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        private void AssertAllUpdateArguments(DateTime startDate, DateTime? endDate, RepeatEndStrategy repeatEndStrategy, 
                                              RepeatInterval repeatInterval, int repeatOccurrenceNumber, string cronExpressionOverride)
        {
            AssertArguments(startDate > DateTime.MinValue, "A valid start date must be provided");

            AssertArguments(repeatEndStrategy != RepeatEndStrategy.OnEndDate || endDate >= startDate, "It makes no sense to have an EndDate that's before the StartDate");

            AssertArguments(repeatEndStrategy.DoesNotHaveEndStrategy() || repeatInterval.IsRepeating(),
                $"a {nameof(RepeatEndStrategy)} with no {nameof(RepeatInterval)} doesn't make sense"); 

            AssertArguments(repeatEndStrategy != RepeatEndStrategy.AfterOccurrenceNumber || repeatOccurrenceNumber > 0,
                $"{nameof(RepeatOccurrenceNumber)} must be > 0 if the {nameof(RepeatEndStrategy.AfterOccurrenceNumber)} {nameof(RepeatEndStrategy)} is being used");
        }
    }
}