-- Append this block to MinCourseraSQLScript.sql

IF OBJECT_ID(N'[dbo].[AiGeneratedQuizzes]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AiGeneratedQuizzes](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ModuleId] [int] NOT NULL,
        [GeneratedByUserId] [int] NOT NULL,
        [GeneratedForEnrollmentId] [int] NULL,
        [Output] [nvarchar](max) NOT NULL,
        [Difficulty] [int] NOT NULL,
        [QuestionsCount] [int] NOT NULL,
        [Language] [nvarchar](8) NOT NULL CONSTRAINT [DF_AiGeneratedQuizzes_Language] DEFAULT (N'en'),
        [IncludeExplanations] [bit] NOT NULL,
        [GenerationSource] [nvarchar](20) NOT NULL CONSTRAINT [DF_AiGeneratedQuizzes_GenerationSource] DEFAULT (N'Student'),
        [CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_AiGeneratedQuizzes_CreatedAt] DEFAULT (GETDATE()),
        [IsActive] [bit] NOT NULL CONSTRAINT [DF_AiGeneratedQuizzes_IsActive] DEFAULT ((1)),
        CONSTRAINT [PK_AiGeneratedQuizzes] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_AiGeneratedQuizzes_CourseModules_ModuleId] FOREIGN KEY([ModuleId]) REFERENCES [dbo].[CourseModules]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AiGeneratedQuizzes_Enrollments_GeneratedForEnrollmentId] FOREIGN KEY([GeneratedForEnrollmentId]) REFERENCES [dbo].[Enrollments]([Id]),
        CONSTRAINT [FK_AiGeneratedQuizzes_Users_GeneratedByUserId] FOREIGN KEY([GeneratedByUserId]) REFERENCES [dbo].[Users]([Id])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AiGeneratedQuizzes_ModuleId' AND object_id = OBJECT_ID('[dbo].[AiGeneratedQuizzes]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AiGeneratedQuizzes_ModuleId] ON [dbo].[AiGeneratedQuizzes]([ModuleId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AiGeneratedQuizzes_GeneratedByUserId' AND object_id = OBJECT_ID('[dbo].[AiGeneratedQuizzes]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AiGeneratedQuizzes_GeneratedByUserId] ON [dbo].[AiGeneratedQuizzes]([GeneratedByUserId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AiGeneratedQuizzes_GeneratedForEnrollmentId' AND object_id = OBJECT_ID('[dbo].[AiGeneratedQuizzes]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AiGeneratedQuizzes_GeneratedForEnrollmentId] ON [dbo].[AiGeneratedQuizzes]([GeneratedForEnrollmentId]);
END
GO

IF OBJECT_ID(N'[dbo].[QuizAssignments]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[QuizAssignments](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [AiGeneratedQuizId] [int] NOT NULL,
        [EnrollmentId] [int] NOT NULL,
        [AssignedByInstructorId] [int] NOT NULL,
        [AssignedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_QuizAssignments_AssignedAt] DEFAULT (GETDATE()),
        [DueAt] [datetime2](7) NULL,
        [IsActive] [bit] NOT NULL CONSTRAINT [DF_QuizAssignments_IsActive] DEFAULT ((1)),
        CONSTRAINT [PK_QuizAssignments] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_QuizAssignments_AiGeneratedQuizzes_AiGeneratedQuizId] FOREIGN KEY([AiGeneratedQuizId]) REFERENCES [dbo].[AiGeneratedQuizzes]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_QuizAssignments_Enrollments_EnrollmentId] FOREIGN KEY([EnrollmentId]) REFERENCES [dbo].[Enrollments]([Id]),
        CONSTRAINT [FK_QuizAssignments_Users_AssignedByInstructorId] FOREIGN KEY([AssignedByInstructorId]) REFERENCES [dbo].[Users]([Id])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_QuizAssignments_AiGeneratedQuizId_EnrollmentId' AND object_id = OBJECT_ID('[dbo].[QuizAssignments]'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_QuizAssignments_AiGeneratedQuizId_EnrollmentId]
        ON [dbo].[QuizAssignments]([AiGeneratedQuizId], [EnrollmentId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_QuizAssignments_AssignedByInstructorId' AND object_id = OBJECT_ID('[dbo].[QuizAssignments]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_QuizAssignments_AssignedByInstructorId] ON [dbo].[QuizAssignments]([AssignedByInstructorId]);
END
GO

IF OBJECT_ID(N'[dbo].[StudentQuizAttempts]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[StudentQuizAttempts](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [AiGeneratedQuizId] [int] NOT NULL,
        [EnrollmentId] [int] NOT NULL,
        [QuizAssignmentId] [int] NULL,
        [AttemptNumber] [int] NOT NULL,
        [StudentResponses] [nvarchar](max) NOT NULL,
        [Score] [decimal](5,2) NOT NULL,
        [CorrectAnswers] [int] NOT NULL,
        [TotalQuestions] [int] NOT NULL,
        [IsCompleted] [bit] NOT NULL,
        [CompletedAt] [datetime2](7) NULL,
        [DurationSeconds] [int] NOT NULL,
        [CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_StudentQuizAttempts_CreatedAt] DEFAULT (GETDATE()),
        CONSTRAINT [PK_StudentQuizAttempts] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_StudentQuizAttempts_AiGeneratedQuizzes_AiGeneratedQuizId] FOREIGN KEY([AiGeneratedQuizId]) REFERENCES [dbo].[AiGeneratedQuizzes]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_StudentQuizAttempts_Enrollments_EnrollmentId] FOREIGN KEY([EnrollmentId]) REFERENCES [dbo].[Enrollments]([Id]),
        CONSTRAINT [FK_StudentQuizAttempts_QuizAssignments_QuizAssignmentId] FOREIGN KEY([QuizAssignmentId]) REFERENCES [dbo].[QuizAssignments]([Id]) ON DELETE SET NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StudentQuizAttempts_EnrollmentId_AiGeneratedQuizId_CreatedAt' AND object_id = OBJECT_ID('[dbo].[StudentQuizAttempts]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_StudentQuizAttempts_EnrollmentId_AiGeneratedQuizId_CreatedAt]
        ON [dbo].[StudentQuizAttempts]([EnrollmentId], [AiGeneratedQuizId], [CreatedAt]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StudentQuizAttempts_QuizAssignmentId' AND object_id = OBJECT_ID('[dbo].[StudentQuizAttempts]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_StudentQuizAttempts_QuizAssignmentId] ON [dbo].[StudentQuizAttempts]([QuizAssignmentId]);
END
GO
