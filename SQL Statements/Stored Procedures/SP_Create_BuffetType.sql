CREATE PROCEDURE CreateBuffetType
    @Name VARCHAR(100),
    @Description VARCHAR(255) = NULL
AS
BEGIN
	-- Insert record to the table
    INSERT INTO BuffetTypes (Name, Description)
    VALUES (@Name, @Description);
END;
