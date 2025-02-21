CREATE PROCEDURE UpdatePassword
    @UserId INT,
    @NewPassword NVARCHAR(255)
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = @UserId)
    BEGIN
        PRINT 'User not found';
        RETURN;
    END

    UPDATE Users
    SET Password = @NewPassword,
		UpdatedAt = GETDATE()
    WHERE Id = @UserId;
END;
