CREATE PROCEDURE GetOngoingReservations
AS
BEGIN
    SELECT 
        Id,
        ReservationDate,
        ReservationTime,
        TotalPrice,
        TableNumber,
        CustomerFullName,
        BuffetType,           
        SpecialRequest,       
        ReservationStatus
    FROM 
        ReservationDetails
    WHERE 
        CAST(ReservationDate AS DATE) = CAST(GETDATE() AS DATE) AND ReservationStatus = 'Ongoing'
END;