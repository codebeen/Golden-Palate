CREATE PROCEDURE GetAvailableTablesForToday 
    @BuffetType VARCHAR(20)  -- Expected values: 'Breakfast', 'Lunch', 'Dinner'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Today DATE = CAST(GETDATE() AS DATE);

    -- Get tables that are NOT reserved for today and the given buffet type
    SELECT T.*
    FROM Tables AS T
    WHERE NOT EXISTS (
        SELECT 1
        FROM Reservations AS R
        WHERE R.TableId = T.Id
          AND R.ReservationDate = @Today
          AND R.BuffetType = @BuffetType
          AND R.Status IN ('Pending', 'Ongoing')
    );
END;
