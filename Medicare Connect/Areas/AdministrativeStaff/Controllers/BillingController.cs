using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medicare_Connect.Areas.AdministrativeStaff.Models;
using Medicare_Connect.Data;
using Medicare_Connect.Data.Entities;

namespace Medicare_Connect.Areas.AdministrativeStaff.Controllers;

[Area("AdministrativeStaff")]
[Authorize(Roles = "Administrative Staff")]
public class BillingController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _context;

    public BillingController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, string? status, DateTime? startDate, DateTime? endDate)
    {
        var patients = await _userManager.GetUsersInRoleAsync("Patients");
        
        // Query actual billing data from database
        var billingQuery = _context.Billings
            .Include(b => b.Patient)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            billingQuery = billingQuery.Where(b => 
                (b.Patient != null && (b.Patient.Email != null && b.Patient.Email.Contains(searchTerm)) ||
                 (b.Patient.UserName != null && b.Patient.UserName.Contains(searchTerm))) ||
                b.ServiceType.Contains(searchTerm)
            );
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            billingQuery = billingQuery.Where(b => b.PaymentStatus.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        if (startDate.HasValue)
        {
            billingQuery = billingQuery.Where(b => b.ServiceDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            billingQuery = billingQuery.Where(b => b.ServiceDate <= endDate.Value);
        }

        var billingEntities = await billingQuery
            .OrderByDescending(b => b.ServiceDate)
            .ToListAsync();

        // Convert to view model
        var billingList = billingEntities.Select(b => new BillingListItem
        {
            Id = b.Id,
            PatientName = b.Patient?.Email ?? b.Patient?.UserName ?? "Unknown Patient",
            ServiceType = b.ServiceType,
            ServiceDate = b.ServiceDate,
            Amount = b.Amount,
            InsuranceAmount = b.InsuranceAmount,
            PatientResponsibility = b.PatientResponsibility,
            PaymentStatus = b.PaymentStatus,
            DueDate = b.DueDate,
            IsOverdue = b.IsOverdue
        }).ToList();

        ViewBag.SearchTerm = searchTerm;
        ViewBag.Status = status;
        ViewBag.StartDate = startDate;
        ViewBag.EndDate = endDate;
        ViewBag.Patients = patients.Select(p => new { Id = p.Id, Name = p.Email ?? p.UserName ?? p.Id }).ToList();

        return View(billingList);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var patients = await _userManager.GetUsersInRoleAsync("Patients");
        ViewBag.Patients = patients.Select(p => new { Id = p.Id, Name = p.Email ?? p.UserName ?? p.Id }).ToList();

        return View(new BillingViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BillingViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var patients = await _userManager.GetUsersInRoleAsync("Patients");
            ViewBag.Patients = patients.Select(p => new { Id = p.Id, Name = p.Email ?? p.UserName ?? p.Id }).ToList();
            return View(model);
        }

        try
        {
            // Calculate patient responsibility
            if (model.InsuranceCoverage && model.InsuranceAmount.HasValue)
            {
                model.PatientResponsibility = model.Amount - model.InsuranceAmount.Value;
            }
            else
            {
                model.PatientResponsibility = model.Amount;
            }

            // Create and save billing entity
            var billingEntity = new BillingEntity
            {
                PatientId = model.PatientId,
                ServiceType = model.ServiceType,
                ServiceDate = model.ServiceDate,
                Amount = model.Amount,
                InsuranceAmount = model.InsuranceCoverage ? model.InsuranceAmount : null,
                PatientResponsibility = model.PatientResponsibility,
                PaymentStatus = model.PaymentStatus,
                DueDate = model.DueDate,
                Notes = model.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Billings.Add(billingEntity);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Billing record created successfully. Patient responsibility: {model.PatientResponsibility:C}";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            
            var patients = await _userManager.GetUsersInRoleAsync("Patients");
            ViewBag.Patients = patients.Select(p => new { Id = p.Id, Name = p.Email ?? p.UserName ?? p.Id }).ToList();
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> SeedSampleData()
    {
        try
        {
            // Check if we already have billing data
            if (await _context.Billings.AnyAsync())
            {
                TempData["InfoMessage"] = "Sample data already exists.";
                return RedirectToAction(nameof(Index));
            }

            var patients = await _userManager.GetUsersInRoleAsync("Patients");
            if (!patients.Any())
            {
                TempData["ErrorMessage"] = "No patients found. Please register some patients first.";
                return RedirectToAction(nameof(Index));
            }

            var sampleBillings = new List<BillingEntity>();
            var random = new Random();

            foreach (var patient in patients.Take(5)) // Create sample data for first 5 patients
            {
                var serviceDate = DateTime.Today.AddDays(-random.Next(1, 30));
                var amount = random.Next(5000, 50000) / 100.0m;
                var insuranceAmount = random.Next(0, (int)(amount * 100)) / 100.0m;
                var patientResponsibility = amount - insuranceAmount;
                var dueDate = serviceDate.AddDays(30);

                var serviceTypes = new[] { "General Checkup", "Follow-up", "Lab Results", "Vaccination", "Specialist Consultation", "Emergency Care", "Surgery", "Physical Therapy" };
                var paymentStatuses = new[] { "Pending", "Paid", "Overdue", "Partial", "Cancelled" };

                sampleBillings.Add(new BillingEntity
                {
                    PatientId = patient.Id,
                    ServiceType = serviceTypes[random.Next(serviceTypes.Length)],
                    ServiceDate = serviceDate,
                    Amount = amount,
                    InsuranceAmount = insuranceAmount > 0 ? insuranceAmount : null,
                    PatientResponsibility = patientResponsibility,
                    PaymentStatus = paymentStatuses[random.Next(paymentStatuses.Length)],
                    DueDate = dueDate,
                    Notes = "Sample billing record for demonstration",
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.Billings.AddRange(sampleBillings);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Successfully created {sampleBillings.Count} sample billing records.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred while seeding data: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Payments(string? searchTerm, string? status, string? paymentType, DateTime? startDate, DateTime? endDate)
    {
        var patients = await _userManager.GetUsersInRoleAsync("Patients");
        
        // Query actual payment data from database
        var paymentsQuery = _context.Payments
            .Include(p => p.Patient)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            paymentsQuery = paymentsQuery.Where(p => 
                (p.Patient != null && (p.Patient.Email != null && p.Patient.Email.Contains(searchTerm)) ||
                 (p.Patient.UserName != null && p.Patient.UserName.Contains(searchTerm))) ||
                p.Description.Contains(searchTerm) ||
                p.ReferenceId.Contains(searchTerm)
            );
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (Enum.TryParse<PaymentStatus>(status, out var statusEnum))
            {
                paymentsQuery = paymentsQuery.Where(p => p.Status == statusEnum);
            }
        }

        if (!string.IsNullOrWhiteSpace(paymentType))
        {
            if (Enum.TryParse<PaymentType>(paymentType, out var typeEnum))
            {
                paymentsQuery = paymentsQuery.Where(p => p.PaymentType == typeEnum);
            }
        }

        if (startDate.HasValue)
        {
            paymentsQuery = paymentsQuery.Where(p => p.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            paymentsQuery = paymentsQuery.Where(p => p.CreatedAt <= endDate.Value);
        }

        var paymentEntities = await paymentsQuery
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        // Convert to view model
        var paymentList = paymentEntities.Select(p => new PaymentListItem
        {
            Id = p.Id,
            PatientName = p.Patient?.Email ?? p.Patient?.UserName ?? "Unknown Patient",
            PatientEmail = p.Patient?.Email ?? "No email",
            PaymentType = p.PaymentType.ToString(),
            Description = p.Description,
            Amount = p.Amount,
            Status = p.Status.ToString(),
            CreatedAt = p.CreatedAt,
            CompletedAt = p.CompletedAt,
            StripeSessionId = p.StripeSessionId,
            ReferenceId = p.ReferenceId,
            Notes = p.Notes
        }).ToList();

        ViewBag.SearchTerm = searchTerm;
        ViewBag.Status = status;
        ViewBag.PaymentType = paymentType;
        ViewBag.StartDate = startDate;
        ViewBag.EndDate = endDate;
        ViewBag.Patients = patients.Select(p => new { Id = p.Id, Name = p.Email ?? p.UserName ?? p.Id }).ToList();

        return View(paymentList);
    }

    [HttpGet]
    public async Task<IActionResult> SeedSamplePayments()
    {
        try
        {
            // Check if we already have payment data
            if (await _context.Payments.AnyAsync())
            {
                TempData["InfoMessage"] = "Sample payment data already exists.";
                return RedirectToAction(nameof(Payments));
            }

            var patients = await _userManager.GetUsersInRoleAsync("Patients");
            if (!patients.Any())
            {
                TempData["ErrorMessage"] = "No patients found. Please register some patients first.";
                return RedirectToAction(nameof(Payments));
            }

            var samplePayments = new List<PaymentEntity>();
            var random = new Random();

            foreach (var patient in patients.Take(5)) // Create sample data for first 5 patients
            {
                var paymentTypes = new[] { PaymentType.Appointment, PaymentType.PrescriptionRefill, PaymentType.Consultation };
                var paymentStatuses = new[] { PaymentStatus.Completed, PaymentStatus.Pending, PaymentStatus.Failed };
                var descriptions = new[] 
                { 
                    "General Checkup Appointment", 
                    "Follow-up Consultation", 
                    "Prescription Refill - Antibiotics",
                    "Lab Results Review",
                    "Vaccination Appointment"
                };

                // Create 2-3 sample payments per patient
                for (int i = 0; i < random.Next(2, 4); i++)
                {
                    var paymentType = paymentTypes[random.Next(paymentTypes.Length)];
                    var status = paymentStatuses[random.Next(paymentStatuses.Length)];
                    var amount = random.Next(5000, 50000) / 100.0m;
                    var createdAt = DateTime.Today.AddDays(-random.Next(1, 30));
                    var completedAt = status == PaymentStatus.Completed ? createdAt.AddHours(random.Next(1, 24)) : (DateTime?)null;

                    samplePayments.Add(new PaymentEntity
                    {
                        PatientId = patient.Id,
                        PaymentType = paymentType,
                        Description = descriptions[random.Next(descriptions.Length)],
                        Amount = amount,
                        Status = status,
                        StripeSessionId = status == PaymentStatus.Completed ? $"cs_test_{Guid.NewGuid().ToString("N")}" : null,
                        StripePaymentIntentId = status == PaymentStatus.Completed ? $"pi_{Guid.NewGuid().ToString("N")}" : null,
                        ReferenceId = Guid.NewGuid().ToString("N").Substring(0, 8),
                        Notes = random.Next(0, 3) == 0 ? "Sample payment record for demonstration" : null,
                        CreatedAt = createdAt,
                        CompletedAt = completedAt
                    });
                }
            }

            _context.Payments.AddRange(samplePayments);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Successfully created {samplePayments.Count} sample payment records.";
            return RedirectToAction(nameof(Payments));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred while seeding payment data: {ex.Message}";
            return RedirectToAction(nameof(Payments));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            TempData["ErrorMessage"] = "Billing record ID is required.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            // Log the ID being searched for debugging
            System.Diagnostics.Debug.WriteLine($"Searching for billing ID: {id}");
            
            // Check if there are any billing records at all
            var totalBillings = await _context.Billings.CountAsync();
            System.Diagnostics.Debug.WriteLine($"Total billing records in database: {totalBillings}");
            
            // Get a sample of existing billing IDs for debugging
            if (totalBillings > 0)
            {
                var sampleBilling = await _context.Billings.Select(b => new { b.Id, b.ServiceType }).FirstAsync();
                System.Diagnostics.Debug.WriteLine($"Sample billing ID: {sampleBilling.Id}, Type: {sampleBilling.ServiceType}");
            }
            
            var billing = await _context.Billings
                .Include(b => b.Patient)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (billing == null)
            {
                TempData["ErrorMessage"] = $"Billing record with ID '{id}' not found. Total records in database: {totalBillings}";
                return RedirectToAction(nameof(Index));
            }

        var viewModel = new BillingViewModel
        {
            PatientId = billing.PatientId,
            ServiceType = billing.ServiceType,
            ServiceDate = billing.ServiceDate,
            Amount = billing.Amount,
            InsuranceCoverage = billing.InsuranceAmount.HasValue,
            InsuranceAmount = billing.InsuranceAmount,
            PatientResponsibility = billing.PatientResponsibility,
            PaymentStatus = billing.PaymentStatus,
            DueDate = billing.DueDate,
            Notes = billing.Notes
        };

        ViewBag.PatientName = billing.Patient?.Email ?? billing.Patient?.UserName ?? "Unknown Patient";
        ViewBag.BillingId = billing.Id;
        ViewBag.CreatedAt = billing.CreatedAt;
        ViewBag.UpdatedAt = billing.UpdatedAt;
        ViewBag.IsOverdue = billing.IsOverdue;

        return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred while retrieving billing details: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            TempData["ErrorMessage"] = "Billing record ID is required.";
            return RedirectToAction(nameof(Index));
        }

        var billing = await _context.Billings
            .Include(b => b.Patient)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (billing == null)
        {
            TempData["ErrorMessage"] = "Billing record not found.";
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new BillingViewModel
        {
            PatientId = billing.PatientId,
            ServiceType = billing.ServiceType,
            ServiceDate = billing.ServiceDate,
            Amount = billing.Amount,
            InsuranceCoverage = billing.InsuranceAmount.HasValue,
            InsuranceAmount = billing.InsuranceAmount,
            PatientResponsibility = billing.PatientResponsibility,
            PaymentStatus = billing.PaymentStatus,
            DueDate = billing.DueDate,
            Notes = billing.Notes
        };

        var patients = await _userManager.GetUsersInRoleAsync("Patients");
        ViewBag.Patients = patients.Select(p => new { Id = p.Id, Name = p.Email ?? p.UserName ?? p.Id }).ToList();
        ViewBag.BillingId = billing.Id;

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, BillingViewModel model)
    {
        if (string.IsNullOrEmpty(id))
        {
            TempData["ErrorMessage"] = "Billing record ID is required.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            var patients = await _userManager.GetUsersInRoleAsync("Patients");
            ViewBag.Patients = patients.Select(p => new { Id = p.Id, Name = p.Email ?? p.UserName ?? p.Id }).ToList();
            ViewBag.BillingId = id;
            return View(model);
        }

        try
        {
            var billing = await _context.Billings.FindAsync(id);
            if (billing == null)
            {
                TempData["ErrorMessage"] = "Billing record not found.";
                return RedirectToAction(nameof(Index));
            }

            // Calculate patient responsibility
            if (model.InsuranceCoverage && model.InsuranceAmount.HasValue)
            {
                model.PatientResponsibility = model.Amount - model.InsuranceAmount.Value;
            }
            else
            {
                model.PatientResponsibility = model.Amount;
            }

            // Update billing entity
            billing.PatientId = model.PatientId;
            billing.ServiceType = model.ServiceType;
            billing.ServiceDate = model.ServiceDate;
            billing.Amount = model.Amount;
            billing.InsuranceAmount = model.InsuranceCoverage ? model.InsuranceAmount : null;
            billing.PatientResponsibility = model.PatientResponsibility;
            billing.PaymentStatus = model.PaymentStatus;
            billing.DueDate = model.DueDate;
            billing.Notes = model.Notes;
            billing.UpdatedAt = DateTime.UtcNow;

            _context.Billings.Update(billing);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Billing record updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            
            var patients = await _userManager.GetUsersInRoleAsync("Patients");
            ViewBag.Patients = patients.Select(p => new { Id = p.Id, Name = p.Email ?? p.UserName ?? p.Id }).ToList();
            ViewBag.BillingId = id;
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePaymentStatus(string id, string newStatus)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(newStatus))
        {
            return Json(new { success = false, message = "Invalid parameters." });
        }

        try
        {
            var billing = await _context.Billings.FindAsync(id);
            if (billing == null)
            {
                return Json(new { success = false, message = "Billing record not found." });
            }

            billing.PaymentStatus = newStatus;
            billing.UpdatedAt = DateTime.UtcNow;

            _context.Billings.Update(billing);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Payment status updated successfully." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            TempData["ErrorMessage"] = "Billing record ID is required.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var billing = await _context.Billings.FindAsync(id);
            if (billing == null)
            {
                TempData["ErrorMessage"] = "Billing record not found.";
                return RedirectToAction(nameof(Index));
            }

            _context.Billings.Remove(billing);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Billing record deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkUpdatePaymentStatus(List<string> billingIds, string newStatus)
    {
        if (billingIds == null || !billingIds.Any() || string.IsNullOrEmpty(newStatus))
        {
            return Json(new { success = false, message = "Invalid parameters." });
        }

        try
        {
            var billings = await _context.Billings
                .Where(b => billingIds.Contains(b.Id))
                .ToListAsync();

            if (!billings.Any())
            {
                return Json(new { success = false, message = "No billing records found." });
            }

            foreach (var billing in billings)
            {
                billing.PaymentStatus = newStatus;
                billing.UpdatedAt = DateTime.UtcNow;
            }

            _context.Billings.UpdateRange(billings);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Successfully updated {billings.Count} billing record(s)." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportBillingData(string? searchTerm, string? status, DateTime? startDate, DateTime? endDate)
    {
        var billingQuery = _context.Billings
            .Include(b => b.Patient)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            billingQuery = billingQuery.Where(b => 
                (b.Patient != null && (b.Patient.Email != null && b.Patient.Email.Contains(searchTerm)) ||
                 (b.Patient.UserName != null && b.Patient.UserName.Contains(searchTerm))) ||
                b.ServiceType.Contains(searchTerm)
            );
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            billingQuery = billingQuery.Where(b => b.PaymentStatus.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        if (startDate.HasValue)
        {
            billingQuery = billingQuery.Where(b => b.ServiceDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            billingQuery = billingQuery.Where(b => b.ServiceDate <= endDate.Value);
        }

        var billingEntities = await billingQuery
            .OrderByDescending(b => b.ServiceDate)
            .ToListAsync();

        // Convert to CSV format
        var csvContent = "Patient,Service Type,Service Date,Amount,Insurance Amount,Patient Responsibility,Payment Status,Due Date,Notes\n";
        
        foreach (var billing in billingEntities)
        {
            var patientName = billing.Patient?.Email ?? billing.Patient?.UserName ?? "Unknown Patient";
            var insuranceAmount = billing.InsuranceAmount?.ToString("F2") ?? "0.00";
            var notes = billing.Notes?.Replace("\"", "\"\"") ?? "";
            
            csvContent += $"\"{patientName}\",\"{billing.ServiceType}\",\"{billing.ServiceDate:yyyy-MM-dd}\",\"{billing.Amount:F2}\",\"{insuranceAmount}\",\"{billing.PatientResponsibility:F2}\",\"{billing.PaymentStatus}\",\"{billing.DueDate:yyyy-MM-dd}\",\"{notes}\"\n";
        }

        var fileName = $"BillingData_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
        
        return File(bytes, "text/csv", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> GetBillingStats()
    {
        try
        {
            var totalBillings = await _context.Billings.CountAsync();
            var totalAmount = await _context.Billings.SumAsync(b => b.Amount);
            var pendingAmount = await _context.Billings
                .Where(b => b.PaymentStatus == "Pending")
                .SumAsync(b => b.PatientResponsibility);
            var overdueAmount = await _context.Billings
                .Where(b => b.IsOverdue)
                .SumAsync(b => b.PatientResponsibility);
            var paidAmount = await _context.Billings
                .Where(b => b.PaymentStatus == "Paid")
                .SumAsync(b => b.Amount);

            var stats = new
            {
                TotalBillings = totalBillings,
                TotalAmount = totalAmount,
                PendingAmount = pendingAmount,
                OverdueAmount = overdueAmount,
                PaidAmount = paidAmount,
                CollectionRate = totalAmount > 0 ? (paidAmount / totalAmount * 100) : 0
            };

            return Json(stats);
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message });
        }
    }
} 