
BEGIN

-- Create the temporary table if it doesn't exist
CREATE TEMP TABLE IF NOT EXISTS Scores (
    id INT,
    score DECIMAL,
	name TEXT
);

-- Insert initial data into the temporary table
INSERT INTO Scores (id, score, name)
    SELECT id, 0, name
    FROM tblUser;

-- Update the score based on aggregated values
UPDATE Scores
SET score = (
    SELECT COALESCE(SUM(F2.fighter1Odds), 0) + COALESCE(SUM(F3.Fighter2Odds), 0)
    FROM tblUserPicks P
    JOIN tblFight F ON F.id = P.fightd
    LEFT JOIN tblFight F2 ON F2.id = F.id AND F2.winner = F.fighter1name
    LEFT JOIN tblFight F3 ON F3.id = F.id AND F3.winner = F.fighter2name
    WHERE P.userid = Scores.id AND P.name = F.Winner
)
WHERE id IN (
    SELECT userid
    FROM tblUserPicks
);
	
    RETURN QUERY SELECT id, name, score FROM Scores;
END;
