using Microsoft.EntityFrameworkCore.Migrations;

namespace Scheduling.DataAccess.Migrations
{
    public partial class Initial_Migrations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            CreateSchemaIfNotExists(migrationBuilder);
            CreateRepeatEndStrategyTableIfNotExists(migrationBuilder);
            CreateRepeatIntervalTableIfNotExists(migrationBuilder);
            CreateJobTableIfNotExists(migrationBuilder);
        }

        private static void CreateSchemaIfNotExists(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT schema_name 
                    FROM information_schema.schemata 
                    WHERE schema_name = 'scheduling' )
                BEGIN
                    EXEC sp_executesql N'CREATE SCHEMA scheduling;';
                END
            ");
        }

        private static void CreateRepeatIntervalTableIfNotExists(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SET ANSI_NULLS ON
                GO

                SET QUOTED_IDENTIFIER ON
                GO

                IF (NOT EXISTS (SELECT * 
                                 FROM INFORMATION_SCHEMA.TABLES 
                                 WHERE TABLE_SCHEMA = 'scheduling' 
                                 AND  TABLE_NAME = 'RepeatInterval'))
                BEGIN
                    CREATE TABLE [scheduling].[RepeatInterval](
                        [Id] [int] NOT NULL,
                        [Name] [nvarchar](75) NOT NULL,
                     CONSTRAINT [PK_scheduling.RepeatInterval] PRIMARY KEY CLUSTERED 
                    (
                        [Id] ASC
                    )WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
                    ) ON [PRIMARY]
                END
                ");
        }

        private static void CreateRepeatEndStrategyTableIfNotExists(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SET ANSI_NULLS ON
                GO

                SET QUOTED_IDENTIFIER ON
                GO

                IF (NOT EXISTS (SELECT * 
                                 FROM INFORMATION_SCHEMA.TABLES 
                                 WHERE TABLE_SCHEMA = 'scheduling' 
                                 AND  TABLE_NAME = 'RepeatEndStrategy'))
                BEGIN
                    CREATE TABLE [scheduling].[RepeatEndStrategy](
                        [Id] [int] NOT NULL,
                        [Name] [nvarchar](75) NOT NULL,
                     CONSTRAINT [PK_scheduling.RepeatEndStrategy] PRIMARY KEY CLUSTERED 
                    (
                        [Id] ASC
                    )WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
                    ) ON [PRIMARY]
                END
                ");
        }

        private static void CreateJobTableIfNotExists(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SET ANSI_NULLS ON
                GO

                SET QUOTED_IDENTIFIER ON
                GO

                IF (NOT EXISTS (SELECT * 
                                 FROM INFORMATION_SCHEMA.TABLES 
                                 WHERE TABLE_SCHEMA = 'scheduling' 
                                 AND  TABLE_NAME = 'Job'))
                BEGIN
                    CREATE TABLE [scheduling].[Job](
                        [JobId] [int] IDENTITY(1,1) NOT NULL,
                        [JobIdentifier] [varchar](75) NOT NULL,
                        [SubscriptionName] [nvarchar](75) NOT NULL,
                        [DomainName] [nvarchar](2048) NULL,
                        [StartAt] [datetime] NOT NULL,
                        [EndAt] [datetime] NULL,
                        [RepeatEndStrategyId] [int] NOT NULL,
                        [RepeatIntervalId] [int] NOT NULL,
                        [RepeatOccurrenceNumber] [int] NOT NULL,
                        [CronExpressionOverride] [nvarchar](998) NULL,
                        [CreatedBy] [nvarchar](128) NOT NULL,
                        [UpdatedBy] [nvarchar](128) NOT NULL,
                        [CreatedAt] [datetime] NOT NULL,
                        [UpdatedAt] [datetime] NOT NULL,
                        [IsActive] [bit] NOT NULL,
                     CONSTRAINT [PK_scheduling.Job] PRIMARY KEY CLUSTERED 
                    (
                        [JobId] ASC
                    )WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
                    ) ON [PRIMARY]

                    CREATE NONCLUSTERED INDEX [IX_RepeatEndStrategyId] ON [scheduling].[Job]
                    (
                        [RepeatEndStrategyId] ASC
                    )WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]

                    CREATE NONCLUSTERED INDEX [IX_RepeatIntervalId] ON [scheduling].[Job]
                    (
                        [RepeatIntervalId] ASC
                    )WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]

                    SET ANSI_PADDING ON

                    CREATE UNIQUE NONCLUSTERED INDEX [UX_scheduling_Job_JobIdentifier_SubscriptionName] ON [scheduling].[Job]
                    (
                        [JobIdentifier] ASC,
                        [SubscriptionName] ASC
                    )WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]

                    ALTER TABLE [scheduling].[Job] ADD  CONSTRAINT [DF_Job_IsActive]  DEFAULT ((0)) FOR [IsActive]

                    ALTER TABLE [scheduling].[Job]  WITH CHECK ADD  CONSTRAINT [FK_scheduling.Job_scheduling.RepeatEndStrategy_RepeatEndStrategyId] FOREIGN KEY([RepeatEndStrategyId])
                    REFERENCES [scheduling].[RepeatEndStrategy] ([Id])

                    ALTER TABLE [scheduling].[Job] CHECK CONSTRAINT [FK_scheduling.Job_scheduling.RepeatEndStrategy_RepeatEndStrategyId]

                    ALTER TABLE [scheduling].[Job]  WITH CHECK ADD  CONSTRAINT [FK_scheduling.Job_scheduling.RepeatInterval_RepeatIntervalId] FOREIGN KEY([RepeatIntervalId])
                    REFERENCES [scheduling].[RepeatInterval] ([Id])

                    ALTER TABLE [scheduling].[Job] CHECK CONSTRAINT [FK_scheduling.Job_scheduling.RepeatInterval_RepeatIntervalId]
                END
                ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}