CREATE PROCEDURE GetCompletedReservations
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
        ReservationStatus = 'Completed';
END;