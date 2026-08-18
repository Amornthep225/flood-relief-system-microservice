-- รันครั้งเดียวกับฐานข้อมูล floodreliefdb ก่อนทดสอบ SOS ฉุกเฉิน
ALTER TABLE `sos_requests`
    ADD COLUMN `RequestType` VARCHAR(20) NOT NULL DEFAULT 'Relief' AFTER `AddressDetail`,
    ADD COLUMN `EmergencyType` VARCHAR(50) NULL AFTER `RequestType`,
    ADD COLUMN `VictimCount` INT NOT NULL DEFAULT 1 AFTER `EmergencyType`,
    ADD COLUMN `ChildCount` INT NOT NULL DEFAULT 0 AFTER `VictimCount`,
    ADD COLUMN `ElderlyCount` INT NOT NULL DEFAULT 0 AFTER `ChildCount`,
    ADD COLUMN `DisabledCount` INT NOT NULL DEFAULT 0 AFTER `ElderlyCount`,
    ADD COLUMN `PatientCount` INT NOT NULL DEFAULT 0 AFTER `DisabledCount`,
    ADD COLUMN `WaterLevel` DECIMAL(5,2) NULL AFTER `PatientCount`,
    ADD COLUMN `EmergencyDetail` VARCHAR(1000) NULL AFTER `WaterLevel`;

-- ข้อมูลเดิมทั้งหมดคือคำขอรับสิ่งของ
UPDATE `sos_requests`
SET `RequestType` = 'Relief'
WHERE `RequestType` IS NULL OR `RequestType` = '';
