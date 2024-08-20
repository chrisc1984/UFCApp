SELECT * FROM tblEvent

SELECT * FROM tblFight WHERE eventId IN (27)

SELECT * FROM tblUserPicks WHERE fightd IN (SELECT id FROM tblFight WHERE eventId IN (21,22,23))

DELETE FROM tblUserPicks WHERE fightd IN (SELECT id FROM tblFight WHERE eventId IN (28));

DELETE FROM tblFight WHERE eventId IN (28);

DELETE FROM tblEvent WHERE id IN (28);