# Kursach DoctorProfile datetime-local fix v8

Fixes current UI build errors:
- Hospital.UI/Pages/DoctorProfile.razor(89,86): cannot convert from string to DateTime
- generated Razor errors around TimeOnly?/string

Cause:
The datetime-local input used @bind directly with a string variable. Razor generated a typed binder that expected a DateTime/TimeOnly-compatible value.

Fix:
The input now uses a normal value attribute and @onchange handler:
<input type="datetime-local" value="@appointmentDateValue" @onchange="OnAppointmentDateChanged" />

Apply:
Expand-Archive -Path .\kursach_doctorprofile_datetime_fix_v8.zip -DestinationPath . -Force

Then:
dotnet clean "Cursach AppZ.slnx"
Get-ChildItem -Recurse -Directory -Include bin,obj | Remove-Item -Recurse -Force
dotnet restore "Cursach AppZ.slnx"
dotnet build "Cursach AppZ.slnx"
