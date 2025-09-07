# Medicare Connect - Role Responsibilities & System Status

## System User Roles

| Role | Responsibilities | Status |
| --- | --- | --- |
| Patients | ✅ Create and maintain secure digital profiles with medical history<br>✅ Book, modify, or cancel appointments online (Stripe checkout for booking payments)<br>✅ View personal medical records and test results<br>✅ Access and review digital prescriptions (request refills, pay now or later)<br>✅ Make online payments for services (Stripe, per appointment/refill)<br>✅ Receive appointment and medication reminders (auto-create on booking; manual add/toggle/delete)<br>✅ View consultations (schedule and details)<br>✅ Set preferred language | ✅ COMPLETE |
| Doctors | ✅ Access and review patient medical records<br>🟡 Conduct digital consultations and update treatment plans (create and list consultations; completion/notes pending)
<br>🟡 Create electronic prescriptions (pharmacy workflow pending)
<br>✅ Approve prescription refill requests<br>✅ Manage personal appointment schedules<br>⚠️ Add clinical notes and treatment documentation<br>⚠️ Review test results and patient history<br>⚠️ Communicate with other healthcare staff | 🟡 PARTIAL |
| Nurses | ✅ Assist with patient care coordination<br>✅ Update patient records with nursing notes<br>✅ Collaborate with doctors and other staff<br>⚠️ Manage patient flow and triage<br>✅ Access patient information for care delivery<br>⚠️ Coordinate follow-up care | 🟡 PARTIAL |
| Pharmacists | ⚠️ Receive and process electronic prescriptions<br>⚠️ Manage medication inventory and stock levels<br>⚠️ Document medication dispensing<br>⚠️ Track drug interactions and allergies<br>⚠️ Monitor prescription histories<br>⚠️ Maintain pharmacy records<br>⚠️ Handle medication refill requests | ⚠️ INCOMPLETE |
| Administrative Staff | ✅ Register new patients in the system<br>✅ Manage appointment scheduling and confirmations<br>✅ Handle billing and payment processing<br>✅ Manage staff schedules and leave requests<br>✅ Generate reports and analytics<br>✅ Maintain system user accounts<br>✅ Handle patient inquiries and support | ✅ COMPLETE |
| Managers/Admin | ⚠️ Oversee system operations and user management<br>⚠️ Access comprehensive analytics and reports<br>⚠️ Manage staff permissions and roles<br>⚠️ Monitor system performance and security<br>⚠️ Handle escalated issues<br>⚠️ Make strategic decisions based on system data<br>⚠️ Ensure compliance and audit trail maintenance | ⚠️ INCOMPLETE |

### Role Responsibilities Detail

| Role | Responsibility | Status |
| --- | --- | --- |
| Patients | Create and maintain secure digital profiles with medical history | ✅ COMPLETE |
| Patients | Book, modify, or cancel appointments online | ✅ COMPLETE |
| Patients | View personal medical records and test results | ✅ COMPLETE  |
| Patients | Access and review digital prescriptions | ✅ COMPLETE  |
| Patients | Request prescription refills (pay now with Stripe or later) | ✅ COMPLETE |
| Patients | Make online payments for services | ✅ COMPLETE  |
| Patients | Receive appointment and medication reminders | ✅ COMPLETE |
| Patients | Update personal and contact information | ✅ COMPLETE |
| Patients | View consultations | ✅ COMPLETE |
| Doctors | Access and review patient medical records | ✅ COMPLETE  |
| Doctors | Conduct digital consultations and update treatment plans | 🟡 PARTIAL |
| Doctors | Create electronic prescriptions | 🟡 PARTIAL |
| Doctors | Approve prescription refill requests | ✅ COMPLETE |
| Doctors | Manage personal appointment schedules | ✅ COMPLETE  |
| Doctors | Add clinical notes and treatment documentation | ⚠️ INCOMPLETE |
| Doctors | Review test results and patient history | ⚠️ INCOMPLETE |
| Doctors | Communicate with other healthcare staff | ⚠️ INCOMPLETE |
| Nurses | Assist with patient care coordination | ✅ COMPLETE |
| Nurses | Update patient records with nursing notes | ✅ COMPLETE |
| Nurses | Collaborate with doctors and other staff | ✅ COMPLETE |
| Nurses | Manage patient flow and triage | ⚠️ INCOMPLETE |
| Nurses | Access patient information for care delivery | ✅ COMPLETE |
| Nurses | Coordinate follow-up care | ⚠️ INCOMPLETE |

## System Capabilities Status

| Feature | Status |
| --- | --- |
| Patient registration and profiles | ✅ COMPLETE |
| Online appointment booking | 🟡 PARTIAL |
| Electronic medical records (EMR) | 🟡 PARTIAL |
| Digital prescriptions | 🟡 PARTIAL |
| Basic billing and payments | ✅ COMPLETE |
| Role-based access control | ✅ COMPLETE |
| Mobile and web access | ⚠️ INCOMPLETE |
| Staff scheduling system | ✅ COMPLETE |
| Notification and alert system | 🟡 PARTIAL |
| Pharmacy integration | ⚠️ INCOMPLETE |
| Follow-up and reminder system | ✅ COMPLETE |
| Security and compliance measures | ⚠️ INCOMPLETE |

## Codebase Index by Role

See `CODEBASE_INDEX.md` for the full index. Highlights below map roles to implemented areas and controllers/actions.

- Patients
  - Area: `Patients`
  - Dashboard: `/Patients/Dashboard`
  - Controllers/Actions:
    - `AppointmentsController`: `Index`, `Book`, `Edit`, `Cancel`
    - `PaymentsController`: `Index`, `RefillPaid`
    - `PrescriptionsController`: `Index`, `RequestRefill`, `PayRefill`
    - `ConsultationsController`: `Index`
    - `ProfileController`: `Index (GET)`, `Index (POST)`, `RemoveProfilePhoto`
    - `RecordsController`: `Index`
    - `RemindersController`: `Index`
    - `SettingsController`: `Index`

- Doctors
  - Area: `Doctors`
  - Dashboard: `/Doctors/Dashboard`
  - Controllers/Actions:
    - `AppointmentsController`: `Index`, `Complete`, `Cancel`
    - `PatientsController`: `Index`
    - `PrescriptionsController`: `Index`, `For`, `Create`, `ApproveRefill`, `RejectRefill`
    - `ConsultationsController`: `Index`, `For`, `Create`

- Nurses
  - Area: `Nurses`
  - Dashboard: `/Nurses/Dashboard`
  - Controllers/Actions:
    - `DashboardController`: `Index`
    - `PatientsController`: `Index`, `Care`
    - `CareController`: `Index`
    - `RecordsController`: `Index`
    - `ScheduleController`: `Index`
    - `ProfileController`: `Index`

- Pharmacists
  - Area: `Pharmacists`
  - Dashboard: `/Pharmacists/Dashboard`

- Administrative Staff
  - Area: `AdministrativeStaff`
  - Dashboard: `/AdministrativeStaff/Dashboard`
  - Controllers/Actions:
    - `DashboardController`: `Index`
    - `PatientsController`: `Index`, `Register`, `Details`
    - `AppointmentsController`: `Index`, `Create`
    - `BillingController`: `Index`, `Create`
    - `SchedulingController`: `Index`, `Create`, `LeaveRequests`
    - `ReportsController`: `Index`, `Generate`
    - `SupportController`: `Index`, `Create`
    - `ProfileController`: `Index`, `Edit`

- Managers/Admin
  - Area: `Managers`
  - Dashboard: `/Managers/Dashboard`
  - Controllers/Actions: `DashboardController.Index` (additional features pending)