-- ============================================================
-- Tao cau truc database PostgreSQL cho phan mem InventoryManagement.
-- Ket noi vao database "quanlyhanghoa" truoc khi chay file nay.
-- File nay an toan khi chay lai nhieu lan vi co IF NOT EXISTS.
-- ============================================================

CREATE TABLE IF NOT EXISTS loaihang
(
    ma_loaihang serial PRIMARY KEY,
    ten_loaihang varchar(100) NOT NULL,
    ghi_chu text
);

CREATE TABLE IF NOT EXISTS nhacungcap
(
    ma_nhacungcap serial PRIMARY KEY,
    ten_nhacungcap varchar(255) NOT NULL,
    dia_chi_ncc varchar(500),
    so_dien_thoai varchar(20),
    email varchar(100),
    ghi_chu text
);

CREATE TABLE IF NOT EXISTS khachhang
(
    ma_khachhang serial PRIMARY KEY,
    ten_khachhang varchar(255) NOT NULL,
    dia_chi_kh varchar(500),
    so_dien_thoai varchar(20),
    email varchar(100),
    ghi_chu text
);

CREATE TABLE IF NOT EXISTS nhanvien
(
    ma_nhanvien serial PRIMARY KEY,
    ten_nhanvien varchar(255) NOT NULL,
    dia_chi_nv varchar(500),
    so_dien_thoai varchar(20),
    email varchar(100),
    ngay_sinh date,
    chuc_vu varchar(100),
    ghi_chu text,
    is_deleted boolean NOT NULL DEFAULT false
);

CREATE TABLE IF NOT EXISTS hanghoa
(
    ma_hanghoa serial PRIMARY KEY,
    ten_hanghoa varchar(255) NOT NULL,
    ma_loaihang integer NOT NULL,
    ma_nhacungcap integer NOT NULL,
    gia_nhap numeric(18, 2) NOT NULL DEFAULT 0,
    gia_ban numeric(18, 2) NOT NULL DEFAULT 0,
    so_luong_ton integer NOT NULL DEFAULT 0,
    don_vi_tinh varchar(50) NOT NULL DEFAULT '',
    ghi_chu text,
    is_deleted boolean NOT NULL DEFAULT false,
    CONSTRAINT fk_hanghoa_loaihang FOREIGN KEY (ma_loaihang) REFERENCES loaihang(ma_loaihang),
    CONSTRAINT fk_hanghoa_nhacungcap FOREIGN KEY (ma_nhacungcap) REFERENCES nhacungcap(ma_nhacungcap),
    CONSTRAINT ck_hanghoa_gia_nhap_khong_am CHECK (gia_nhap >= 0),
    CONSTRAINT ck_hanghoa_gia_ban_khong_am CHECK (gia_ban >= 0),
    CONSTRAINT ck_hanghoa_ton_khong_am CHECK (so_luong_ton >= 0)
);

CREATE TABLE IF NOT EXISTS taikhoan
(
    ma_taikhoan serial PRIMARY KEY,
    ma_nhanvien integer NOT NULL,
    ten_taikhoan varchar(50) NOT NULL UNIQUE,
    mat_khau varchar(255) NOT NULL,
    vai_tro varchar(50) NOT NULL DEFAULT 'NhanVien',
    trang_thai boolean NOT NULL DEFAULT true,
    CONSTRAINT fk_taikhoan_nhanvien FOREIGN KEY (ma_nhanvien) REFERENCES nhanvien(ma_nhanvien),
    CONSTRAINT ck_taikhoan_mat_khau_hash CHECK (mat_khau LIKE 'pbkdf2$%')
);

CREATE TABLE IF NOT EXISTS phieunhap
(
    ma_phieunhap serial PRIMARY KEY,
    ma_nhacungcap integer NOT NULL,
    ma_nhanvien integer NOT NULL,
    ngay_nhap date NOT NULL DEFAULT CURRENT_DATE,
    tong_tien numeric(18, 2) NOT NULL DEFAULT 0,
    ghi_chu text,
    CONSTRAINT fk_phieunhap_nhacungcap FOREIGN KEY (ma_nhacungcap) REFERENCES nhacungcap(ma_nhacungcap),
    CONSTRAINT fk_phieunhap_nhanvien FOREIGN KEY (ma_nhanvien) REFERENCES nhanvien(ma_nhanvien),
    CONSTRAINT ck_phieunhap_tong_tien_khong_am CHECK (tong_tien >= 0)
);

CREATE TABLE IF NOT EXISTS chitietphieunhap
(
    ma_chitiet serial PRIMARY KEY,
    ma_phieunhap integer NOT NULL,
    ma_hanghoa integer NOT NULL,
    so_luong integer NOT NULL,
    don_gia_nhap numeric(18, 2) NOT NULL DEFAULT 0,
    thanh_tien numeric(18, 2) NOT NULL DEFAULT 0,
    CONSTRAINT fk_ctpn_phieunhap FOREIGN KEY (ma_phieunhap) REFERENCES phieunhap(ma_phieunhap) ON DELETE CASCADE,
    CONSTRAINT fk_ctpn_hanghoa FOREIGN KEY (ma_hanghoa) REFERENCES hanghoa(ma_hanghoa),
    CONSTRAINT ck_ctpn_so_luong_duong CHECK (so_luong > 0),
    CONSTRAINT ck_ctpn_don_gia_khong_am CHECK (don_gia_nhap >= 0),
    CONSTRAINT ck_ctpn_thanh_tien_khong_am CHECK (thanh_tien >= 0)
);

CREATE TABLE IF NOT EXISTS phieuxuat
(
    ma_phieuxuat serial PRIMARY KEY,
    ma_khachhang integer NOT NULL,
    ma_nhanvien integer NOT NULL,
    ngay_xuat date NOT NULL DEFAULT CURRENT_DATE,
    tong_tien numeric(18, 2) NOT NULL DEFAULT 0,
    ghi_chu text,
    CONSTRAINT fk_phieuxuat_khachhang FOREIGN KEY (ma_khachhang) REFERENCES khachhang(ma_khachhang),
    CONSTRAINT fk_phieuxuat_nhanvien FOREIGN KEY (ma_nhanvien) REFERENCES nhanvien(ma_nhanvien),
    CONSTRAINT ck_phieuxuat_tong_tien_khong_am CHECK (tong_tien >= 0)
);

CREATE TABLE IF NOT EXISTS chitietphieuxuat
(
    ma_chitiet serial PRIMARY KEY,
    ma_phieuxuat integer NOT NULL,
    ma_hanghoa integer NOT NULL,
    so_luong integer NOT NULL,
    don_gia_xuat numeric(18, 2) NOT NULL DEFAULT 0,
    thanh_tien numeric(18, 2) NOT NULL DEFAULT 0,
    CONSTRAINT fk_ctpx_phieuxuat FOREIGN KEY (ma_phieuxuat) REFERENCES phieuxuat(ma_phieuxuat) ON DELETE CASCADE,
    CONSTRAINT fk_ctpx_hanghoa FOREIGN KEY (ma_hanghoa) REFERENCES hanghoa(ma_hanghoa),
    CONSTRAINT ck_ctpx_so_luong_duong CHECK (so_luong > 0),
    CONSTRAINT ck_ctpx_don_gia_khong_am CHECK (don_gia_xuat >= 0),
    CONSTRAINT ck_ctpx_thanh_tien_khong_am CHECK (thanh_tien >= 0)
);

CREATE TABLE IF NOT EXISTS auditlog
(
    ma_auditlog serial PRIMARY KEY,
    ten_taikhoan varchar(50),
    vai_tro varchar(50),
    hanh_dong varchar(50) NOT NULL,
    bang_du_lieu varchar(100) NOT NULL,
    ma_ban_ghi integer,
    noi_dung text,
    ip_address varchar(64),
    thoi_gian timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Bo sung cot cho database cu neu thieu.
ALTER TABLE taikhoan ADD COLUMN IF NOT EXISTS trang_thai boolean NOT NULL DEFAULT true;
ALTER TABLE hanghoa ADD COLUMN IF NOT EXISTS is_deleted boolean NOT NULL DEFAULT false;
ALTER TABLE nhanvien ADD COLUMN IF NOT EXISTS is_deleted boolean NOT NULL DEFAULT false;

-- Index phuc vu tim kiem/tra cuu nhanh.
CREATE INDEX IF NOT EXISTS idx_hanghoa_ten ON hanghoa(ten_hanghoa);
CREATE INDEX IF NOT EXISTS idx_hanghoa_loai ON hanghoa(ma_loaihang);
CREATE INDEX IF NOT EXISTS idx_hanghoa_nhacungcap ON hanghoa(ma_nhacungcap);
CREATE UNIQUE INDEX IF NOT EXISTS uq_hanghoa_ten_ncc_active
    ON hanghoa (lower(trim(ten_hanghoa)), ma_nhacungcap)
    WHERE is_deleted = false;
CREATE INDEX IF NOT EXISTS idx_nhanvien_active ON nhanvien(is_deleted);
CREATE INDEX IF NOT EXISTS idx_phieunhap_ngay ON phieunhap(ngay_nhap);
CREATE INDEX IF NOT EXISTS idx_phieuxuat_ngay ON phieuxuat(ngay_xuat);
CREATE INDEX IF NOT EXISTS idx_auditlog_thoi_gian ON auditlog(thoi_gian DESC);
CREATE INDEX IF NOT EXISTS idx_auditlog_bang_ban_ghi ON auditlog(bang_du_lieu, ma_ban_ghi);
