-- Chay file nay neu database cu bao loi:
-- 42703: column "trang_thai" does not exist

ALTER TABLE taikhoan
ADD COLUMN IF NOT EXISTS trang_thai boolean NOT NULL DEFAULT true;
