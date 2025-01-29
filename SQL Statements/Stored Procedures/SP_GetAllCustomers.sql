CREATE PROCEDURE GetAllCustomers
AS
BEGIN
    SELECT Id, FirstName, LastName, PhoneNumber, Email, CreatedAt, UpdatedAt
    FROM Customers;
END;
