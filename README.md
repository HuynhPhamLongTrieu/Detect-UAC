# UAC Bypass Detector v3.1 - Auto Kill Mode

**Công cụ phát hiện và ngăn chặn UAC Bypass thời gian thực**

Một tool giám sát realtime các kỹ thuật bypass UAC phổ biến (Debug Object Stealing + PPID Spoofing qua ComputerDefaults.exe và winver.exe).

---

## ✨ Tính năng chính

- **Giám sát realtime** mọi tiến trình được tạo trên Windows bằng WMI Event Watcher.
- **Phát hiện nhanh** kỹ thuật UAC Bypass sử dụng `winver.exe` và `ComputerDefaults.exe`.
- **Tự động Kill** tiến trình payload vi phạm (stager.exe, payload.exe...).
- **Ghi log chi tiết** ra file `UAC_Bypass_Detected.log`.
- **Giao diện console** rõ ràng, dễ theo dõi.
- Hỗ trợ bật/tắt Auto Kill (trong phiên bản nâng cao).

---

## 🚀 Cách sử dụng

### 1. Build project
- Mở project trong **Visual Studio**.
- Đảm bảo đã add **NuGet Package**: `System.Management`.
- Build solution (`Ctrl + Shift + B`).

### 2. Chạy tool
1. Right-click vào file `DetectUAC.exe` → **Run as administrator**.
2. Tool sẽ hiển thị:
[+] Detector đã sẵn sàng (Auto Kill Mode)!
text3. **Giữ tool chạy** và tiến hành test file bypass UAC (`example.exe`).

### 3. Kiểm tra kết quả
- Khi phát hiện UAC Bypass, tool sẽ:
- In thông báo đỏ rõ ràng.
- Tự động kill tiến trình payload.
- Ghi log vào file `UAC_Bypass_Detected.log`.

---

## 🔧 Cách hoạt động

Tool sử dụng cơ chế **WMI Event Watcher** (`__InstanceCreationEvent`) để lắng nghe mọi sự kiện tạo tiến trình mới trên hệ thống.

**Quy trình phát hiện:**

1. Khi `winver.exe` hoặc `ComputerDefaults.exe` được khởi tạo → tool ghi nhận làm **parent đáng ngờ**.
2. Khi có tiến trình con được spawn với `ParentProcessID` trùng với parent trên:
- Kiểm tra xem tiến trình con có nằm trong danh sách hợp pháp không (`conhost.exe`, `dllhost.exe`, ...).
- Nếu **không hợp pháp** → coi là UAC Bypass.
3. Thực hiện **Kill process** payload và ghi log.

---

## ⚠️ Các trường hợp False Positive (Kill nhầm)

Tool có thể kill nhầm trong các trường hợp sau:

- Một số phần mềm hợp pháp sử dụng `ComputerDefaults.exe` làm parent để spawn tiến trình con (ví dụ: một số installer, tool debug, script hệ thống).
- Tiến trình `notepad.exe`, `calc.exe`, hoặc các chương trình thông thường được bypass UAC bằng kỹ thuật tương tự.
- Quá trình cập nhật Windows hoặc một số tính năng hệ thống chạy ngầm.
- Người dùng chạy nhiều instance của `ComputerDefaults.exe` cùng lúc.

**Cách giảm False Positive:**
- Tắt Auto Kill bằng cách chỉnh `AutoKillEnabled = false`.
- Mở rộng danh sách `LEGITIMATE_CHILDREN`.
- Thêm kiểm tra đường dẫn file (chỉ kill nếu nằm ngoài `%System32%`).

---

## 📋 Yêu cầu hệ thống

- **Hệ điều hành**: Windows 10 / Windows 11 (64-bit)
- **Quyền**: Phải chạy với quyền **Administrator**
- **.NET**: .NET 8.0 (hoặc .NET Framework 4.7+)
- **Package**: System.Management

---

## 📁 Cấu trúc project
UAC_RealTime_Detector/
├── Program.cs              ← Mã nguồn chính
├── UAC_Bypass_Detected.log ← File log tự động tạo
├── bin/
└── obj/
text---

**Đồ án môn học**  
**Mô tả**: Công cụ phát hiện và ngăn chặn kỹ thuật bypass UAC bằng phương pháp Debug Object Stealing + PPID Spoofing.

---

Nếu bạn cần thêm phần **"Hướng dẫn nộp đồ án"**, **báo cáo chi tiết**, hoặc **phiên bản có GUI**, cứ nói
