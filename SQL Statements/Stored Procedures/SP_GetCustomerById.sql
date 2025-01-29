CREATE PROCEDURE GetCustomerById
    @Id INT
AS
BEGIN
    SELECT Id, FirstName, LastName, PhoneNumber, Email, CreatedAt, UpdatedAt
    FROM Customers
    WHERE Id = @Id;
END;
