-- Flood Relief: Donation traceability + in-app notifications
-- Run this once against the same MySQL database used by the backend.
-- Column names intentionally match the EF Core property names used by this project.

CREATE TABLE IF NOT EXISTS donation_batches (
    Id VARCHAR(10) NOT NULL,
    DonationId VARCHAR(10) NOT NULL,
    DonationItemId VARCHAR(10) NOT NULL,
    CenterId VARCHAR(5) NOT NULL,
    ReliefItemId VARCHAR(10) NOT NULL,
    ReceivedQuantity INT NOT NULL,
    RemainingQuantity INT NOT NULL,
    ReceivedAt DATETIME(6) NOT NULL,
    CONSTRAINT PK_donation_batches PRIMARY KEY (Id),
    CONSTRAINT UQ_donation_batches_DonationItemId UNIQUE (DonationItemId),
    CONSTRAINT FK_donation_batches_donations
        FOREIGN KEY (DonationId) REFERENCES donations(Id) ON DELETE RESTRICT,
    CONSTRAINT FK_donation_batches_donation_items
        FOREIGN KEY (DonationItemId) REFERENCES donation_items(Id) ON DELETE RESTRICT,
    CONSTRAINT FK_donation_batches_centers
        FOREIGN KEY (CenterId) REFERENCES centers(Id) ON DELETE RESTRICT,
    CONSTRAINT FK_donation_batches_relief_items
        FOREIGN KEY (ReliefItemId) REFERENCES relief_items(Id) ON DELETE RESTRICT,
    INDEX IX_donation_batches_FIFO (CenterId, ReliefItemId, ReceivedAt)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS donation_allocations (
    Id VARCHAR(10) NOT NULL,
    DonationBatchId VARCHAR(10) NOT NULL,
    SosRequestId VARCHAR(10) NOT NULL,
    ReliefItemId VARCHAR(10) NOT NULL,
    Quantity INT NOT NULL,
    AllocatedAt DATETIME(6) NOT NULL,
    CONSTRAINT PK_donation_allocations PRIMARY KEY (Id),
    CONSTRAINT FK_donation_allocations_donation_batches
        FOREIGN KEY (DonationBatchId) REFERENCES donation_batches(Id) ON DELETE RESTRICT,
    CONSTRAINT FK_donation_allocations_sos_requests
        FOREIGN KEY (SosRequestId) REFERENCES sos_requests(Id) ON DELETE CASCADE,
    CONSTRAINT FK_donation_allocations_relief_items
        FOREIGN KEY (ReliefItemId) REFERENCES relief_items(Id) ON DELETE RESTRICT,
    INDEX IX_donation_allocations_SosBatch (SosRequestId, DonationBatchId),
    INDEX IX_donation_allocations_Batch (DonationBatchId)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS notifications (
    Id VARCHAR(10) NOT NULL,
    UserId VARCHAR(10) NOT NULL,
    Type VARCHAR(40) NOT NULL,
    Title VARCHAR(150) NOT NULL,
    Message VARCHAR(500) NOT NULL,
    ReferenceType VARCHAR(30) NULL,
    ReferenceId VARCHAR(10) NULL,
    IsRead TINYINT(1) NOT NULL DEFAULT 0,
    CreatedAt DATETIME(6) NOT NULL,
    ReadAt DATETIME(6) NULL,
    CONSTRAINT PK_notifications PRIMARY KEY (Id),
    CONSTRAINT FK_notifications_users
        FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE,
    INDEX IX_notifications_UserUnreadCreated (UserId, IsRead, CreatedAt)
) ENGINE=InnoDB;
