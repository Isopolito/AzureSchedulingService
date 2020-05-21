using Microsoft.EntityFrameworkCore.Migrations;

namespace Scheduling.DataAccess.Migrations
{
    public partial class Create_Quartz_Tables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
-- this script is for SQL Server and Azure SQL

IF OBJECT_ID(N'[scheduling].[FK_quartz_TRIGGERS_quartz_JOB_DETAILS]', N'F') IS NOT NULL
ALTER TABLE [scheduling].[quartz_TRIGGERS] DROP CONSTRAINT [FK_quartz_TRIGGERS_quartz_JOB_DETAILS];
GO

IF OBJECT_ID(N'[scheduling].[FK_quartz_CRON_TRIGGERS_quartz_TRIGGERS]', N'F') IS NOT NULL
ALTER TABLE [scheduling].[quartz_CRON_TRIGGERS] DROP CONSTRAINT [FK_quartz_CRON_TRIGGERS_quartz_TRIGGERS];
GO

IF OBJECT_ID(N'[scheduling].[FK_quartz_SIMPLE_TRIGGERS_quartz_TRIGGERS]', N'F') IS NOT NULL
ALTER TABLE [scheduling].[quartz_SIMPLE_TRIGGERS] DROP CONSTRAINT [FK_quartz_SIMPLE_TRIGGERS_quartz_TRIGGERS];
GO

IF OBJECT_ID(N'[scheduling].[FK_quartz_SIMPROP_TRIGGERS_quartz_TRIGGERS]', N'F') IS NOT NULL
ALTER TABLE [scheduling].[quartz_SIMPROP_TRIGGERS] DROP CONSTRAINT [FK_quartz_SIMPROP_TRIGGERS_quartz_TRIGGERS];
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[scheduling].[FK_quartz_JOB_LISTENERS_quartz_JOB_DETAILS]') AND parent_object_id = OBJECT_ID(N'[scheduling].[quartz_JOB_LISTENERS]'))
ALTER TABLE [scheduling].[quartz_JOB_LISTENERS] DROP CONSTRAINT [FK_quartz_JOB_LISTENERS_quartz_JOB_DETAILS];

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[scheduling].[FK_quartz_TRIGGER_LISTENERS_quartz_TRIGGERS]') AND parent_object_id = OBJECT_ID(N'[scheduling].[quartz_TRIGGER_LISTENERS]'))
ALTER TABLE [scheduling].[quartz_TRIGGER_LISTENERS] DROP CONSTRAINT [FK_quartz_TRIGGER_LISTENERS_quartz_TRIGGERS];


IF OBJECT_ID(N'[scheduling].[quartz_CALENDARS]', N'U') IS NOT NULL
DROP TABLE [scheduling].[quartz_CALENDARS];
GO

IF OBJECT_ID(N'[scheduling].[quartz_CRON_TRIGGERS]', N'U') IS NOT NULL
DROP TABLE [scheduling].[quartz_CRON_TRIGGERS];
GO

IF OBJECT_ID(N'[scheduling].[quartz_BLOB_TRIGGERS]', N'U') IS NOT NULL
DROP TABLE [scheduling].[quartz_BLOB_TRIGGERS];
GO

IF OBJECT_ID(N'[scheduling].[quartz_FIRED_TRIGGERS]', N'U') IS NOT NULL
DROP TABLE [scheduling].[quartz_FIRED_TRIGGERS];
GO

IF OBJECT_ID(N'[scheduling].[quartz_PAUSED_TRIGGER_GRPS]', N'U') IS NOT NULL
DROP TABLE [scheduling].[quartz_PAUSED_TRIGGER_GRPS];
GO

IF  OBJECT_ID(N'[scheduling].[quartz_JOB_LISTENERS]', N'U') IS NOT NULL
DROP TABLE [scheduling].[quartz_JOB_LISTENERS];

IF OBJECT_ID(N'[scheduling].[quartz_SCHEDULER_STATE]', N'U') IS NOT NULL
DROP TABLE [scheduling].[quartz_SCHEDULER_STATE];
GO

IF OBJECT_ID(N'[scheduling].[quartz_LOCKS]', N'U') IS NOT NULL
DROP TABLE [scheduling].[quartz_LOCKS];
GO
IF OBJECT_ID(N'[scheduling].[quartz_TRIGGER_LISTENERS]', N'U') IS NOT NULL
DROP TABLE [scheduling].[quartz_TRIGGER_LISTENERS];


IF OBJECT_ID(N'[scheduling].[quartz_JOB_DETAILS]', N'U') IS NOT NULL
DROP TABLE [scheduling].[quartz_JOB_DETAILS];
GO

IF OBJECT_ID(N'[scheduling].[quartz_SIMPLE_TRIGGERS]', N'U') IS NOT NULL
DROP TABLE [scheduling].[quartz_SIMPLE_TRIGGERS];
GO

IF OBJECT_ID(N'[scheduling].[quartz_SIMPROP_TRIGGERS]', N'U') IS NOT NULL
DROP TABLE [scheduling].[quartz_SIMPROP_TRIGGERS];
GO

IF OBJECT_ID(N'[scheduling].[quartz_TRIGGERS]', N'U') IS NOT NULL
DROP TABLE [scheduling].[quartz_TRIGGERS];
GO

CREATE TABLE [scheduling].[quartz_CALENDARS] (
  [SCHED_NAME] nvarchar(120) NOT NULL,
  [CALENDAR_NAME] nvarchar(200) NOT NULL,
  [CALENDAR] varbinary(max) NOT NULL
);
GO

CREATE TABLE [scheduling].[quartz_CRON_TRIGGERS] (
  [SCHED_NAME] nvarchar(120) NOT NULL,
  [TRIGGER_NAME] nvarchar(150) NOT NULL,
  [TRIGGER_GROUP] nvarchar(150) NOT NULL,
  [CRON_EXPRESSION] nvarchar(120) NOT NULL,
  [TIME_ZONE_ID] nvarchar(80) 
);
GO

CREATE TABLE [scheduling].[quartz_FIRED_TRIGGERS] (
  [SCHED_NAME] nvarchar(120) NOT NULL,
  [ENTRY_ID] nvarchar(140) NOT NULL,
  [TRIGGER_NAME] nvarchar(150) NOT NULL,
  [TRIGGER_GROUP] nvarchar(150) NOT NULL,
  [INSTANCE_NAME] nvarchar(200) NOT NULL,
  [FIRED_TIME] bigint NOT NULL,
  [SCHED_TIME] bigint NOT NULL,
  [PRIORITY] int NOT NULL,
  [STATE] nvarchar(16) NOT NULL,
  [JOB_NAME] nvarchar(150) NULL,
  [JOB_GROUP] nvarchar(150) NULL,
  [IS_NONCONCURRENT] bit NULL,
  [REQUESTS_RECOVERY] bit NULL 
);
GO

CREATE TABLE [scheduling].[quartz_PAUSED_TRIGGER_GRPS] (
  [SCHED_NAME] nvarchar(120) NOT NULL,
  [TRIGGER_GROUP] nvarchar(150) NOT NULL 
);
GO

CREATE TABLE [scheduling].[quartz_SCHEDULER_STATE] (
  [SCHED_NAME] nvarchar(120) NOT NULL,
  [INSTANCE_NAME] nvarchar(200) NOT NULL,
  [LAST_CHECKIN_TIME] bigint NOT NULL,
  [CHECKIN_INTERVAL] bigint NOT NULL
);
GO

CREATE TABLE [scheduling].[quartz_LOCKS] (
  [SCHED_NAME] nvarchar(120) NOT NULL,
  [LOCK_NAME] nvarchar(40) NOT NULL 
);
GO

CREATE TABLE [scheduling].[quartz_JOB_DETAILS] (
  [SCHED_NAME] nvarchar(120) NOT NULL,
  [JOB_NAME] nvarchar(150) NOT NULL,
  [JOB_GROUP] nvarchar(150) NOT NULL,
  [DESCRIPTION] nvarchar(250) NULL,
  [JOB_CLASS_NAME] nvarchar(250) NOT NULL,
  [IS_DURABLE] bit NOT NULL,
  [IS_NONCONCURRENT] bit NOT NULL,
  [IS_UPDATE_DATA] bit NOT NULL,
  [REQUESTS_RECOVERY] bit NOT NULL,
  [JOB_DATA] varbinary(max) NULL
);
GO

CREATE TABLE [scheduling].[quartz_SIMPLE_TRIGGERS] (
  [SCHED_NAME] nvarchar(120) NOT NULL,
  [TRIGGER_NAME] nvarchar(150) NOT NULL,
  [TRIGGER_GROUP] nvarchar(150) NOT NULL,
  [REPEAT_COUNT] int NOT NULL,
  [REPEAT_INTERVAL] bigint NOT NULL,
  [TIMES_TRIGGERED] int NOT NULL
);
GO

CREATE TABLE [scheduling].[quartz_SIMPROP_TRIGGERS] (
  [SCHED_NAME] nvarchar(120) NOT NULL,
  [TRIGGER_NAME] nvarchar(150) NOT NULL,
  [TRIGGER_GROUP] nvarchar(150) NOT NULL,
  [STR_PROP_1] nvarchar(512) NULL,
  [STR_PROP_2] nvarchar(512) NULL,
  [STR_PROP_3] nvarchar(512) NULL,
  [INT_PROP_1] int NULL,
  [INT_PROP_2] int NULL,
  [LONG_PROP_1] bigint NULL,
  [LONG_PROP_2] bigint NULL,
  [DEC_PROP_1] numeric(13,4) NULL,
  [DEC_PROP_2] numeric(13,4) NULL,
  [BOOL_PROP_1] bit NULL,
  [BOOL_PROP_2] bit NULL,
  [TIME_ZONE_ID] nvarchar(80) NULL 
);
GO

CREATE TABLE [scheduling].[quartz_BLOB_TRIGGERS] (
  [SCHED_NAME] nvarchar(120) NOT NULL,
  [TRIGGER_NAME] nvarchar(150) NOT NULL,
  [TRIGGER_GROUP] nvarchar(150) NOT NULL,
  [BLOB_DATA] varbinary(max) NULL
);
GO

CREATE TABLE [scheduling].[quartz_TRIGGERS] (
  [SCHED_NAME] nvarchar(120) NOT NULL,
  [TRIGGER_NAME] nvarchar(150) NOT NULL,
  [TRIGGER_GROUP] nvarchar(150) NOT NULL,
  [JOB_NAME] nvarchar(150) NOT NULL,
  [JOB_GROUP] nvarchar(150) NOT NULL,
  [DESCRIPTION] nvarchar(250) NULL,
  [NEXT_FIRE_TIME] bigint NULL,
  [PREV_FIRE_TIME] bigint NULL,
  [PRIORITY] int NULL,
  [TRIGGER_STATE] nvarchar(16) NOT NULL,
  [TRIGGER_TYPE] nvarchar(8) NOT NULL,
  [START_TIME] bigint NOT NULL,
  [END_TIME] bigint NULL,
  [CALENDAR_NAME] nvarchar(200) NULL,
  [MISFIRE_INSTR] int NULL,
  [JOB_DATA] varbinary(max) NULL
);
GO

ALTER TABLE [scheduling].[quartz_CALENDARS] WITH NOCHECK ADD
  CONSTRAINT [PK_quartz_CALENDARS] PRIMARY KEY  CLUSTERED
  (
    [SCHED_NAME],
    [CALENDAR_NAME]
  );
GO

ALTER TABLE [scheduling].[quartz_CRON_TRIGGERS] WITH NOCHECK ADD
  CONSTRAINT [PK_quartz_CRON_TRIGGERS] PRIMARY KEY  CLUSTERED
  (
    [SCHED_NAME],
    [TRIGGER_NAME],
    [TRIGGER_GROUP]
  );
GO

ALTER TABLE [scheduling].[quartz_FIRED_TRIGGERS] WITH NOCHECK ADD
  CONSTRAINT [PK_quartz_FIRED_TRIGGERS] PRIMARY KEY  CLUSTERED
  (
    [SCHED_NAME],
    [ENTRY_ID]
  );
GO

ALTER TABLE [scheduling].[quartz_PAUSED_TRIGGER_GRPS] WITH NOCHECK ADD
  CONSTRAINT [PK_quartz_PAUSED_TRIGGER_GRPS] PRIMARY KEY  CLUSTERED
  (
    [SCHED_NAME],
    [TRIGGER_GROUP]
  );
GO

ALTER TABLE [scheduling].[quartz_SCHEDULER_STATE] WITH NOCHECK ADD
  CONSTRAINT [PK_quartz_SCHEDULER_STATE] PRIMARY KEY  CLUSTERED
  (
    [SCHED_NAME],
    [INSTANCE_NAME]
  );
GO

ALTER TABLE [scheduling].[quartz_LOCKS] WITH NOCHECK ADD
  CONSTRAINT [PK_quartz_LOCKS] PRIMARY KEY  CLUSTERED
  (
    [SCHED_NAME],
    [LOCK_NAME]
  );
GO

ALTER TABLE [scheduling].[quartz_JOB_DETAILS] WITH NOCHECK ADD
  CONSTRAINT [PK_quartz_JOB_DETAILS] PRIMARY KEY  CLUSTERED
  (
    [SCHED_NAME],
    [JOB_NAME],
    [JOB_GROUP]
  );
GO

ALTER TABLE [scheduling].[quartz_SIMPLE_TRIGGERS] WITH NOCHECK ADD
  CONSTRAINT [PK_quartz_SIMPLE_TRIGGERS] PRIMARY KEY  CLUSTERED
  (
    [SCHED_NAME],
    [TRIGGER_NAME],
    [TRIGGER_GROUP]
  );
GO

ALTER TABLE [scheduling].[quartz_SIMPROP_TRIGGERS] WITH NOCHECK ADD
  CONSTRAINT [PK_quartz_SIMPROP_TRIGGERS] PRIMARY KEY  CLUSTERED
  (
    [SCHED_NAME],
    [TRIGGER_NAME],
    [TRIGGER_GROUP]
  );
GO

ALTER TABLE [scheduling].[quartz_TRIGGERS] WITH NOCHECK ADD
  CONSTRAINT [PK_quartz_TRIGGERS] PRIMARY KEY  CLUSTERED
  (
    [SCHED_NAME],
    [TRIGGER_NAME],
    [TRIGGER_GROUP]
  );
GO

ALTER TABLE [scheduling].[quartz_BLOB_TRIGGERS] WITH NOCHECK ADD
  CONSTRAINT [PK_quartz_BLOB_TRIGGERS] PRIMARY KEY  CLUSTERED
  (
    [SCHED_NAME],
    [TRIGGER_NAME],
    [TRIGGER_GROUP]
  );
GO

ALTER TABLE [scheduling].[quartz_CRON_TRIGGERS] ADD
  CONSTRAINT [FK_quartz_CRON_TRIGGERS_quartz_TRIGGERS] FOREIGN KEY
  (
    [SCHED_NAME],
    [TRIGGER_NAME],
    [TRIGGER_GROUP]
  ) REFERENCES [scheduling].[quartz_TRIGGERS] (
    [SCHED_NAME],
    [TRIGGER_NAME],
    [TRIGGER_GROUP]
  ) ON DELETE CASCADE;
GO

ALTER TABLE [scheduling].[quartz_SIMPLE_TRIGGERS] ADD
  CONSTRAINT [FK_quartz_SIMPLE_TRIGGERS_quartz_TRIGGERS] FOREIGN KEY
  (
    [SCHED_NAME],
    [TRIGGER_NAME],
    [TRIGGER_GROUP]
  ) REFERENCES [scheduling].[quartz_TRIGGERS] (
    [SCHED_NAME],
    [TRIGGER_NAME],
    [TRIGGER_GROUP]
  ) ON DELETE CASCADE;
GO

ALTER TABLE [scheduling].[quartz_SIMPROP_TRIGGERS] ADD
  CONSTRAINT [FK_quartz_SIMPROP_TRIGGERS_quartz_TRIGGERS] FOREIGN KEY
  (
	[SCHED_NAME],
    [TRIGGER_NAME],
    [TRIGGER_GROUP]
  ) REFERENCES [scheduling].[quartz_TRIGGERS] (
    [SCHED_NAME],
    [TRIGGER_NAME],
    [TRIGGER_GROUP]
  ) ON DELETE CASCADE;
GO

ALTER TABLE [scheduling].[quartz_TRIGGERS] ADD
  CONSTRAINT [FK_quartz_TRIGGERS_quartz_JOB_DETAILS] FOREIGN KEY
  (
    [SCHED_NAME],
    [JOB_NAME],
    [JOB_GROUP]
  ) REFERENCES [scheduling].[quartz_JOB_DETAILS] (
    [SCHED_NAME],
    [JOB_NAME],
    [JOB_GROUP]
  );
GO

CREATE INDEX [IDX_quartz_T_J] ON [scheduling].[quartz_TRIGGERS](SCHED_NAME, JOB_NAME,JOB_GROUP);
CREATE INDEX [IDX_quartz_T_JG] ON [scheduling].[quartz_TRIGGERS](SCHED_NAME, JOB_GROUP);
CREATE INDEX [IDX_quartz_T_C] ON [scheduling].[quartz_TRIGGERS](SCHED_NAME, CALENDAR_NAME);
CREATE INDEX [IDX_quartz_T_G] ON [scheduling].[quartz_TRIGGERS](SCHED_NAME, TRIGGER_GROUP);
CREATE INDEX [IDX_quartz_T_STATE] ON [scheduling].[quartz_TRIGGERS](SCHED_NAME, TRIGGER_STATE);
CREATE INDEX [IDX_quartz_T_N_STATE] ON [scheduling].[quartz_TRIGGERS](SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP, TRIGGER_STATE);
CREATE INDEX [IDX_quartz_T_N_G_STATE] ON [scheduling].[quartz_TRIGGERS](SCHED_NAME, TRIGGER_GROUP, TRIGGER_STATE);
CREATE INDEX [IDX_quartz_T_NEXT_FIRE_TIME] ON [scheduling].[quartz_TRIGGERS](SCHED_NAME, NEXT_FIRE_TIME);
CREATE INDEX [IDX_quartz_T_NFT_ST] ON [scheduling].[quartz_TRIGGERS](SCHED_NAME, TRIGGER_STATE, NEXT_FIRE_TIME);
CREATE INDEX [IDX_quartz_T_NFT_MISFIRE] ON [scheduling].[quartz_TRIGGERS](SCHED_NAME, MISFIRE_INSTR, NEXT_FIRE_TIME);
CREATE INDEX [IDX_quartz_T_NFT_ST_MISFIRE] ON [scheduling].[quartz_TRIGGERS](SCHED_NAME, MISFIRE_INSTR, NEXT_FIRE_TIME, TRIGGER_STATE);
CREATE INDEX [IDX_quartz_T_NFT_ST_MISFIRE_GRP] ON [scheduling].[quartz_TRIGGERS](SCHED_NAME, MISFIRE_INSTR, NEXT_FIRE_TIME, TRIGGER_GROUP, TRIGGER_STATE);

CREATE INDEX [IDX_quartz_FT_TRIG_INST_NAME] ON [scheduling].[quartz_FIRED_TRIGGERS](SCHED_NAME, INSTANCE_NAME);
CREATE INDEX [IDX_quartz_FT_INST_JOB_REQ_RCVRY] ON [scheduling].[quartz_FIRED_TRIGGERS](SCHED_NAME, INSTANCE_NAME, REQUESTS_RECOVERY);
CREATE INDEX [IDX_quartz_FT_J_G] ON [scheduling].[quartz_FIRED_TRIGGERS](SCHED_NAME, JOB_NAME, JOB_GROUP);
CREATE INDEX [IDX_quartz_FT_JG] ON [scheduling].[quartz_FIRED_TRIGGERS](SCHED_NAME, JOB_GROUP);
CREATE INDEX [IDX_quartz_FT_T_G] ON [scheduling].[quartz_FIRED_TRIGGERS](SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP);
CREATE INDEX [IDX_quartz_FT_TG] ON [scheduling].[quartz_FIRED_TRIGGERS](SCHED_NAME, TRIGGER_GROUP);
GO
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}