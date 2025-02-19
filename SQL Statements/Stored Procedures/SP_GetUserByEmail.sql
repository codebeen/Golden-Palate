CREATE PROCEDURE GetUserByEmail
    @Email VARCHAR(255)
AS
BEGIN
    SELECT Id, FirstName, LastName, Email, Password, Role, Status, IsDeleted, CreatedAt, UpdatedAt
    FROM Users
    WHERE Email = @Email
		AND IsDeleted = 0
END
