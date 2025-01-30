CREATE PROCEDURE UpdateTable
    @Id INT,
    @TableNumber INT,
    @Description VARCHAR(255) = NULL,
    @SeatingCapacity INT,
    @TableLocation VARCHAR(100),
    @Price DECIMAL(10,2) = 0.00,
    @TableImagePath VARCHAR(255) = NULL,
    @Status VARCHAR(50) = 'Available'
AS
BEGIN
    UPDATE Tables
    SET TableNumber = @TableNumber,
        Description = @Description,
        SeatingCapacity = @SeatingCapacity,
        TableLocation = @TableLocation,
        Price = @Price,
        TableImagePath = @TableImagePath,
        Status = @Status,
        UpdatedAt = GETDATE()
    WHERE Id = @Id AND IsDeleted = 0;
END;
