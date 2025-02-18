CREATE PROCEDURE RegisterUser
    @FirstName VARCHAR(100),
    @LastName VARCHAR(100),
    @Email VARCHAR(255),
    @Password VARCHAR(255),
    @Role Varchar(50) = "Staff"
AS
BEGIN
    -- Check if the email already exists
    IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email AND IsDeleted = 0)
    BEGIN
        PRINT 'Error: Email already registered';
        RETURN;
    END

    -- Insert new user into Users table
    INSERT INTO Users (FirstName, LastName, Email, Password, Role)
    VALUES (@FirstName, @LastName, @Email, @Password, @Role);

    PRINT 'User registered successfully';
END;
