CREATE PROCEDURE CreateCustomer
    @FirstName VARCHAR(100),
    @LastName VARCHAR(100),
    @PhoneNumber VARCHAR(15),
    @Email VARCHAR(255)
AS
BEGIN
    -- Insert the customer into the Customers table
    INSERT INTO Customers (FirstName, LastName, PhoneNumber, Email)
    VALUES (@FirstName, @LastName, @PhoneNumber, @Email);

    SELECT *
    FROM Customers
    WHERE Id = SCOPE_IDENTITY();
END;
