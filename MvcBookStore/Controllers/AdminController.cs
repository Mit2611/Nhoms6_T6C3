﻿using MvcBookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;
using System.IO;
using System.Net;

namespace MvcBookStore.Controllers
{
    public class AdminController : Controller
    {
        dbQLBansachDataContext db = new dbQLBansachDataContext();
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Sach(int? page)
        {
            if (Session["Taikhoanadmin"] == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            int pageNumber = (page ?? 1);
            int pageSize = 10;
            //return View(db.SACHes.ToList());
            return View(db.SACHes.ToList().OrderBy(n => n.Masach).ToPagedList(pageNumber, pageSize));
        }
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(FormCollection collection)
        {
            //Gan cac gia tri nguoi dung nhap lieu cac bien
            var tendn = collection["username"];
            var matkhau = collection["password"];
            if (String.IsNullOrEmpty(tendn))
            {
                ViewData["Loi1"] = "Phải nhập tên đăng nhập!";
            }
            else if (String.IsNullOrEmpty(matkhau))
            {
                ViewData["Loi2"] = "Phải nhập mật khẩu!";
            }
            else
            {
                //Gan gia tri cho doi tuong duoc tao moi
                Admin ad = db.Admins.SingleOrDefault(n => n.UserAdmin == tendn && n.PassAdmin == matkhau);
                if (ad != null)
                {
                    Session["Ten"] = ad.Hoten;
                    Session["Taikhoanadmin"] = ad;
                    return RedirectToAction("Index", "Admin");
                }
                else
                    ViewBag.Thongbao = "Tên đăng nhập hoặc mật khẩu không đúng!";
            }
            return View();
        }
        [HttpGet]
        public ActionResult ThemmoiSach()
        {
            if (Session["Taikhoanadmin"] == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            //Dua du lieu vao dropdownList
            //Lay ds tu table LOAI, sap xep theo ten loai, cho lay gia tri Ma Loai, hien thi Tenloai
            ViewBag.MaLoai = new SelectList(db.Loais.ToList().OrderBy(n => n.TenLoai), "MaLoai", "TenLoai");
            ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB");
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ThemmoiSach(SACH sach, HttpPostedFileBase fileupload)
        {
            if (Session["Taikhoanadmin"] == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            //Dua du lieu vao dropdownload
            ViewBag.MaLoai = new SelectList(db.Loais.ToList().OrderBy(n => n.TenLoai), "MaLoai", "TenLoai");
            ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB");
            //Kiem tra duong dan file
            if (fileupload == null)
            {
                ViewBag.Thongbao = "Vui lòng chọn ảnh nền!";
                return View();
            }
            //Them vao CSDL
            else
            {
                if (ModelState.IsValid)
                {
                    //Luu ten file, luu y bo sung thu vien
                    var fileName = Path.GetFileName(fileupload.FileName);
                    //Luu duong dan cua file
                    var path = Path.Combine(Server.MapPath("~/images/anhsp/"), fileName);
                    //Kiem tra anh da ton tai hay chua
                    if (System.IO.File.Exists(path))
                    {
                        ViewBag.Thongbao = "Hình ảnh đã tồn tại tên tương tự! Vui lòng đặt tên khác!";
                    }
                    else
                    {
                        //Luu hinh anh vao duong dan
                        fileupload.SaveAs(path);
                    }
                    sach.Anhbia = fileName;
                    //Luu vao CSDL
                    db.SACHes.InsertOnSubmit(sach);
                    db.SubmitChanges();
                }
            }
            return View();
        }
        public ActionResult Chitietsach(int id)
        {
            //Lay doi tuong sach theo ma
            SACH sach = db.SACHes.SingleOrDefault(n => n.Masach == id);
            ViewBag.Masach = sach.Masach;
            if (sach == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sach);
        }
        public ActionResult Xoasach(int id)
        {
            //Lay ra doi tuong sach can xoa theo ma
            SACH sach = db.SACHes.SingleOrDefault(n => n.Masach == id);
            ViewBag.Masach = sach.Masach;
            if (sach == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sach);
        }
        [HttpPost, ActionName("Xoasach")]
        public ActionResult Xacnhanxoa(int id)
        {
            //Lay ra doi tuong sach can xoa theo ma
            SACH sach = db.SACHes.SingleOrDefault(n => n.Masach == id);
            ViewBag.Masach = sach.Masach;
            if (sach == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            db.SACHes.DeleteOnSubmit(sach);
            db.SubmitChanges();
            return RedirectToAction("Sach");
        }
        //Sua san pham
        [HttpGet]
        public ActionResult Suasach(int? id)  // Thêm nullable
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SACH sach = db.SACHes.SingleOrDefault(n => n.Masach == id);
            if (sach == null)
            {
                return HttpNotFound();
            }

            // Thêm selected value cho dropdown
            ViewBag.MaLoai = new SelectList(db.Loais.ToList().OrderBy(n => n.TenLoai), "MaLoai", "TenLoai", sach.MaLoai);
            ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB", sach.MaNXB);

            return View(sach);
        }
        [HttpPost]
        public ActionResult Suasach(SACH sach, HttpPostedFileBase fileupload)
        {
            if (Session["Taikhoanadmin"] == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            //Dua du lieu vao dropdownload
            ViewBag.MaLoai = new SelectList(db.Loais.ToList().OrderBy(n => n.TenLoai), "MaLoai", "TenLoai", sach.MaLoai);
            ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB", sach.MaNXB);

            if (ModelState.IsValid)
            {
                // Lấy sách hiện tại từ database
                var sachUpdate = db.SACHes.SingleOrDefault(n => n.Masach == sach.Masach);
                if (sachUpdate == null)
                {
                    return HttpNotFound();
                }

                // Cập nhật các thông tin cơ bản
                sachUpdate.Tensach = sach.Tensach;
                sachUpdate.Giaban = sach.Giaban;
                sachUpdate.Soluongton = sach.Soluongton;
                sachUpdate.MaLoai = sach.MaLoai;
                sachUpdate.MaNXB = sach.MaNXB;

                // Xử lý upload file
                if (fileupload != null && fileupload.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(fileupload.FileName);
                    var path = Path.Combine(Server.MapPath("~/images/anhsp"), fileName);


                    // Lưu file mới
                    fileupload.SaveAs(path);
                    sachUpdate.Anhbia = fileName;
                }

                try
                {
                    db.SubmitChanges();
                    return RedirectToAction("Sach");
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Có lỗi xảy ra khi cập nhật: " + ex.Message;
                    return View(sach);
                }
            }

            // Nếu ModelState không hợp lệ
            return View(sach);
        }

        public ActionResult DonHang(int? page)
        {
            if (Session["Taikhoanadmin"] == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            int pageNumber = (page ?? 1);
            int pageSize = 10;
            return View(db.DONDATHANGs.ToList().OrderBy(n => n.MaDonHang).ToPagedList(pageNumber, pageSize));
        }
        //Đơn hàng
        public ActionResult Chitietdonhang(int id)
        {
            //Lay doi tuong sach theo ma
            CHITIETDONTHANG dh = (CHITIETDONTHANG)db.CHITIETDONTHANGs.ToList().OrderBy(n => n.MaDonHang == id);
            ViewBag.MaDonHang = dh.MaDonHang;
            if (dh == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(dh);
        }
        //Người dùng
        public ActionResult Chitietnguoidung(int id)
        {
            //Lay doi tuong nguoi dung theo ma
            KHACHHANG nguoidung = db.KHACHHANGs.SingleOrDefault(n => n.MaKH == id);
            ViewBag.MaKH = nguoidung.MaKH;
            if (nguoidung == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(nguoidung);
        }

        //Phân loại điện thoại
        public ActionResult Loai(int? page)
        {
            if (Session["Taikhoanadmin"] == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            int pageNumber = (page ?? 1);
            int pageSize = 10;
            //return View(db.SACHes.ToList());
            return View(db.Loais.ToList().OrderBy(n => n.MaLoai).ToPagedList(pageNumber, pageSize));
        }
        //Sua san pham
        [HttpGet]
        public ActionResult SuaLoai(int id)
        {
            //Lay ra doi tuong sach can xoa theo ma
            Loai Loai = db.Loais.SingleOrDefault(n => n.MaLoai == id);
            if (Loai == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            //Lay ds tu table LOAI, sap xep theo ten loai, cho lay gia tri Ma Loai, hien thi Tenloai
            ViewBag.MaLoai = new SelectList(db.Loais.ToList().OrderBy(n => n.TenLoai), "MaLoai", "TenLoai");
            ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB");
            return View(Loai);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SuaLoai(Loai loai)
        {
            if (Session["Taikhoanadmin"] == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            //Dua du lieu vao dropdownload
            ViewBag.MaLoai = new SelectList(db.Loais.ToList().OrderBy(n => n.TenLoai), "MaLoai", "TenLoai");
            ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB");
            //Kiem tra duong dan file

            return RedirectToAction("Loai");
        }
        [HttpGet]
        public ActionResult XoaLoai(int id)
        {
            //Lay ra doi tuong sach can xoa theo ma
            Loai Loai = db.Loais.SingleOrDefault(n => n.MaLoai == id);
            ViewBag.MaLoai = Loai.MaLoai;
            if (Loai == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(Loai);
        }
        [HttpPost, ActionName("XoaLoai")]
        public ActionResult Xacnhanxoaloai(int id)
        {
            //Lay ra doi tuong sach can xoa theo ma
            Loai Loai = db.Loais.SingleOrDefault(n => n.MaLoai == id);
            ViewBag.MaLoai = Loai.MaLoai;
            if (Loai == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            db.Loais.DeleteOnSubmit(Loai);
            db.SubmitChanges();
            return RedirectToAction("Index");
        }
        //Phân loại điện thoại
        public ActionResult NXB(int? page)
        {
            if (Session["Taikhoanadmin"] == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            int pageNumber = (page ?? 1);
            int pageSize = 10;
            //return View(db.SACHes.ToList());
            return View(db.NHAXUATBANs.ToList().OrderBy(n => n.MaNXB).ToPagedList(pageNumber, pageSize));
        }
    }
}