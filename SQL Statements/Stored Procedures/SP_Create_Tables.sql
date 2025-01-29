CREATE PROCEDURE CreateTable
    @TableNumber INT,
    @Description VARCHAR(255) = NULL,
    @SeatingCapacity INT,
    @TableLocation VARCHAR(100),
    @Price DECIMAL(10,2),
    @TableImagePath VARCHAR(255) = NULL
AS
BEGIN
    INSERT INTO Tables (TableNumber, Description, SeatingCapacity, TableLocation, Price, TableImagePath)
    VALUES (@TableNumber, @Description, @SeatingCapacity, @TableLocation, @Price, @TableImagePath);
END;
