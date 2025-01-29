CREATE PROCEDURE CreateCustomer
    @FirstName VARCHAR(100),
    @LastName VARCHAR(100),
    @PhoneNumber VARCHAR(15),
    @Email VARCHAR(255)
AS
BEGIN
    INSERT INTO Customers (FirstName, LastName, PhoneNumber, Email)
    VALUES (@FirstName, @LastName, @PhoneNumber, @Email);
END;
