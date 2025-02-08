CREATE PROCEDURE GetAvailableTablesForToday
    @BuffetType VARCHAR(20)  -- 'Breakfast', 'Lunch', 'Dinner'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Today DATE = CAST(GETDATE() AS DATE);

    -- Get tables that are NOT reserved for today and the given buffet type
    SELECT T.*
    FROM Tables AS T
    WHERE T.Id NOT IN (
        SELECT R.TableId
        FROM Reservations AS R
        WHERE R.ReservationDate = @Today
        AND R.BuffetType = @BuffetType
        AND R.Status = 'Confirmed'  -- Consider only confirmed reservations
    );
END;

--EXEC GetAvailableTablesForToday 'Lunch';
