# FloodRelief Backend Refactor

## สิ่งที่ปรับโครงสร้าง

- แยก business logic ออกจาก Controller ทุกตัวไปยังโฟลเดอร์ `Services`
- Controller เหลือหน้าที่รับ HTTP request, ส่งต่อไปยัง Service และคืน response
- รักษา Route, HTTP method, Authorize attribute, DTO และรูปแบบ response เดิม
- เพิ่ม Dependency Injection ของ Service ทั้งหมดใน `Program.cs`
- แยก WeatherForecast logic ไปยัง `WeatherForecastService`

## Controller ที่แยกแล้ว

- AuthController -> AuthService
- UsersController -> UsersService
- StaffsController -> StaffsService
- CentersController -> CentersService
- CenterInventoriesController -> CenterInventoriesService
- DonationsController -> DonationsService
- ReliefCategoriesController -> ReliefCategoriesService
- ReliefItemsController -> ReliefItemsService
- SosRequestsController -> SosRequestsService
- ThaiAddressesController -> ThaiAddressesService
- UploadController -> UploadService
- WeatherForecastController -> WeatherForecastService

## โครงสร้างใหม่

```text
Controllers/       HTTP endpoints เท่านั้น
Services/          logic เดิมจาก Controller
DTOs/              request/response models
Models/            database entities
Data/              AppDbContext
Constants/         ค่าคงที่ของระบบ
```

## หมายเหตุสำคัญ

Service ชุดนี้ยังคืนค่าเป็น `IActionResult` เพื่อรักษาพฤติกรรมและ response ของ API เดิมทั้งหมด และลดความเสี่ยงที่ Frontend จะพังระหว่าง refactor ครั้งใหญ่ ขั้นถัดไปสามารถปรับ Service ให้คืน `ServiceResult<T>` และแยก validation/helper/repository ได้โดยไม่กระทบ route ภายนอก

## การทดสอบที่แนะนำ

1. รัน `dotnet restore`
2. รัน `dotnet build`
3. เปิด Swagger และทดสอบ Auth, Center, SOS, Donation, Inventory
4. ทดสอบ role User / Staff / Admin
5. ทดสอบ transaction รับบริจาคและตัด stock จาก SOS

สภาพแวดล้อมที่ใช้จัดไฟล์ครั้งนี้ไม่มี .NET SDK จึงไม่สามารถรัน `dotnet build` ภายในระบบได้ ควร build บนเครื่องก่อน merge เข้า branch หลัก

## Primary Key Helper update

เพิ่ม `Helpers/PrimaryKeyHelper.cs` เพื่อรวม logic การสร้างและตรวจสอบ Primary Key แบบตัวเลขที่เก็บเป็น string ไว้จุดเดียว

รองรับ:
- สร้างรหัสถัดไปจากข้อมูลล่าสุด
- กำหนดจำนวนหลัก เช่น 2, 5, 8 และ 10 หลัก
- เติมเลขศูนย์ด้านหน้า
- ตรวจสอบรูปแบบรหัสก่อนเพิ่มค่า
- ป้องกันรหัสเกินช่วงสูงสุด
- เพิ่มรหัสต่อเนื่องสำหรับการสร้างหลายรายการใน transaction เดียว

นำไปใช้กับ:
- User
- Staff
- Admin
- Center
- ReliefCategory
- ReliefItem
- Donation
- DonationItem
- CenterInventory
- InventoryTransaction
- SosRequest
- SosRequestItem

เมธอด Generate/Increment ID ที่ซ้ำอยู่ในแต่ละ Service ถูกลบออกแล้ว


## Refactor รอบล่าสุด: ยกเลิก ControllerContext ใน Service

- ลบ `_service.ControllerContext = ControllerContext;` ออกจาก Controller ทุกตัว
- เพิ่ม `Services/Common/CurrentUserService.cs` เพื่ออ่าน UserId, CenterId และ Role จาก JWT Claims
- Service ที่ต้องใช้ข้อมูลผู้ใช้ปัจจุบันรับ `CurrentUserService` ผ่าน Dependency Injection
- ย้าย `[HttpGet]`, `[HttpPost]`, `[Authorize]` และ `[FromBody]` ให้คงอยู่เฉพาะ Controller
- Route, HTTP method, request DTO, response body และ status code เดิมไม่เปลี่ยน จึงไม่กระทบ Frontend
- ไม่เพิ่ม Repository หรือ Validator ตามที่ขอ

### Service ที่ใช้ CurrentUserService

- DonationsService
- SosRequestsService
- CenterInventoriesService

### การลงทะเบียน DI ที่เพิ่ม

```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentUserService>();
```
