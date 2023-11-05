IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OutboxMessages')
BEGIN
  CREATE TABLE OutboxMessages
  (
      Id UNIQUEIDENTIFIER PRIMARY KEY,
      MessageType NVARCHAR(255) NOT NULL,
      Payload NVARCHAR(MAX) CHECK (ISJSON(Payload) = 1) NOT NULL,
      CreatedAt DATETIME NOT NULL DEFAULT(GETUTCDATE()),
      ProcessedAt DATETIME NULL
  )
END