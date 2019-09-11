
			IF NOT EXISTS ( SELECT  *
							FROM    sys.schemas
							WHERE   name = N'plinovodiduty' ) 
				EXEC('CREATE SCHEMA [plinovodiduty]');






CREATE TABLE plinovodiduty.duty 
    ( id INTEGER NOT NULL IDENTITY NOT FOR REPLICATION , 
     "From" DATETIME2 NOT NULL , 
     "To" DATETIME2 NOT NULL 
    )

ALTER TABLE PlinovodiDuty.Duty ADD constraint duty_pk PRIMARY KEY CLUSTERED (Id)
     WITH (
     ALLOW_PAGE_LOCKS = ON , 
     ALLOW_ROW_LOCKS = ON )

CREATE TABLE plinovodiduty.employee 
    ( id INTEGER NOT NULL IDENTITY NOT FOR REPLICATION , 
     Name VARCHAR (20) NOT NULL , 
     Surname VARCHAR (20) NOT NULL 
    )

ALTER TABLE PlinovodiDuty.Employee ADD constraint employee_pk PRIMARY KEY CLUSTERED (Id)
     WITH (
     ALLOW_PAGE_LOCKS = ON , 
     ALLOW_ROW_LOCKS = ON )

CREATE TABLE employeeonduty (
    employeeid   INTEGER NOT NULL,
    dutyid       INTEGER NOT NULL
)

ALTER TABLE EmployeeOnDuty ADD constraint employeeonduty_pk PRIMARY KEY CLUSTERED (DutyId, EmployeeId)
     WITH (
     ALLOW_PAGE_LOCKS = ON , 
     ALLOW_ROW_LOCKS = ON )

CREATE TABLE plinovodiduty.intervention 
    ( id INTEGER NOT NULL IDENTITY NOT FOR REPLICATION , 
     DutyId INTEGER NOT NULL , 
     "From" DATETIME2 NOT NULL , 
     "To" DATETIME2 NOT NULL , 
     ShortDescription VARCHAR (50) NOT NULL , 
     LongDescription VARCHAR (500) NOT NULL 
    )

ALTER TABLE PlinovodiDuty.Intervention ADD constraint intervention_pk PRIMARY KEY CLUSTERED (Id)
     WITH (
     ALLOW_PAGE_LOCKS = ON , 
     ALLOW_ROW_LOCKS = ON )

ALTER TABLE EmployeeOnDuty
    ADD CONSTRAINT employeeonduty_duty_fk FOREIGN KEY ( dutyid )
        REFERENCES plinovodiduty.duty ( id )
ON DELETE NO ACTION 
    ON UPDATE no action 

ALTER TABLE EmployeeOnDuty
    ADD CONSTRAINT employeeonduty_employee_fk FOREIGN KEY ( employeeid )
        REFERENCES plinovodiduty.employee ( id )
ON DELETE NO ACTION 
    ON UPDATE no action 

ALTER TABLE PlinovodiDuty.Intervention
    ADD CONSTRAINT intervencija_dezurstvo_fk FOREIGN KEY ( dutyid )
        REFERENCES plinovodiduty.duty ( id )
ON DELETE NO ACTION 
    ON UPDATE no action 

ALTER SCHEMA PlinovodiDuty

TRANSFER dbo.EmployeeOnDuty

GO





INSERT INTO [plinovodiduty].[employee] ([Name] ,[Surname]) VALUES ('Janez', 'Novak')
INSERT INTO [plinovodiduty].[employee] ([Name] ,[Surname]) VALUES ('Micka', 'Kovačeva')
INSERT INTO [plinovodiduty].[employee] ([Name] ,[Surname]) VALUES ('Peter', 'Klepec')


DECLARE @i int = 0
DECLARE @startTime DateTime2 = '2019-08-05 07:00'
select DATEADD(week, 1, @startTime)
select DATEADD(hour, -1, DATEADD(week, 1, @startTime))

WHILE @i < 15
BEGIN
    SET @i = @i + 1
	INSERT INTO [plinovodiduty].[duty]([From] ,[To]) VALUES (@startTime,DATEADD(hour, -1, DATEADD(week, 1, @startTime)))
	SET @startTime = DATEADD(week, 1, @startTime)
END



SET @i  = 1
WHILE @i <= 10
BEGIN
	INSERT INTO [plinovodiduty].[employeeonduty]([employeeid] ,[dutyid]) VALUES((@i % 3) + 1, @i)
    SET @i = @i + 1
END

INSERT INTO [plinovodiduty].[intervention]
           ([DutyId]
           ,[From]
           ,[To]
           ,[ShortDescription]
           ,[LongDescription])
     VALUES
           (3, '2019-09-17 15:00', '2019-09-17 17:00', 'kratek opis', 'dolg opis')