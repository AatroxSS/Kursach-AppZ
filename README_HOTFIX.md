# Kursach-AppZ hotfix

Скопіюй ці файли поверх відповідних файлів у гілці `trash`.

Що виправлено:

1. `Cursach AppZ.slnx` — API та UI більше не виключені з Debug build.
2. `Hospital.API/Program.cs` — додано реєстрацію `IAppointmentService`, винесено JWT у конфіг, явно підключено мапінг і сервіси.
3. `AppointmentsController` + `IAppointmentService` + `AppointmentService` — додано `GET api/Appointments`, перевірку пацієнта, перевірку зайнятого часу лікаря, нормальну обробку null у Notes.
4. `AuthService` — JWT ключ більше не хардкодиться у сервісі, роль парситься case-insensitive, невалідна роль дає помилку.
5. `HospitalMapperProfile` — прибрані сміттєві using-и, додані нормальні мапи з null-safe FullName.
6. `CustomAuthStateProvider` — роль з JWT тепер мапиться у `ClaimTypes.Role`, тому `IsInRole(...)` і AuthorizeView починають працювати нормально.
7. `DoctorProfile.razor` — більше не викликає неіснуючий `api/Appointments`, а використовує `api/Appointments/doctor/{id}`.
8. `Doctors.razor` — використовує відносний шлях `api/Doctors`, а не захардкоджений `https://localhost:7012/api/Doctors`.
9. `AppointmentServiceTests` — оновлені під нову валідацію.

Команди після заміни:

```powershell
git switch trash
dotnet restore "Cursach AppZ.slnx"
dotnet build "Cursach AppZ.slnx"
dotnet test "Hospital.Tests/Hospital.Tests.csproj"
dotnet run --project "Hospital.API/Hospital.API.csproj" --launch-profile https
dotnet run --project "Hospital.UI/Hospital.UI.csproj"
```

У `Hospital.API/appsettings.json` вже є потрібні `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience` і `ConnectionStrings:DefaultConnection`, тому окремо їх додавати не треба.
