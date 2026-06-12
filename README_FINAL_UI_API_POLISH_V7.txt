# Kursach final UI/API polish v7

Цей пакет робить інтерфейс цілісним і прибирає "дефолтний" вигляд Blazor.

Що замінює:
- Hospital.UI/App.razor
- Hospital.UI/_Imports.razor
- Hospital.UI/Program.cs
- Hospital.UI/Layout/MainLayout.razor
- Hospital.UI/Layout/NavMenu.razor
- Hospital.UI/wwwroot/index.html
- Hospital.UI/wwwroot/css/app.css
- Hospital.UI/Models/ApiModels.cs
- Hospital.UI/Providers/CustomAuthStateProvider.cs
- Hospital.UI/Pages/Index.razor
- Hospital.UI/Pages/Login.razor
- Hospital.UI/Pages/Register.razor
- Hospital.UI/Pages/Doctors.razor
- Hospital.UI/Pages/DoctorProfile.razor
- Hospital.UI/Pages/Profile.razor
- Hospital.UI/Pages/AdminPanel.razor
- Hospital.UI/Pages/NotFound.razor
- Hospital.API/Controllers/PatientsController.cs
- Hospital.API/Controllers/DoctorsController.cs
- Hospital.API/Controllers/AppointmentsController.cs
- Hospital.BLL/Services/AuthService.cs

Що виправлено:
1. Єдиний нормальний layout: sidebar + topbar + content cards.
2. Сторінки більше не виглядають як гола HTML-таблиця.
3. UI більше не тягне Hospital.BLL.DTO напряму.
4. Ролі нормально читаються з JWT у Blazor.
5. Адмін може створювати користувача і одразу ставити роль.
6. При зміні ролі синхронізується профіль: Manager -> Doctor, RegisteredUser -> Patient, Administrator -> без doctor/patient профілю.
7. Пацієнт може записатися до лікаря через /doctor/{id}, не вводячи PatientId вручну.
8. /profile показує релевантні записи для ролі: пацієнт бачить свої, лікар свої, адмін усі.
9. API має Patients/me, Doctors/me, Appointments/my.

Команди:
Expand-Archive -Path .\kursach_final_ui_api_polish_v7.zip -DestinationPath . -Force

dotnet clean "Cursach AppZ.slnx"
Get-ChildItem -Recurse -Directory -Include bin,obj | Remove-Item -Recurse -Force
dotnet restore "Cursach AppZ.slnx"
dotnet build "Cursach AppZ.slnx"

Запуск:
dotnet run --project .\Hospital.API\Hospital.API.csproj --launch-profile https

В іншому терміналі:
dotnet run --project .\Hospital.UI\Hospital.UI.csproj --launch-profile http

Потім:
http://localhost:5174

Після заміни файлів у браузері натисни Ctrl+F5.
