CREATE TABLE IF NOT EXISTS `sos_victim_severity_counts` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `SosRequestId` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Severity` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ChildCount` int NOT NULL DEFAULT 0,
  `AdultCount` int NOT NULL DEFAULT 0,
  `ElderlyCount` int NOT NULL DEFAULT 0,
  `DisabledCount` int NOT NULL DEFAULT 0,
  `PatientCount` int NOT NULL DEFAULT 0,
  `DeathCount` int NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_sos_victim_severity_counts_SosRequestId_Severity` (`SosRequestId`,`Severity`),
  CONSTRAINT `FK_sos_victim_severity_counts_sos_requests_SosRequestId`
    FOREIGN KEY (`SosRequestId`) REFERENCES `sos_requests` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

INSERT IGNORE INTO `sos_victim_severity_counts`
(`SosRequestId`,`Severity`,`ChildCount`,`AdultCount`,`ElderlyCount`,`DisabledCount`,`PatientCount`,`DeathCount`)
SELECT
  `Id`,
  COALESCE(NULLIF(`Severity`, ''), 'Mild'),
  `ChildCount`,
  GREATEST(`VictimCount` - `ChildCount` - `ElderlyCount` - `DisabledCount` - `PatientCount` - `DeathCount`, 0),
  `ElderlyCount`,
  `DisabledCount`,
  `PatientCount`,
  `DeathCount`
FROM `sos_requests`
WHERE `RequestType` = 'Emergency'
  AND (`VictimCount` > 0 OR `ChildCount` > 0 OR `ElderlyCount` > 0 OR `DisabledCount` > 0 OR `PatientCount` > 0 OR `DeathCount` > 0);
