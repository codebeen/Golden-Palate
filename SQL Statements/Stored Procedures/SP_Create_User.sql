CREATE PROCEDURE RegisterUser
    @FirstName VARCHAR(100),
    @LastName VARCHAR(100),
    @Email VARCHAR(255),
    @Password VARCHAR(255)
AS
BEGIN
    -- Check if the email already exists
    IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
    BEGIN
        PRINT 'Error: Email already registered';
        RETURN;
    END

    -- Insert new user into Users table
    INSERT INTO Users (FirstName, LastName, Email, Password)
    VALUES (@FirstName, @LastName, @Email, @Password);

    PRINT 'User registered successfully';
END;
