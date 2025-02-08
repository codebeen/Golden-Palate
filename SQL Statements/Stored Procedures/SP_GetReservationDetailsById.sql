CREATE PROCEDURE GetReservationDetailsById
    @ReservationId INT
AS
BEGIN
    SELECT 
        Id,
        ReservationDate,
        TotalPrice,
        TableNumber,
        CustomerFullName,
        BuffetType,           
        SpecialRequest,       
        ReservationStatus
    FROM 
        ReservationDetails
    WHERE 
        Id = @ReservationId;
END;