CREATE PROCEDURE UpdateCustomer
    @Id INT,
    @FirstName VARCHAR(100),
    @LastName VARCHAR(100),
    @PhoneNumber VARCHAR(15),
    @Email VARCHAR(255)
AS
BEGIN
    UPDATE Customers
    SET FirstName = @FirstName,
        LastName = @LastName,
        PhoneNumber = @PhoneNumber,
        Email = @Email,
        UpdatedAt = GETDATE()
    WHERE Id = @Id;
END;
