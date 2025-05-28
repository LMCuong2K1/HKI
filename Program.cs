using System;
using System.Threading.Tasks;
using ConsoleApp1.Services;
using System.Collections.Generic;

namespace ConsoleApp1
{
    class Program
    {
        static UserService userService = new UserService();

        static BookingService bookingService = new BookingService();
        static InventoryService inventoryService = new InventoryService();
        static OrderProcessingService orderService = new OrderProcessingService();
        static RecruitmentManagementService recruitmentService = new RecruitmentManagementService();

        static async Task Main(string[] args)
        {
            Console.WriteLine("****************************************");
            Console.WriteLine("* CHAO MUNG DEN VOI HE THONG QUAN LY TIEM BARBER *");
            Console.WriteLine("****************************************");
            bool exit = false;
            while (!exit)
            {
                Console.WriteLine();
                Console.WriteLine("Vui long lua chon:");
                Console.WriteLine("1. Dang nhap");
                Console.WriteLine("2. Dang ky (cho Khach hang moi / Ung vien)");
                // Removed "3. Xem cac vi tri tuyen dung" from the first menu as requested
                Console.WriteLine("0. Thoat");
                Console.Write("Nhap lua chon cua ban: ");
                string input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        await LoginScreen();
                        break;
                    case "2":
                        await RegisterUser();
                        break;
                    case "0":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Lua chon khong hop le. Vui long thu lai.");
                        break;
                }
            }
        }

        static async Task ManageUsersByRole(Entities.UserRole role)
        {
            bool exit = false;
            while (!exit)
            {
                Console.WriteLine();
                Console.WriteLine("------------------------------------------------");
                Console.WriteLine($"|  QUAN LY TAI KHOAN {role.ToString().ToUpper()}  |");
                Console.WriteLine("------------------------------------------------");
                Console.WriteLine("1. Xem danh sach");
                Console.WriteLine("2. Cap nhat thong tin");
                Console.WriteLine("3. Xoa tai khoan");
                Console.WriteLine("0. Quay lai Menu Admin");
                Console.Write("Nhap lua chon cua ban: ");
                string input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        ViewUsersByRole(role);
                        break;
                    case "2":
                        await UpdateUserByRole(role);
                        break;
                    case "3":
                        await DeleteUserByRole(role);
                        break;
                    case "0":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Lua chon khong hop le. Vui long thu lai.");
                        break;
                }
            }
        }

        static void ViewUsersByRole(Entities.UserRole role)
        {
            var users = userService.GetAllUsers().FindAll(u => u.Role == role);
            if (users.Count == 0)
            {
                Console.WriteLine($"Khong co tai khoan {role.ToString()} nao.");
                return;
            }
            Console.WriteLine($"Danh sach tai khoan {role.ToString()}:");
            Console.WriteLine("ID | Ho va Ten | Email | Dien thoai");
            foreach (var user in users)
            {
                Console.WriteLine($"{user.UserID} | {user.FullName} | {user.Email} | {user.Phone}");
            }
        }

        static async Task UpdateUserByRole(Entities.UserRole role)
        {
            var users = userService.GetAllUsers().FindAll(u => u.Role == role);
            if (users.Count == 0)
            {
                Console.WriteLine($"Khong co tai khoan {role.ToString()} nao de cap nhat.");
                return;
            }
            Console.WriteLine($"Danh sach tai khoan {role.ToString()}:");
            for (int i = 0; i < users.Count; i++)
            {
                var user = users[i];
                Console.WriteLine($"{i + 1}. {user.FullName} - {user.Email} - {user.Phone}");
            }
            Console.Write("Chon tai khoan de cap nhat (nhap so): ");
            if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > users.Count)
            {
                Console.WriteLine("Lua chon khong hop le.");
                return;
            }
            var selectedUser = users[choice - 1];
            Console.WriteLine("Nhap thong tin moi (de trong neu khong muon thay doi):");
            Console.Write($"Ho va Ten ({selectedUser.FullName}): ");
            string newFullName = Console.ReadLine();
            Console.Write($"Email ({selectedUser.Email}): ");
            string newEmail = Console.ReadLine();
            Console.Write($"Dien thoai ({selectedUser.Phone}): ");
            string newPhone = Console.ReadLine();
            Console.Write("Mat khau (de trong neu khong thay doi): ");
            string newPassword = Console.ReadLine();

            if (!string.IsNullOrEmpty(newFullName))
            {
                selectedUser.FullName = newFullName;
            }
            if (!string.IsNullOrEmpty(newEmail))
            {
                selectedUser.Email = newEmail;
            }
            if (!string.IsNullOrEmpty(newPhone))
            {
                selectedUser.Phone = newPhone;
            }
            if (!string.IsNullOrEmpty(newPassword))
            {
                // Do not set PasswordHash directly; pass newPassword to UpdateUserDetails
            }
            bool success = userService.UpdateUserDetails(selectedUser, string.IsNullOrEmpty(newPassword) ? null : newPassword);
            Console.WriteLine(success ? "Cap nhat thanh cong." : "Cap nhat that bai.");
            await Task.CompletedTask;
        }

        static async Task DeleteUserByRole(Entities.UserRole role)
        {
            var users = userService.GetAllUsers().FindAll(u => u.Role == role);
            if (users.Count == 0)
            {
                Console.WriteLine($"Khong co tai khoan {role.ToString()} nao de xoa.");
                return;
            }
            Console.WriteLine($"Danh sach tai khoan {role.ToString()}:");
            for (int i = 0; i < users.Count; i++)
            {
                var user = users[i];
                Console.WriteLine($"{i + 1}. {user.FullName} - {user.Email} - {user.Phone}");
            }
            Console.Write("Chon tai khoan de xoa (nhap so): ");
            if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > users.Count)
            {
                Console.WriteLine("Lua chon khong hop le.");
                return;
            }
            var selectedUser = users[choice - 1];
            Console.WriteLine($"Ban co chac chan muon xoa tai khoan cua {selectedUser.FullName} (Email: {selectedUser.Email})? (Y/N)");
            string confirm = Console.ReadLine();
            if (confirm?.ToUpper() == "Y")
            {
                bool success = userService.DeleteUser(selectedUser.UserID);
                Console.WriteLine(success ? "Xoa tai khoan thanh cong." : "Xoa tai khoan that bai.");
            }
            else
            {
                Console.WriteLine("Huy xoa tai khoan.");
            }
            await Task.CompletedTask;
        }

        static async Task UpdateOrderStatusAdmin()
        {
            var orders = orderService.GetAllOrders();
            if (orders.Count == 0)
            {
                Console.WriteLine("Khong co don hang nao de cap nhat.");
                return;
            }
            Console.WriteLine("Danh sach don hang:");
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine("STT | OrderID                             | CustomerID                        | OrderDate           | TotalAmount | Status    ");
            Console.WriteLine("--------------------------------------------------------------------------------");
            for (int i = 0; i < orders.Count; i++)
            {
                var order = orders[i];
                Console.WriteLine($"{i + 1,-4}| {order.OrderID,-35} | {order.CustomerID,-30} | {order.OrderDate,-19} | {order.TotalAmount,11} | {order.Status}");
            }
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.Write("Chon don hang de cap nhat (nhap so): ");
            if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > orders.Count)
            {
                Console.WriteLine("Lua chon khong hop le.");
                return;
            }
            var selectedOrder = orders[choice - 1];
            Console.WriteLine("Chon trang thai moi:");
            Console.WriteLine("1. PENDING");
            Console.WriteLine("2. PAID");
            Console.WriteLine("3. SHIPPED");
            Console.WriteLine("4. CANCELLED");
            Console.Write("Nhap lua chon: ");
            string statusChoice = Console.ReadLine();
            Entities.OrderStatus newStatus;
            switch (statusChoice)
            {
                case "1":
                    newStatus = Entities.OrderStatus.PENDING;
                    break;
                case "2":
                    newStatus = Entities.OrderStatus.PAID;
                    break;
                case "3":
                    newStatus = Entities.OrderStatus.SHIPPED;
                    break;
                case "4":
                    newStatus = Entities.OrderStatus.CANCELLED;
                    break;
                default:
                    Console.WriteLine("Lua chon khong hop le.");
                    return;
            }
            bool success = orderService.UpdateOrderStatus(selectedOrder.OrderID, newStatus);
            if (success)
            {
                Console.WriteLine("Cap nhat trang thai don hang thanh cong.");
            }
            else
            {
                Console.WriteLine("Cap nhat trang thai don hang that bai.");
            }
            await Task.CompletedTask;
        }

        static async Task ViewMyOrders(string customerId)
        {
            var orders = orderService.GetAllOrders().FindAll(o => o.CustomerID == customerId);
            if (orders.Count == 0)
            {
                Console.WriteLine("Khong co don hang nao da dat.");
                Console.WriteLine("Nhan phim bat ky de quay lai...");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("Danh sach don hang da dat:");
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine("STT | OrderID                             | OrderDate           | TotalAmount | Status    ");
            Console.WriteLine("--------------------------------------------------------------------------------");
            for (int i = 0; i < orders.Count; i++)
            {
                var order = orders[i];
                Console.WriteLine($"{i + 1,-4}| {order.OrderID,-35} | {order.OrderDate,-19} | {order.TotalAmount,11} | {order.Status}");
            }
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine("Nhan phim bat ky de quay lai...");
            Console.ReadKey();
            await Task.CompletedTask;
        }

        static async Task ViewServices()
        {
            var services = inventoryService.GetAllServices();
            if (services.Count == 0)
            {
                Console.WriteLine("Khong co dich vu nao.");
                Console.WriteLine("Nhan phim bat ky de quay lai...");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("Danh sach dich vu:");
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("ID | Ten dich vu         | Gia (VND) | Thoi gian (phut)");
            Console.WriteLine("---|---------------------|-----------|------------------");
            foreach (var s in services)
            {
                Console.WriteLine($"{s.ServiceID}  | {s.ServiceName,-20} | {s.Price,9} | {s.Duration.TotalMinutes,16}");
            }
            Console.WriteLine();
            Console.WriteLine("Nhan phim bat ky de quay lai...");
            Console.ReadKey();
            await Task.CompletedTask;
        }

        static async Task ViewProducts()
        {
            var products = inventoryService.GetAllProducts();
            if (products.Count == 0)
            {
                Console.WriteLine("Khong co san pham nao.");
                Console.WriteLine("Nhan phim bat ky de quay lai...");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("Danh sach san pham:");
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("ID | Ten san pham           | Gia       | So luong");
            Console.WriteLine("---|------------------------|-----------|---------");
            foreach (var p in products)
            {
                Console.WriteLine($"{p.ProductID} | {p.ProductName,-22} | {p.Price,-9} | {p.QuantityInStock}");
            }
            Console.WriteLine();
            Console.WriteLine("Nhan phim bat ky de quay lai...");
            Console.ReadKey();
            await Task.CompletedTask;
        }

        static async Task ApproveCandidateApplications()
        {
            Console.Clear();
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("|               DUYET HO SO UNG VIEN             |");
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine("Danh sach ho so cho duyet:");
            Console.WriteLine("ID | Ung Vien           | Vi Tri Ung Tuyen | Ngay Nop   | Trang Thai");
            Console.WriteLine("---|--------------------|------------------|------------|------------");

            // Fetch candidates with status "Cho duyet" (Pending approval)
            var candidates = GetCandidatesPendingApproval();

            for (int i = 0; i < candidates.Count; i++)
            {
                var c = candidates[i];
                // Display sequential index as ID instead of actual ApplicationID
                Console.WriteLine($"{i + 1,-2} | {c.CandidateName,-18} | {c.Position,-16} | {c.SubmitDate:yyyy-MM-dd} | {c.Status}");
            }

            Console.WriteLine("...");
            Console.WriteLine();
            Console.Write("Nhap ID ho so de xem chi tiet (hoac 0 de quay lai): ");
            string input = Console.ReadLine();

            if (int.TryParse(input, out int selectedId))
            {
                if (selectedId == 0)
                {
                    // Go back or exit
                    return;
                }

                if (selectedId >= 1 && selectedId <= candidates.Count)
                {
                    var candidate = candidates[selectedId - 1];
                    ShowCandidateDetails(candidate);
                }
                else
                {
                    Console.WriteLine("ID ho so khong hop le. Nhan phim bat ky de tiep tuc...");
                    Console.ReadKey();
                    await ApproveCandidateApplications();
                }
            }
            else
            {
                Console.WriteLine("Vui long nhap mot so hop le. Nhan phim bat ky de tiep tuc...");
                Console.ReadKey();
                await ApproveCandidateApplications();
            }
        }

        // Candidate DTO for display
        class CandidateProfile
        {
            public string ID { get; set; }
            public string CandidateName { get; set; }
            public string Position { get; set; }
            public DateTime SubmitDate { get; set; }
            public string Status { get; set; }
        }

        static List<CandidateProfile> GetCandidatesPendingApproval()
        {
            // Fetch candidates with status "RECEIVED" from the database via recruitmentService
            var applications = recruitmentService.GetApplicationsByStatus("RECEIVED");
            var list = new List<CandidateProfile>();

            foreach (var app in applications)
            {
                var candidateUser = userService.GetUserById(app.CandidateID);
                var jobPosting = recruitmentService.GetJobPostingById(app.JobPostingID);

                if (candidateUser != null && jobPosting != null)
                {
                    list.Add(new CandidateProfile
                    {
                        ID = app.ApplicationID,
                        CandidateName = candidateUser.FullName,
                        Position = jobPosting.Title,
                        SubmitDate = app.SubmissionDate,
                        Status = app.Status.ToString()
                    });
                }
            }

            return list;
        }

        static void ShowCandidateDetails(CandidateProfile candidate)
        {
                Console.Clear();
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine("|               CHI TIET HO SO UNG VIEN          |");
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine();

                // Fetch full candidate application details
                var application = recruitmentService.GetApplicationById(candidate.ID);
                if (application == null)
                {
                    Console.WriteLine("Khong tim thay ho so ung vien.");
                    Console.WriteLine("Nhan phim bat ky de quay lai...");
                    Console.ReadKey();
                    return;
                }

                var candidateUser = userService.GetUserById(application.CandidateID);
                var jobPosting = recruitmentService.GetJobPostingById(application.JobPostingID);

                Console.WriteLine($"Ma ho so: {application.ApplicationID}");
                Console.WriteLine($"Ung vien: {candidateUser?.FullName ?? "N/A"} - Email: {candidateUser?.Email ?? "N/A"} - SDT: {candidateUser?.Phone ?? "N/A"}");
                Console.WriteLine($"Vi tri ung tuyen: {jobPosting?.Title ?? "N/A"} (Ma tin: {jobPosting?.JobPostingID ?? "N/A"})");
                Console.WriteLine($"Ngay nop: {application.SubmissionDate:yyyy-MM-dd}");
                Console.WriteLine($"Duong dan CV: {application.ResumeLink}");
                Console.WriteLine("Thu gioi thieu:");
                Console.WriteLine(application.CoverLetter);
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine($"Trang thai hien tai: {application.Status}");
                Console.WriteLine();

                bool exit = false;
                while (!exit)
                {
                    Console.WriteLine("Lua chon hanh dong:");
                    Console.WriteLine("1. Chap nhan (Moi phong van)");
                    Console.WriteLine("2. Tu choi");
                    Console.WriteLine("3. Xem xet sau (Giu nguyen trang thai)");
                    Console.WriteLine("0. Quay lai danh sach");
                    Console.Write("Nhap lua chon cua ban: ");
                    string actionInput = Console.ReadLine();

                            switch (actionInput)
                            {
                                case "1":
                                    Console.Write("Ghi chu (neu can): ");
                                    string acceptNote = Console.ReadLine();
                                    application.Status = Entities.ApplicationStatus.INTERVIEW_SCHEDULED;
                                    application.ReviewNotes = acceptNote;
if (recruitmentService.UpdateApplicationStatus(application.ApplicationID, Entities.ApplicationStatus.INTERVIEW_SCHEDULED, acceptNote))
{
    // Update user role to Barber
    var user = userService.GetUserById(application.CandidateID);
if (user != null)
{
    bool userUpdated = userService.UpdateUserRole(user.UserID, Entities.UserRole.BARBER);
    if (userUpdated)
    {
        // The insertion into Barbers table is handled inside UpdateUserRole, so no need to insert here.
    }
    else
    {
        Console.WriteLine("Cap nhat vai tro nguoi dung that bai.");
    }
}
    Console.WriteLine("Da chap nhan ho so. Moi phong van.");
}
                                    else
                                    {
                                        Console.WriteLine("Cap nhat trang thai that bai.");
                                    }
                                    exit = true;
                                    break;
                                case "2":
                                    Console.Write("Ghi chu (neu can): ");
                                    string rejectNote = Console.ReadLine();
                                    application.Status = Entities.ApplicationStatus.REJECTED;
                                    application.ReviewNotes = rejectNote;
                                    if (recruitmentService.UpdateApplicationStatus(application.ApplicationID, Entities.ApplicationStatus.REJECTED, rejectNote))
                                    {
                                        Console.WriteLine("Da tu choi ho so.");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Cap nhat trang thai that bai.");
                                    }
                                    exit = true;
                                    break;
                        case "3":
                            Console.WriteLine("Giữ nguyên trạng thái, xem xét sau.");
                            exit = true;
                            break;
                        case "0":
                            exit = true;
                            break;
                        default:
                            Console.WriteLine("Lua chon khong hop le. Vui long thu lai.");
                            break;
                    }
                }

                if (!exit)
                {
                    Console.WriteLine("Nhan phim bat ky de tiep tuc...");
                    Console.ReadKey();
                }

                ApproveCandidateApplications().Wait();
        }

            static async Task BookAppointment(string customerId)
            {
                Console.WriteLine("------------------------------------------------");
                Console.WriteLine("|                 DAT LICH HEN MOI              |");
                Console.WriteLine("------------------------------------------------");

                // Use the passed customerId instead of asking for input

                // Step 1: Choose Service
                Console.WriteLine();
                Console.WriteLine("--- Buoc 1: Chon Dich Vu ---");
                var services = inventoryService.GetAllServices();
                if (services.Count == 0)
                {
                    Console.WriteLine("Khong co dich vu nao hien tai.");
                    return;
                }
                Console.WriteLine("Danh sach dich vu:");
                for (int i = 0; i < services.Count; i++)
                {
                    var svc = services[i];
                    Console.WriteLine($"{i + 1}. {svc.ServiceName} - Gia: {svc.Price} - Thoi luong: {svc.Duration}");
                }
                Console.Write("Chon dich vu (nhap so): ");
                if (!int.TryParse(Console.ReadLine(), out int serviceChoice) || serviceChoice < 1 || serviceChoice > services.Count)
                {
                    Console.WriteLine("Lua chon khong hop le.");
                    return;
                }
                var selectedService = services[serviceChoice - 1];

                // Step 2: Choose Barber (optional)
                Console.WriteLine();
                Console.WriteLine("--- Buoc 2: Chon Barber (Neu co) ---");
                Console.Write("Co muon chon Barber cu the khong? (C/K): ");
                string chooseBarberInput = Console.ReadLine()?.Trim().ToUpper();
                List<Entities.User> barbers = new List<Entities.User>();
                Entities.User selectedBarber = null;
                if (chooseBarberInput == "C")
                {
                    barbers = bookingService.GetAvailableBarbers();
                    if (barbers.Count == 0)
                    {
                        Console.WriteLine("Khong co barber nao hien tai.");
                        return;
                    }
                    Console.WriteLine("Danh sach Barber:");
                    for (int i = 0; i < barbers.Count; i++)
                    {
                        var barber = barbers[i] as ConsoleApp1.Entities.Barber;
                        if (barber != null)
                        {
                            Console.WriteLine($"{i + 1}. {barber.FullName} - Chuyen mon: {barber.Specialty}");
                        }
                        else
                        {
                            Console.WriteLine($"{i + 1}. {barbers[i].FullName} - Chuyen mon: N/A");
                        }
                    }
                    Console.Write("Chon Barber (nhap so, hoac bo trong de he thong tu chon): ");
                    string barberChoiceInput = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(barberChoiceInput))
                    {
                        selectedBarber = null; // System will choose barber later
                    }
                    else if (int.TryParse(barberChoiceInput, out int barberChoice) && barberChoice >= 1 && barberChoice <= barbers.Count)
                    {
                        selectedBarber = barbers[barberChoice - 1];
                    }
                    else
                    {
                        Console.WriteLine("Lua chon barber khong hop le.");
                        return;
                    }
                }

                // Step 3: Choose Appointment Date
                Console.WriteLine();
                Console.WriteLine("--- Buoc 3: Chon Ngay Hen ---");
                Console.Write("Nhap ngay muon dat (YYYY-MM-DD): ");
                if (!DateTime.TryParse(Console.ReadLine(), out DateTime appointmentDate))
                {
                    Console.WriteLine("Dinh dang ngay khong hop le.");
                    return;
                }

                // If barber not selected, system chooses one with available slots
                if (selectedBarber == null)
                {
                    var availableBarbers = bookingService.GetAvailableBarbers();
                    foreach (var barber in availableBarbers)
                    {
                        var slots = bookingService.GetAvailableSlots(barber.UserID, appointmentDate);
                        if (slots.Count > 0)
                        {
                            selectedBarber = barber;
                            break;
                        }
                    }
                    if (selectedBarber == null)
                    {
                        Console.WriteLine("Khong co barber nao co lich trong ngay chon.");
                        return;
                    }
                }

                // Step 4: Choose Appointment Time Slot
                Console.WriteLine();

                // Ensure availability slots exist for barber and date
                var availableSlots = bookingService.GetAvailableSlots(selectedBarber.UserID, appointmentDate);
                if (availableSlots.Count == 0)
                {
                    bool slotsCreated = bookingService.CreateAvailabilitySlots(selectedBarber.UserID, appointmentDate);
                    if (!slotsCreated)
                    {
                        Console.WriteLine("Khong the tao cac khung gio trong cho. Khong the tiep tuc dat lich.");
                        return;
                    }
                    availableSlots = bookingService.GetAvailableSlots(selectedBarber.UserID, appointmentDate);
                }

                string barberNameDisplay = selectedBarber != null ? (selectedBarber.FullName ?? "N/A") : "N/A";
                Console.WriteLine($"Cac khung gio con trong cho {appointmentDate.ToString("yyyy-MM-dd")} (voi {selectedService.ServiceName} va {barberNameDisplay}):");

                if (availableSlots.Count == 0)
                {
                    Console.WriteLine("Khong co slot trong cho trong ngay chon.");
                    return;
                }

                for (int i = 0; i < availableSlots.Count; i++)
                {
                    var slot = availableSlots[i];
                    Console.WriteLine($"{i + 1}. {slot.StartTime} - {slot.EndTime}");
                }
                Console.Write("Chon khung gio (nhap so): ");
                if (!int.TryParse(Console.ReadLine(), out int slotChoice) || slotChoice < 1 || slotChoice > availableSlots.Count)
                {
                    Console.WriteLine("Lua chon khong hop le.");
                    return;
                }
                var selectedSlot = availableSlots[slotChoice - 1];

                var appointment = new Entities.Appointment
                {
                    SlotID = selectedSlot.SlotID,  // Added SlotID assignment
                    CustomerID = customerId,
                    BarberID = selectedBarber.UserID,
                    ServiceID = selectedService.ServiceID,
                    AppointmentDateTime = appointmentDate.Date + selectedSlot.StartTime,
                    Status = Entities.AppointmentStatus.SCHEDULED,
                    Notes = "",
                    Duration = selectedSlot.EndTime - selectedSlot.StartTime,
                    BookingDate = DateTime.Now
                };

                // Step 5: Confirm Booking Information
                Console.WriteLine();
                Console.WriteLine("--- Buoc 5: Xac Nhan Thong Tin ---");
                Console.WriteLine("Xac nhan dat lich:");
                Console.WriteLine($"Dich vu: {selectedService.ServiceName}");
                string barberNameDisplayConfirm = selectedBarber != null ? (selectedBarber.FullName ?? "He thong tu chon") : "He thong tu chon";
                Console.WriteLine($"Barber: {barberNameDisplayConfirm}");
                Console.WriteLine($"Thoi gian: {selectedSlot.StartTime} - {appointmentDate.ToString("yyyy-MM-dd")}");
                Console.WriteLine($"Gia: {selectedService.Price}");
                Console.WriteLine();
                Console.Write("Ghi chu (neu co): ");
                string notes = Console.ReadLine();

                Console.WriteLine();
                Console.WriteLine("[1. Xac nhan dat lich] [0. Huy]");
                Console.Write("Nhap Lua chon cua ban: ");
                string confirmChoice = Console.ReadLine();

                if (confirmChoice == "1")
                {
                    var appointmentConfirmed = new Entities.Appointment
                    {
                        SlotID = selectedSlot.SlotID,  // Added SlotID assignment
                        CustomerID = customerId,
                        BarberID = selectedBarber.UserID,
                        ServiceID = selectedService.ServiceID,
                        AppointmentDateTime = appointmentDate.Date + selectedSlot.StartTime,
                        Status = Entities.AppointmentStatus.SCHEDULED,
                        Notes = notes,
                        Duration = selectedSlot.EndTime - selectedSlot.StartTime,
                        BookingDate = DateTime.Now
                    };

                    bool success = bookingService.CreateNewAppointment(appointmentConfirmed);
                    if (success)
                    {
                        Console.WriteLine("Dat lich hen thanh cong.");
                    }
                    else
                    {
                        Console.WriteLine("Dat lich hen that bai. Slot co the da bi dat.");
                    }
                }
                else
                {
                    Console.WriteLine("Huy dat lich.");
                }
            }

            static async Task ViewMyAppointments()
            {
                Console.WriteLine("View My Appointments");

                Console.Write("Enter your Customer ID: ");
                string customerId = Console.ReadLine();

                var appointments = GetAppointmentsByCustomer(customerId);
                if (appointments.Count == 0)
                {
                    Console.WriteLine("No appointments found.");
                    return;
                }

                Console.WriteLine("Your Appointments:");
                foreach (var appt in appointments)
                {
                    Console.WriteLine($"ID: {appt.AppointmentID}, BarberID: {appt.BarberID}, ServiceID: {appt.ServiceID}, DateTime: {appt.AppointmentDateTime}, Status: {appt.Status}");
                }
            }

            static List<Entities.Appointment> GetAppointmentsByCustomer(string customerId)
            {
                var appointments = new List<Entities.Appointment>();
                using (var connection = new MySql.Data.MySqlClient.MySqlConnection(DataAccess.DatabaseConfig.ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM Appointments WHERE CustomerID = @CustomerID";
                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@CustomerID", customerId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var appt = new Entities.Appointment
                                {
                                    AppointmentID = reader["AppointmentID"].ToString(),
                                    CustomerID = reader["CustomerID"].ToString(),
                                    BarberID = reader["BarberID"].ToString(),
                                    ServiceID = reader["ServiceID"].ToString(),
                                    AppointmentDateTime = Convert.ToDateTime(reader["AppointmentDateTime"]),
                                    Status = Enum.TryParse(reader["Status"].ToString(), out Entities.AppointmentStatus status) ? status : Entities.AppointmentStatus.SCHEDULED,
                                    Notes = reader["Notes"].ToString(),
                                    Duration = TimeSpan.FromMinutes(Convert.ToDouble(reader["DurationMinutes"])),
                                    BookingDate = Convert.ToDateTime(reader["BookingDate"])
                                };
                                appointments.Add(appt);
                            }
                        }
                    }
                }
                return appointments;
            }

            static async Task RegisterUser()
            {
                Console.WriteLine("DANG KY");
                bool exit = false;
                while (!exit)
                {
                    Console.WriteLine("Vui long chon loai tai khoan:");
                    Console.WriteLine("1. Khach hang");
                    Console.WriteLine("2. Ung vien");
                    Console.WriteLine("0. Quay lai");
                    Console.Write("Nhap lua chon cua ban: ");
                    string choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            await RegisterUserByRole(Entities.UserRole.CUSTOMER);
                            exit = true;
                            break;
                        case "2":
                            await RegisterUserByRole(Entities.UserRole.CANDIDATE);
                            exit = true;
                            break;
                        case "0":
                            exit = true;
                            break;
                        default:
                            Console.WriteLine("Lua chon khong hop le. Vui long thu lai.");
                            break;
                    }
                }
            }

            static async Task RegisterUserByRole(Entities.UserRole role)
            {
                Console.WriteLine("User Registration");
                Console.Write("Full Name: ");
                string fullName = Console.ReadLine();
                Console.Write("Email: ");
                string email = Console.ReadLine();

                string phone;
                while (true)
                {
                    Console.Write("Phone (10 digits): ");
                    phone = Console.ReadLine();
                    if (phone != null && phone.Length == 10 && long.TryParse(phone, out _))
                    {
                        break;
                    }
                    Console.WriteLine("Invalid phone number. Please enter exactly 10 digits.");
                }

                Console.Write("Password: ");
                string password = Console.ReadLine();

                var newUser = new Entities.User
                {
                    FullName = fullName,
                    Email = email,
                    Phone = phone,
                    PasswordHash = password, // Note: In production, hash the password
                    Role = role
                };

                bool success = userService.RegisterUser(newUser);
                if (success)
                {
                    Console.WriteLine("Registration successful.");
                    if (role == Entities.UserRole.CUSTOMER)
                    {
                        await CustomerLoggedInMenu(fullName, newUser.UserID);
                    }
else if (role == Entities.UserRole.CANDIDATE)
{
    await CandidateMenu(newUser.FullName, newUser.UserID);
}
                }
                else
                {
                    Console.WriteLine("Registration failed. Email may already exist.");
                }
            }

            static async Task AddAdminUser()
            {
                Console.WriteLine("Add Admin User");
                Console.Write("Full Name: ");
                string fullName = Console.ReadLine();
                Console.Write("Email: ");
                string email = Console.ReadLine();

                string phone;
                while (true)
                {
                    Console.Write("Phone (10 digits): ");
                    phone = Console.ReadLine();
                    if (phone != null && phone.Length == 10 && long.TryParse(phone, out _))
                    {
                        break;
                    }
                    Console.WriteLine("Invalid phone number. Please enter exactly 10 digits.");
                }

                Console.Write("Password: ");
                string password = Console.ReadLine();

                var newAdmin = new Entities.User
                {
                    FullName = fullName,
                    Email = email,
                    Phone = phone,
                    PasswordHash = password, // Note: In production, hash the password
                    Role = Entities.UserRole.ADMIN
                };

                bool success = userService.RegisterUser(newAdmin);
                if (success)
                {
                    // Insert into Admins table to satisfy foreign key constraint
                    using (var connection = new MySql.Data.MySqlClient.MySqlConnection(DataAccess.DatabaseConfig.ConnectionString))
                    {
                        connection.Open();
                        string insertAdminQuery = "INSERT INTO Admins (UserID) VALUES (@UserID)";
                        using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(insertAdminQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@UserID", newAdmin.UserID);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    Console.WriteLine("Admin user added successfully.");
                }
                else
                {
                    Console.WriteLine("Failed to add admin user. Email may already exist.");
                }
            }

            static async Task LoginUser()
            {
                Console.WriteLine("User Login");
                Console.Write("Email: ");
                string email = Console.ReadLine();
                Console.Write("Password: ");
                string password = Console.ReadLine();

                var user = userService.AuthenticateUser(email, password);
                if (user != null)
                {
                    Console.WriteLine($"Login successful. Welcome, {user.FullName}!");
                }
                else
                {
                    Console.WriteLine("Login failed. Invalid email or password.");
                }
            }

            static async Task ManageJobPostings(string adminUserId)
            {
                Console.WriteLine("Manage Job Postings");
                bool exit = false;
                while (!exit)
                {
                    Console.WriteLine("1. List Job Postings");
                    Console.WriteLine("2. Add Job Posting");
                    Console.WriteLine("3. Edit Job Posting");
                    Console.WriteLine("4. Delete Job Posting");
                    Console.WriteLine("0. Back to Admin Menu");
                    Console.Write("Select an option: ");
                    string input = Console.ReadLine();

                    switch (input)
                    {
                        case "1":
                            var postings = recruitmentService.GetAllJobPostings();
                            foreach (var p in postings)
                            {
                                Console.WriteLine($"ID: {p.JobPostingID}, Title: {p.Title}, Status: {p.Status}, Location: {p.Location}");
                            }
                            break;
                        case "2":
                            Console.Write("Title: ");
                            string title = Console.ReadLine();
                            Console.Write("Description: ");
                            string description = Console.ReadLine();
                            Console.Write("Requirements: ");
                            string requirements = Console.ReadLine();
                            Console.Write("Location: ");
                            string location = Console.ReadLine();
                            Console.Write("Salary Range: ");
                            string salaryRange = Console.ReadLine();
                            Console.Write("Closing Date (yyyy-MM-dd, optional): ");
                            string closingDateStr = Console.ReadLine();
                            DateTime? closingDate = null;
                            if (DateTime.TryParse(closingDateStr, out DateTime cd))
                            {
                                closingDate = cd;
                            }

                            var newPosting = new Entities.JobPosting
                            {
                                Title = title,
                                Description = description,
                                Requirements = requirements,
                                Location = location,
                                SalaryRange = salaryRange,
                                ClosingDate = closingDate,
                                Status = Entities.JobPostingStatus.OPEN,
                                PostedByAdminID = adminUserId
                            };
                            bool added = recruitmentService.AddJobPosting(newPosting);
                            Console.WriteLine(added ? "Job posting added successfully." : "Failed to add job posting.");
                            break;
                        case "3":
                            Console.Write("Enter Job Posting ID to edit: ");
                            string editId = Console.ReadLine();
                            var jobPostings = recruitmentService.GetAllJobPostings();
                            var jobToEdit = jobPostings.Find(jp => jp.JobPostingID == editId);
                            if (jobToEdit == null)
                            {
                                Console.WriteLine("Job posting not found.");
                                break;
                            }
                            Console.WriteLine($"Editing Job Posting: {jobToEdit.Title}");
                            Console.Write("New Title (leave blank to keep current): ");
                            string newTitle = Console.ReadLine();
                            if (!string.IsNullOrEmpty(newTitle)) jobToEdit.Title = newTitle;
                            Console.Write("New Description (leave blank to keep current): ");
                            string newDescription = Console.ReadLine();
                            if (!string.IsNullOrEmpty(newDescription)) jobToEdit.Description = newDescription;
                            Console.Write("New Requirements (leave blank to keep current): ");
                            string newRequirements = Console.ReadLine();
                            if (!string.IsNullOrEmpty(newRequirements)) jobToEdit.Requirements = newRequirements;
                            Console.Write("New Location (leave blank to keep current): ");
                            string newLocation = Console.ReadLine();
                            if (!string.IsNullOrEmpty(newLocation)) jobToEdit.Location = newLocation;
                            Console.Write("New Salary Range (leave blank to keep current): ");
                            string newSalaryRange = Console.ReadLine();
                            if (!string.IsNullOrEmpty(newSalaryRange)) jobToEdit.SalaryRange = newSalaryRange;
                            Console.Write("New Closing Date (yyyy-MM-dd, leave blank to keep current): ");
                            string newClosingDateStr = Console.ReadLine();
                            if (!string.IsNullOrEmpty(newClosingDateStr) && DateTime.TryParse(newClosingDateStr, out DateTime newCd))
                            {
                                jobToEdit.ClosingDate = newCd;
                            }
                            Console.Write("New Status (OPEN/CLOSED, leave blank to keep current): ");
                            string newStatusStr = Console.ReadLine();
                            if (!string.IsNullOrEmpty(newStatusStr) && Enum.TryParse(newStatusStr, out Entities.JobPostingStatus newStatus))
                            {
                                jobToEdit.Status = newStatus;
                            }
                            bool updated = recruitmentService.UpdateJobPosting(jobToEdit);
                            Console.WriteLine(updated ? "Job posting updated successfully." : "Failed to update job posting.");
                            break;
                        case "4":
                            Console.Write("Enter Job Posting ID to delete: ");
                            string deleteId = Console.ReadLine();
                            bool deleted = recruitmentService.DeleteJobPosting(deleteId);
                            Console.WriteLine(deleted ? "Job posting deleted successfully." : "Failed to delete job posting.");
                            break;
                        case "0":
                            exit = true;
                            break;
                        default:
                            Console.WriteLine("Invalid option. Please try again.");
                            break;
                    }
                }
            }

            static void ViewUsers()
            {
                Console.WriteLine("Registered Users:");
                foreach (var user in userService.GetAllUsers())
                {
                    Console.WriteLine($"ID: {user.UserID}, Name: {user.FullName}, Email: {user.Email}, Phone: {user.Phone}, Role: {user.Role}");
                }
                Console.WriteLine("Press any key to return...");
                Console.ReadKey();
            }

            static void ViewAdmins()
            {
                var allUsers = userService.GetAllUsers();
                var admins = allUsers.FindAll(u => u.Role == Entities.UserRole.ADMIN);

                if (admins.Count == 0)
                {
                    Console.WriteLine("No admins found.");
                    return;
                }

                Console.WriteLine("Admin Users:");
                foreach (var admin in admins)
                {
                    Console.WriteLine($"ID: {admin.UserID}, Name: {admin.FullName}, Email: {admin.Email}");
                }
            }

            static async Task BarberMenu(string barberName = "", string barberId = "")
            {
                bool exit = false;
                while (!exit)
                {
                    Console.WriteLine();
                    Console.WriteLine("------------------------------------------------");
                    Console.WriteLine($"|  CHAO MUNG BARBER, {barberName}  |");
                    Console.WriteLine("------------------------------------------------");
                    Console.WriteLine("|                MENU BARBER                   |");
                    Console.WriteLine("------------------------------------------------");
                    Console.WriteLine("1. Xem lich hen duoc phan cong");
                    Console.WriteLine("2. Cap nhat trang thai lich hen (Hoan thanh, Huy)");
                    Console.WriteLine("3. Quan ly lich lam viec (Availability Slots)");
                    Console.WriteLine("4. Cap nhat thong tin ca nhan");
                    Console.WriteLine("0. Dang xuat");
                    Console.Write("Nhap lua chon cua ban: ");
                    string input = Console.ReadLine();

                    switch (input)
                    {
                        case "1":
                            await ViewAssignedAppointments(barberId);
                            break;
                        case "2":
                            await UpdateAppointmentStatus(barberId);
                            break;
                        case "3":
                            await ManageAvailabilitySlots(barberId);
                            break;
                        case "4":
                            await UpdatePersonalInfo(barberId);
                            break;
                        case "0":
                            exit = true;
                            break;
                        default:
                            Console.WriteLine("Lua chon khong hop le. Vui long thu lai.");
                            break;
                    }
                }
                await Task.CompletedTask;
            }

            static async Task ViewAssignedAppointments(string barberId)
            {
                Console.WriteLine("Danh sach lich hen duoc phan cong:");
                var appointments = new List<Entities.Appointment>();
                using (var connection = new MySql.Data.MySqlClient.MySqlConnection(DataAccess.DatabaseConfig.ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM Appointments WHERE BarberID = @BarberID AND Status = 'SCHEDULED'";
                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@BarberID", barberId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var appt = new Entities.Appointment
                                {
                                    AppointmentID = reader["AppointmentID"].ToString(),
                                    CustomerID = reader["CustomerID"].ToString(),
                                    BarberID = reader["BarberID"].ToString(),
                                    ServiceID = reader["ServiceID"].ToString(),
                                    AppointmentDateTime = Convert.ToDateTime(reader["AppointmentDateTime"]),
                                    Status = Enum.TryParse(reader["Status"].ToString(), out Entities.AppointmentStatus status) ? status : Entities.AppointmentStatus.SCHEDULED,
                                    Notes = reader["Notes"].ToString(),
                                    Duration = TimeSpan.FromMinutes(Convert.ToDouble(reader["DurationMinutes"])),
                                    BookingDate = Convert.ToDateTime(reader["BookingDate"])
                                };
                                appointments.Add(appt);
                            }
                        }
                    }
                }
                if (appointments.Count == 0)
                {
                    Console.WriteLine("Khong co lich hen nao duoc phan cong.");
                    Console.WriteLine("Press any key to return...");
                    Console.ReadKey();
                    return;
                }
                foreach (var appt in appointments)
                {
                    Console.WriteLine($"ID: {appt.AppointmentID}, CustomerID: {appt.CustomerID}, ServiceID: {appt.ServiceID}, DateTime: {appt.AppointmentDateTime}, Status: {appt.Status}");
                }
                Console.WriteLine("Press any key to return...");
                Console.ReadKey();
                await Task.CompletedTask;
            }

            static async Task UpdateAppointmentStatus(string barberId)
            {
                Console.WriteLine("Cap nhat trang thai lich hen:");
                var appointments = new List<Entities.Appointment>();
                using (var connection = new MySql.Data.MySqlClient.MySqlConnection(DataAccess.DatabaseConfig.ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM Appointments WHERE BarberID = @BarberID AND Status = 'SCHEDULED'";
                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@BarberID", barberId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var appt = new Entities.Appointment
                                {
                                    AppointmentID = reader["AppointmentID"].ToString(),
                                    CustomerID = reader["CustomerID"].ToString(),
                                    BarberID = reader["BarberID"].ToString(),
                                    ServiceID = reader["ServiceID"].ToString(),
                                    AppointmentDateTime = Convert.ToDateTime(reader["AppointmentDateTime"]),
                                    Status = Enum.TryParse(reader["Status"].ToString(), out Entities.AppointmentStatus status) ? status : Entities.AppointmentStatus.SCHEDULED,
                                    Notes = reader["Notes"].ToString(),
                                    Duration = TimeSpan.FromMinutes(Convert.ToDouble(reader["DurationMinutes"])),
                                    BookingDate = Convert.ToDateTime(reader["BookingDate"])
                                };
                                appointments.Add(appt);
                            }
                        }
                    }
                }
                if (appointments.Count == 0)
                {
                    Console.WriteLine("Khong co lich hen nao de cap nhat.");
                    return;
                }
                Console.WriteLine("Danh sach lich hen dang cho cap nhat:");
                for (int i = 0; i < appointments.Count; i++)
                {
                    var appt = appointments[i];
                    Console.WriteLine($"{i + 1}. ID: {appt.AppointmentID}, CustomerID: {appt.CustomerID}, ServiceID: {appt.ServiceID}, DateTime: {appt.AppointmentDateTime}, Status: {appt.Status}");
                }
                Console.Write("Chon lich hen de cap nhat (nhap so): ");
                if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > appointments.Count)
                {
                    Console.WriteLine("Lua chon khong hop le.");
                    return;
                }
                var selectedAppt = appointments[choice - 1];
                Console.WriteLine("Chon trang thai moi:");
                Console.WriteLine("1. Hoan thanh");
                Console.WriteLine("2. Huy");
                Console.Write("Nhap lua chon: ");
                string statusChoice = Console.ReadLine();
                Entities.AppointmentStatus newStatus;
                switch (statusChoice)
                {
                    case "1":
                        newStatus = Entities.AppointmentStatus.COMPLETED;
                        break;
                    case "2":
                        newStatus = Entities.AppointmentStatus.CANCELLED_BY_STAFF;
                        break;
                    default:
                        Console.WriteLine("Lua chon khong hop le.");
                        return;
                }
                using (var connection = new MySql.Data.MySqlClient.MySqlConnection(DataAccess.DatabaseConfig.ConnectionString))
                {
                    connection.Open();
                    string updateQuery = "UPDATE Appointments SET Status = @Status WHERE AppointmentID = @AppointmentID";
                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@Status", newStatus.ToString());
                        cmd.Parameters.AddWithValue("@AppointmentID", selectedAppt.AppointmentID);
                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            Console.WriteLine("Cap nhat trang thai lich hen thanh cong.");
                        }
                        else
                        {
                            Console.WriteLine("Cap nhat trang thai lich hen that bai.");
                        }
                    }
                }
                await Task.CompletedTask;
            }

            static async Task ManageAvailabilitySlots(string barberId)
            {
                Console.WriteLine("QUAN LY LICH LAM VIEC - CHON NGAY LAM VIEC (CHU NHAT NGHI)");
                var bookingService = new BookingService();

                // Get current working days
                var currentWorkingDays = bookingService.GetWeeklyAvailability(barberId);

                Console.WriteLine("Ngay lam viec hien tai:");
                for (int i = 1; i <= 6; i++) // Monday=1 to Saturday=6
                {
                    var day = (DayOfWeek)i;
                    Console.WriteLine($"{i}. {day} {(currentWorkingDays.Contains(day) ? "(Da chon)" : "")}");
                }
                Console.WriteLine("0. Hoan tat");

                var selectedDays = new HashSet<DayOfWeek>(currentWorkingDays);

                while (true)
                {
                    Console.Write("Chon ngay lam viec de them/bot (nhap so tu 0-6): ");
                    string input = Console.ReadLine();
                    if (int.TryParse(input, out int dayChoice))
                    {
                        if (dayChoice == 0)
                        {
                            break;
                        }
                        else if (dayChoice >= 1 && dayChoice <= 6)
                        {
                            var day = (DayOfWeek)dayChoice;
                            if (selectedDays.Contains(day))
                            {
                                selectedDays.Remove(day);
                                Console.WriteLine($"{day} da duoc bo chon.");
                            }
                            else
                            {
                                selectedDays.Add(day);
                                Console.WriteLine($"{day} da duoc chon.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Lua chon khong hop le. Vui long thu lai.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Vui long nhap so hop le.");
                    }
                }

                // Sunday is always off, so no need to add

                bool success = bookingService.UpdateWeeklyAvailability(barberId, new List<DayOfWeek>(selectedDays));
                if (success)
                {
                    Console.WriteLine("Cap nhat lich lam viec thanh cong.");

                    // Automatically create availability slots for future dates matching selected days
                    DateTime startDate = DateTime.Today;
                    DateTime endDate = startDate.AddMonths(1); // Create slots for next 1 month

                    for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                    {
                        if (selectedDays.Contains(date.DayOfWeek))
                        {
                            bool created = bookingService.CreateAvailabilitySlots(barberId, date);
                            if (created)
                            {
                                Console.WriteLine($"Da tao cac khung gio cho ngay {date.ToString("yyyy-MM-dd")}");
                            }
                            else
                            {
                                Console.WriteLine($"Khong the tao cac khung gio cho ngay {date.ToString("yyyy-MM-dd")}");
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Cap nhat lich lam viec that bai.");
                }

                await Task.CompletedTask;
            }

        static async Task UpdatePersonalInfo(string userId)
        {
            var users = userService.GetAllUsers();
            var user = users.Find(u => u.UserID == userId);
            if (user == null)
            {
                Console.WriteLine("Khong tim thay nguoi dung.");
                return;
            }
            Console.WriteLine("Cap nhat thong tin ca nhan:");
            Console.WriteLine($"Ho va Ten hien tai: {user.FullName}");
            Console.Write("Nhap ho va ten moi (de trong neu khong thay doi): ");
            string newFullName = Console.ReadLine();
            Console.WriteLine($"Email hien tai: {user.Email}");
            Console.Write("Nhap email moi (de trong neu khong thay doi): ");
            string newEmail = Console.ReadLine();
            Console.WriteLine($"Dien thoai hien tai: {user.Phone}");
            Console.Write("Nhap dien thoai moi (de trong neu khong thay doi): ");
            string newPhone = Console.ReadLine();
            Console.Write("Nhap mat khau moi (de trong neu khong thay doi): ");
            string newPassword = Console.ReadLine();

            if (!string.IsNullOrEmpty(newFullName))
            {
                user.FullName = newFullName;
            }
            if (!string.IsNullOrEmpty(newEmail))
            {
                user.Email = newEmail;
            }
            if (!string.IsNullOrEmpty(newPhone))
            {
                user.Phone = newPhone;
            }
            bool success = userService.UpdateUserDetails(user, string.IsNullOrEmpty(newPassword) ? null : newPassword);
            Console.WriteLine(success ? "Cap nhat thong tin thanh cong." : "Cap nhat thong tin that bai.");
            await Task.CompletedTask;
        }

            static async Task CustomerMenu(string customerId)
            {
                Console.WriteLine("Customer Menu");
                Console.WriteLine("1. Register");
                Console.WriteLine("2. Login");
                Console.WriteLine("3. Book Appointment");
                Console.WriteLine("4. View My Appointments");
                Console.WriteLine("0. Back to Main Menu");
                Console.Write("Select an option: ");
                string input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        await RegisterUser();
                        break;
                    case "2":
                        await LoginUser();
                        break;
                    case "3":
                        await BookAppointment(customerId);
                        break;
                    case "4":
                        await ViewMyAppointments();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
                await Task.CompletedTask;
            }

            static async Task CustomerLoggedInMenu(string customerName, string customerId)
            {
                bool exit = false;
                while (!exit)
                {
                    Console.WriteLine();
                    Console.WriteLine("------------------------------------------------");
                    Console.WriteLine($"|  CHAO MUNG KHACH HANG, {customerName}  |");
                    Console.WriteLine("------------------------------------------------");
                    Console.WriteLine("|                MENU KHACH HANG               |");
                    Console.WriteLine("------------------------------------------------");
                    Console.WriteLine("1. Dat lich hen moi (UC-01)");
                    Console.WriteLine("2. Xem lich hen cua ban (UC-05)");
                    Console.WriteLine("3. Mua san pham (UC-02)");
                    Console.WriteLine("4. Xem don hang da dat");
                    Console.WriteLine("5. Xem danh sach dich vu");
                    Console.WriteLine("6. Xem danh sach san pham");
                    Console.WriteLine("7. Cap nhat thong tin ca nhan");
                    Console.WriteLine("0. Dang xuat");
                    Console.Write("Nhap lua chon cua ban: ");
                    string input = Console.ReadLine();

                    switch (input)
                    {
                        case "1":
                            await BookAppointment(customerId);
                            break;
                        case "2":
                            await ViewMyAppointments();
                            break;
                        case "3":
                            await BuyProducts(customerId);
                            break;
                        case "4":
                            await ViewMyOrders(customerId);
                            break;
                        case "5":
                            await ViewServices();
                            break;
                        case "6":
                            await ViewProducts();
                            break;
                        case "7":
                            await UpdatePersonalInfo(customerId);
                            break;
                        case "0":
                            exit = true;
                            break;
                        default:
                            Console.WriteLine("Lua chon khong hop le. Vui long thu lai.");
                            break;
                    }
                }
            }

            static async Task AdminLoggedInMenu(string adminName, string adminUserId)
            {
                bool exit = false;
                while (!exit)
                {
                    Console.WriteLine();
                    Console.WriteLine("------------------------------------------------");
                    Console.WriteLine($"|  CHAO MUNG ADMIN, {adminName}  |");
                    Console.WriteLine("------------------------------------------------");
                    Console.WriteLine("|                MENU ADMIN                   |");
                    Console.WriteLine("------------------------------------------------");
                    Console.WriteLine("-- Quan Ly Lich Hen --");
                    Console.WriteLine("1. Xem tat ca lich hen (UC-05)");
                    Console.WriteLine("2. Cap nhat trang thai lich hen");
                    Console.WriteLine();
                    Console.WriteLine("-- Quan Ly San Pham & Dich Vu --");
                    Console.WriteLine("3. Quan ly san pham (UC-06)");
                    Console.WriteLine("4. Quan ly dich vu");
                    Console.WriteLine();
                    Console.WriteLine("-- Quan Ly Don Hang --");
                    Console.WriteLine("5. Xem tat ca don hang");
                    Console.WriteLine("6. Cap nhat trang thai don hang");
                    Console.WriteLine();
                    Console.WriteLine("-- Quan Ly Nguoi Dung --");
                    Console.WriteLine("7. Quan ly tai khoan Khach hang");
                    Console.WriteLine("8. Quan ly tai khoan Barber");
                    Console.WriteLine("9. Quan ly tai khoan Ung vien");
                    Console.WriteLine();
                    Console.WriteLine("-- Quan Ly Tuyen Dung --");
                    Console.WriteLine("10. Quan ly tin tuyen dung");
                    Console.WriteLine("11. Duyet ho so ung vien (UC-07)");
                    Console.WriteLine();
                    Console.WriteLine("0. Dang xuat");
                    Console.Write("Nhap lua chon cua ban: ");
                    string input = Console.ReadLine();

                    switch (input)
                    {
                        case "1":
                            // Implement View All Appointments
                            Console.WriteLine("Chuc nang xem tat ca lich hen dang phat trien.");
                            break;
                        case "2":
                            // Implement Update Appointment Status
                            Console.WriteLine("Chuc nang cap nhat trang thai lich hen dang phat trien.");
                            break;
                        case "3":
                            bool exitProductMenu = false;
                            while (!exitProductMenu)
                            {
                                Console.WriteLine();
                                Console.WriteLine("------------------------------------------------");
                                Console.WriteLine("|               QUAN LY SAN PHAM               |");
                                Console.WriteLine("------------------------------------------------");
                                Console.WriteLine("1. Xem danh sach san pham");
                                Console.WriteLine("2. Them san pham moi");
                                Console.WriteLine("3. Sua thong tin san pham");
                                Console.WriteLine("4. Xoa san pham");
                                Console.WriteLine("0. Quay lai Menu Admin");
                                Console.Write("Nhap lua chon cua ban: ");
                                string productChoice = Console.ReadLine();

                                switch (productChoice)
                                {
                                    case "1":
                                        var products = inventoryService.GetAllProducts();
                                        if (products.Count == 0)
                                        {
                                            Console.WriteLine("Khong co san pham nao.");
                                        }
                                        else
                                        {
                                            Console.WriteLine("Danh sach san pham:");
                                            foreach (var p in products)
                                            {
                                                Console.WriteLine($"ID: {p.ProductID}, Ten: {p.ProductName}, Gia: {p.Price}, So luong: {p.QuantityInStock}");
                                            }
                                        }
                                        break;
                                    case "2":
                                        Console.Write("Ten san pham: ");
                                        string newName = Console.ReadLine();
                                        Console.Write("Gia: ");
                                        if (!decimal.TryParse(Console.ReadLine(), out decimal newPrice))
                                        {
                                            Console.WriteLine("Gia khong hop le.");
                                            break;
                                        }
                                        Console.Write("So luong: ");
                                        if (!int.TryParse(Console.ReadLine(), out int newQuantity))
                                        {
                                            Console.WriteLine("So luong khong hop le.");
                                            break;
                                        }
                                        Console.Write("Mo ta: ");
                                        string newDescription = Console.ReadLine();
                                        Console.Write("Danh muc: ");
                                        string newCategory = Console.ReadLine();
                                        Console.Write("URL hinh anh: ");
                                        string newImageURL = Console.ReadLine();

                                        var newProduct = new Entities.Product
                                        {
                                            ProductName = newName,
                                            Price = newPrice,
                                            QuantityInStock = newQuantity,
                                            Description = newDescription,
                                            Category = newCategory,
                                            ImageURL = newImageURL
                                        };
                                        bool added = inventoryService.AddNewProduct(newProduct);
                                        Console.WriteLine(added ? "Them san pham thanh cong." : "Them san pham that bai.");
                                        break;
                                    case "3":
                                        Console.Write("Nhap ID san pham can sua: ");
                                        string editId = Console.ReadLine();
                                        var productToEdit = inventoryService.GetAllProducts().Find(p => p.ProductID == editId);
                                        if (productToEdit == null)
                                        {
                                            Console.WriteLine("Khong tim thay san pham.");
                                            break;
                                        }
                                        Console.Write($"Ten san pham ({productToEdit.ProductName}): ");
                                        string editName = Console.ReadLine();
                                        if (!string.IsNullOrEmpty(editName))
                                        {
                                            productToEdit.ProductName = editName;
                                        }
                                        Console.Write($"Gia ({productToEdit.Price}): ");
                                        string editPriceStr = Console.ReadLine();
                                        if (!string.IsNullOrEmpty(editPriceStr) && decimal.TryParse(editPriceStr, out decimal editPrice))
                                        {
                                            productToEdit.Price = editPrice;
                                        }
                                        Console.Write($"So luong ({productToEdit.QuantityInStock}): ");
                                        string editQuantityStr = Console.ReadLine();
                                        if (!string.IsNullOrEmpty(editQuantityStr) && int.TryParse(editQuantityStr, out int editQuantity))
                                        {
                                            productToEdit.QuantityInStock = editQuantity;
                                        }
                                        Console.Write($"Mo ta ({productToEdit.Description}): ");
                                        string editDescription = Console.ReadLine();
                                        if (!string.IsNullOrEmpty(editDescription))
                                        {
                                            productToEdit.Description = editDescription;
                                        }
                                        Console.Write($"Danh muc ({productToEdit.Category}): ");
                                        string editCategory = Console.ReadLine();
                                        if (!string.IsNullOrEmpty(editCategory))
                                        {
                                            productToEdit.Category = editCategory;
                                        }
                                        Console.Write($"URL hinh anh ({productToEdit.ImageURL}): ");
                                        string editImageURL = Console.ReadLine();
                                        if (!string.IsNullOrEmpty(editImageURL))
                                        {
                                            productToEdit.ImageURL = editImageURL;
                                        }
                                        bool updated = inventoryService.UpdateExistingProduct(productToEdit);
                                        Console.WriteLine(updated ? "Cap nhat san pham thanh cong." : "Cap nhat san pham that bai.");
                                        break;
                                    case "4":
                                        Console.Write("Nhap ID san pham can xoa: ");
                                        string deleteId = Console.ReadLine();
                                        bool deleted = inventoryService.DeleteExistingProduct(deleteId);
                                        Console.WriteLine(deleted ? "Xoa san pham thanh cong." : "Xoa san pham that bai.");
                                        break;
                                    case "0":
                                        exitProductMenu = true;
                                        break;
                                    default:
                                        Console.WriteLine("Lua chon khong hop le. Vui long thu lai.");
                                        break;
                                }
                            }
                            break;
                        case "4":
                            bool exitServiceMenu = false;
                            while (!exitServiceMenu)
                            {
                                Console.WriteLine();
                                Console.WriteLine("------------------------------------------------");
                                Console.WriteLine("|               QUAN LY DICH VU                |");
                                Console.WriteLine("------------------------------------------------");
                                Console.WriteLine("1. Xem danh sach dich vu");
                                Console.WriteLine("2. Them dich vu moi");
                                Console.WriteLine("3. Sua thong tin dich vu");
                                Console.WriteLine("4. Xoa dich vu");
                                Console.WriteLine("0. Quay lai Menu Admin");
                                Console.Write("Nhap lua chon cua ban: ");
                                string serviceChoice = Console.ReadLine();

                                switch (serviceChoice)
                                {
                                    case "1":
                                        var services = inventoryService.GetAllServices();
                                        if (services.Count == 0)
                                        {
                                            Console.WriteLine("Khong co dich vu nao.");
                                        }
                                        else
                                        {
                                            Console.WriteLine("------------------------------------------------");
                                            Console.WriteLine("|           DANH SACH DICH VU HIEN CO          |");
                                            Console.WriteLine("------------------------------------------------");
                                            Console.WriteLine("ID | Ten dich vu         | Gia (VND) | Thoi gian (phut)");
                                            Console.WriteLine("---|---------------------|-----------|------------------");
                                            foreach (var s in services)
                                            {
                                                Console.WriteLine($"{s.ServiceID}  | {s.ServiceName,-20} | {s.Price,9} | {s.Duration.TotalMinutes,16}");
                                            }
                                            Console.WriteLine();
                                            Console.WriteLine("Nhan phim bat ky de quay lai...");
                                            Console.ReadKey();
                                        }
                                        break;
                                    case "2":
                                        await AddService();
                                        break;
                                    case "3":
                                        await UpdateService();
                                        break;
                                    case "4":
                                        await DeleteService();
                                        break;
                                    case "0":
                                        exitServiceMenu = true;
                                        break;
                                    default:
                                        Console.WriteLine("Lua chon khong hop le. Vui long thu lai.");
                                        break;
                                }
                            }
                            break;
                        case "5":
                            await ViewOrders();
                            break;
                        case "6":
                            await UpdateOrderStatusAdmin();
                            break;
                        case "7":
                            await ManageUsersByRole(Entities.UserRole.CUSTOMER);
                            break;
                        case "8":
                            await ManageUsersByRole(Entities.UserRole.BARBER);
                            break;
                        case "9":
                            await ManageUsersByRole(Entities.UserRole.CANDIDATE);
                            break;
                        case "10":
                            await ManageJobPostings(adminUserId);
                            break;
                        case "11":
                            await ApproveCandidateApplications();
                            break;
                        case "0":
                            exit = true;
                            break;
                        default:
                            Console.WriteLine("Lua chon khong hop le. Vui long thu lai.");
                            break;
                    }
                }
            }

            static async Task CandidateMenu(string candidateName, string candidateId)
            {
                bool exit = false;
                while (!exit)
                {
                    Console.WriteLine();
                    Console.WriteLine("------------------------------------------------");
                    Console.WriteLine($"|  CHAO MUNG UNG VIEN, {candidateName}  |");
                    Console.WriteLine("------------------------------------------------");
                    Console.WriteLine("|                MENU UNG VIEN                 |");
                    Console.WriteLine("------------------------------------------------");
                    Console.WriteLine("1. Xem cac vi tri tuyen dung");
                    Console.WriteLine("2. Xem chi tiet tin tuyen dung");
                    Console.WriteLine("3. Nop ho so ung tuyen");
                    Console.WriteLine("4. Xem trang thai ho so");
                    Console.WriteLine("0. Dang xuat");
                    Console.Write("Nhap lua chon cua ban: ");
                    string input = Console.ReadLine();

                    switch (input)
                    {
                        case "1":
                            // Show job postings
                            var postings = recruitmentService.GetAllJobPostings();
                            if (postings.Count == 0)
                            {
                                Console.WriteLine("Khong co vi tri tuyen dung nao.");
                            }
                            else
                            {
                                Console.WriteLine("Danh sach cac vi tri tuyen dung:");
                                Console.WriteLine("ID | Tieu De           | Dia Diem        | Han Nop");
                                Console.WriteLine("---|-------------------|-----------------|----------");
                                foreach (var p in postings)
                                {
                                    Console.WriteLine($"{p.JobPostingID,-2} | {p.Title,-17} | {p.Location,-15} | {p.ClosingDate?.ToString("yyyy-MM-dd") ?? "N/A"}");
                                }
                            }
                            break;
                        case "2":
                            // View job posting details
                            Console.Write("Nhap ID tin de xem chi tiet (hoac 0 de quay lai): ");
                            string jobIdDetail = Console.ReadLine();
                            if (jobIdDetail == "0")
                            {
                                break;
                            }
                            var jobPostings = recruitmentService.GetAllJobPostings();
                            var job = jobPostings.Find(jp => jp.JobPostingID == jobIdDetail);
                            if (job == null)
                            {
                                Console.WriteLine("Khong tim thay tin tuyen dung.");
                            }
                            else
                            {
                                Console.WriteLine("------------------------------------------------");
                                Console.WriteLine("|           CHI TIET TIN TUYEN DUNG           |");
                                Console.WriteLine("------------------------------------------------");
                                Console.WriteLine($"Ma tin: {job.JobPostingID}");
                                Console.WriteLine($"Tieu de: {job.Title}");
                                Console.WriteLine("Mo ta cong viec:");
                                Console.WriteLine(job.Description);
                                Console.WriteLine("Yeu cau:");
                                Console.WriteLine(job.Requirements);
                                Console.WriteLine($"Dia diem: {job.Location}");
                                Console.WriteLine($"Muc luong: {job.SalaryRange}");
                                Console.WriteLine($"Han nop ho so: {job.ClosingDate?.ToString("yyyy-MM-dd") ?? "N/A"}");
                                Console.WriteLine("------------------------------------------------");
                                Console.WriteLine("1. Ung tuyen ngay");
                                Console.WriteLine("0. Quay lai danh sach");
                                Console.Write("Nhap lua chon cua ban: ");
                                string applyChoice = Console.ReadLine();
                                if (applyChoice == "1")
                                {
                                    // Proceed to application submission
                                    Console.WriteLine("------------------------------------------------");
                                    Console.WriteLine($"|           NOP HO SO CHO VI TRI: {job.Title}           |");
                                    Console.WriteLine("------------------------------------------------");
                                    Console.WriteLine("Thong tin ca nhan cua ban:");
                                    Console.WriteLine($"Ho ten: {candidateName} (Lay tu profile)");
                                    Console.Write("Duong dan den CV (VD: Google Drive, Dropbox...): ");
                                    string cvPath = Console.ReadLine();
                                    Console.WriteLine("Thu gioi thieu/Cover Letter (Nhap truc tiep hoac bo trong): ");
                                    string coverLetter = "";
                                    string line;
                                    while ((line = Console.ReadLine()) != null)
                                    {
                                        if (line.Trim() == "ENDCVL")
                                            break;
                                        coverLetter += line + Environment.NewLine;
                                    }
                                    Console.WriteLine("[1. Xac nhan nop ho so] [0. Huy]");
                                    Console.Write("Nhap lua chon cua ban: ");
                                    string confirm = Console.ReadLine();
                                    if (confirm == "1")
                                    {
                                        var application = new Entities.JobApplication
                                        {
                                            JobPostingID = job.JobPostingID,
                                            CandidateID = candidateId,
                                            ResumeLink = cvPath,
                                            CoverLetter = coverLetter,
                                            SubmissionDate = DateTime.Now,
                                            Status = Entities.ApplicationStatus.RECEIVED,
                                            ReviewNotes = ""
                                        };
                                        bool applied = recruitmentService.SubmitJobApplication(application);
                                        Console.WriteLine(applied ? "Nop ho so ung tuyen thanh cong." : "Nop ho so ung tuyen that bai.");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Huy bo nop ho so.");
                                    }
                                }
                            }
                            break;
                        case "3":
                            // Submit job application (redo)
                            Console.WriteLine("------------------------------------------------");
                            Console.WriteLine("|           NOP HO SO UNG TUYEN                |");
                            Console.WriteLine("------------------------------------------------");
                            Console.Write("Nhap ID tin tuyen dung: ");
                            string enteredJobPostingId = Console.ReadLine();
                            if (string.IsNullOrEmpty(enteredJobPostingId))
                            {
                                Console.WriteLine("ID tin tuyen dung khong duoc de trong.");
                                break;
                            }
                            var jobPosting = recruitmentService.GetJobPostingById(enteredJobPostingId);
                            if (jobPosting == null)
                            {
                                Console.WriteLine("ID tin tuyen dung khong hop le.");
                                break;
                            }
                            Console.WriteLine("Thong tin ca nhan cua ban:");
                            Console.WriteLine($"Ho ten: {candidateName} (Lay tu tu profile)");
                            Console.Write("Duong dan den CV (VD: Google Drive, Dropbox...): ");
                            string cvPath2 = Console.ReadLine();
                            Console.WriteLine("Thu gioi thieu/Cover Letter (Nhap truc tiep hoac bo trong): ");
                            string coverLetter2 = "";
                            string line2;
                            while ((line2 = Console.ReadLine()) != null)
                            {
                                if (line2.Trim() == "ENDCVL")
                                    break;
                                coverLetter2 += line2 + Environment.NewLine;
                            }
                            Console.WriteLine("[1. Xac nhan nop ho so] [0. Huy]");
                            Console.Write("Nhap lua chon cua ban: ");
                            string confirm2 = Console.ReadLine();
                            if (confirm2 == "1")
                            {
                                var application = new Entities.JobApplication
                                {
                                    JobPostingID = enteredJobPostingId,
                                    CandidateID = candidateId,
                                    ResumeLink = cvPath2,
                                    CoverLetter = coverLetter2,
                                    SubmissionDate = DateTime.Now,
                                    Status = Entities.ApplicationStatus.RECEIVED,
                                    ReviewNotes = ""
                                };
                                bool applied = recruitmentService.SubmitJobApplication(application);
                                Console.WriteLine(applied ? "Nop ho so ung tuyen thanh cong." : "Nop ho so ung tuyen that bai.");
                            }
                            else
                            {
                                Console.WriteLine("Huy bo nop ho so.");
                            }
                            break;
case "4":
    // View application status
    Console.Write("Nhap email de xem trang thai ho so: ");
    string applicantEmail = Console.ReadLine();
    var user = userService.GetAllUsers().Find(u => u.Email == applicantEmail);
    if (user == null)
    {
        Console.WriteLine("Khong tim thay nguoi dung voi email nay.");
        break;
    }
    var applications = recruitmentService.GetApplicationsByCandidate(user.UserID);
    if (applications.Count == 0)
    {
        Console.WriteLine("Khong tim thay ho so ung tuyen nao.");
    }
    else
    {
        Console.WriteLine("Trang thai cac ho so ung tuyen:");
        foreach (var app in applications)
        {
            Console.WriteLine($"Job ID: {app.JobPostingID}, Status: {app.Status}, Ngay nop: {app.SubmissionDate}");
        }
    }
    Console.WriteLine("Nhan phim bat ky de thoat...");
    Console.ReadKey();
    break;
                        case "0":
                            exit = true;
                            break;
                        default:
                            Console.WriteLine("Lua chon khong hop le. Vui long thu lai.");
                            break;
                    }
                }
            }

            static async Task AddService()
            {
                Console.WriteLine("------------------------------------------------");
                Console.WriteLine("|                 THEM DICH VU MOI              |");
                Console.WriteLine("------------------------------------------------");
                Console.Write("Nhap ten dich vu: ");
                string name = Console.ReadLine();
                Console.Write("Nhap mo ta (khong bat buoc): ");
                string description = Console.ReadLine();
                Console.Write("Nhap gia (VND): ");
                if (!decimal.TryParse(Console.ReadLine(), out decimal price) || price <= 0)
                {
                    Console.WriteLine("Gia khong hop le. Phai lon hon 0.");
                    return;
                }
                Console.Write("Nhap thoi gian thuc hien (phut): ");
                if (!int.TryParse(Console.ReadLine(), out int duration) || duration <= 0)
                {
                    Console.WriteLine("Thoi gian khong hop le. Phai lon hon 0.");
                    return;
                }
                Console.WriteLine("1. Luu dich vu");
                Console.WriteLine("0. Huy bo");
                Console.Write("Chon: ");
                string choice = Console.ReadLine();
                if (choice == "1")
                {
                    var newService = new Entities.Service
                    {
                        ServiceName = name,
                        Description = description,
                        Price = price,
                        Duration = TimeSpan.FromMinutes(duration)
                    };
                    bool added = inventoryService.AddNewService(newService);
                    Console.WriteLine(added ? $"Da them dich vu [{name}] thanh cong!" : "Them dich vu that bai.");
                }
                else
                {
                    Console.WriteLine("Huy bo them dich vu.");
                }
            }

            static async Task UpdateService()
            {
                Console.WriteLine("------------------------------------------------");
                Console.WriteLine("|               SUA THONG TIN DICH VU           |");
                Console.WriteLine("------------------------------------------------");
                Console.Write("Nhap ID dich vu can sua: ");
                string id = Console.ReadLine();
                var service = inventoryService.GetAllServices().Find(s => s.ServiceID == id);
                if (service == null)
                {
                    Console.WriteLine("Khong tim thay dich vu.");
                    return;
                }
                Console.WriteLine($"- Ten hien tai: {service.ServiceName}");
                Console.WriteLine($"- Mo ta: {service.Description}");
                Console.WriteLine($"- Gia: {service.Price} VND");
                Console.WriteLine($"- Thoi gian: {service.Duration.TotalMinutes} phut");

                Console.Write("Nhap ten moi (Enter de giu nguyen): ");
                string newName = Console.ReadLine();
                Console.Write("Nhap mo ta moi: ");
                string newDescription = Console.ReadLine();
                Console.Write("Nhap gia moi: ");
                string priceInput = Console.ReadLine();
                Console.Write("Nhap thoi gian moi (phut): ");
                string durationInput = Console.ReadLine();

                Console.WriteLine("1. Cap nhat");
                Console.WriteLine("0. Huy bo");
                Console.Write("Chon: ");
                string choice = Console.ReadLine();

                if (choice == "1")
                {
                    if (!string.IsNullOrEmpty(newName))
                    {
                        service.ServiceName = newName;
                    }
                    if (!string.IsNullOrEmpty(newDescription))
                    {
                        service.Description = newDescription;
                    }
                    if (!string.IsNullOrEmpty(priceInput))
                    {
                        if (decimal.TryParse(priceInput, out decimal newPrice) && newPrice > 0)
                        {
                            service.Price = newPrice;
                        }
                        else
                        {
                            Console.WriteLine("Gia moi khong hop le. Bo qua cap nhat gia.");
                        }
                    }
                    if (!string.IsNullOrEmpty(durationInput))
                    {
                        if (int.TryParse(durationInput, out int newDuration) && newDuration > 0)
                        {
                            service.Duration = TimeSpan.FromMinutes(newDuration);
                        }
                        else
                        {
                            Console.WriteLine("Thoi gian moi khong hop le. Bo qua cap nhat thoi gian.");
                        }
                    }
                    bool updated = inventoryService.UpdateExistingService(service);
                    Console.WriteLine(updated ? "Cap nhat dich vu thanh cong." : "Cap nhat dich vu that bai.");
                }
                else
                {
                    Console.WriteLine("Huy bo cap nhat dich vu.");
                }
            }

            static async Task DeleteService()
            {
                Console.WriteLine("------------------------------------------------");
                Console.WriteLine("|                 XOA DICH VU                   |");
                Console.WriteLine("------------------------------------------------");
                Console.Write("Nhap ID dich vu can xoa: ");
                string id = Console.ReadLine();
                var service = inventoryService.GetAllServices().Find(s => s.ServiceID == id);
                if (service == null)
                {
                    Console.WriteLine("Khong tim thay dich vu.");
                    return;
                }
                Console.WriteLine($"Tim thay dich vu: {service.ServiceName} (ID:{service.ServiceID})");
                Console.WriteLine("Ban co CHAC CHAN muon xoa dich vu nay?");
                Console.WriteLine("1. Xoa");
                Console.WriteLine("0. Huy bo");
                Console.Write("Chon: ");
                string choice = Console.ReadLine();

                if (choice == "1")
                {
                    bool hasAppointments = bookingService.HasAppointmentsForService(service.ServiceID);
                    if (hasAppointments)
                    {
                        Console.WriteLine("Khong the xoa dich vu da co lich hen!");
                        return;
                    }
                    bool deleted = inventoryService.DeleteExistingService(service.ServiceID);
                    Console.WriteLine(deleted ? "Xoa dich vu thanh cong." : "Xoa dich vu that bai.");
                }
                else
                {
                    Console.WriteLine("Huy bo xoa dich vu.");
                }
            }

            static async Task LoginScreen()
            {
                bool exit = false;
                while (!exit)
                {
                    Console.WriteLine();
                    Console.WriteLine("------------------------------------------------");
                    Console.WriteLine("|                    DANG NHAP                  |");
                    Console.WriteLine("------------------------------------------------");
                    Console.Write("Email: ");
                    string email = Console.ReadLine();
                    Console.Write("Mat khau: ");
                    string password = Console.ReadLine();

                    Console.WriteLine();
                    Console.WriteLine("[1. Dang nhap] [0. Quay lai]");
                    Console.Write("Nhap lua chon cua ban: ");
                    string input = Console.ReadLine();

                    switch (input)
                    {
                        case "1":
                            var user = userService.AuthenticateUser(email, password);
                            if (user != null)
                            {
                                Console.WriteLine($"Dang nhap thanh cong. Chao mung {user.FullName}!");
                                exit = true;
                                if (user.Role == Entities.UserRole.CUSTOMER)
                                {
                                    await CustomerLoggedInMenu(user.FullName, user.UserID);
                                }
                                else if (user.Role == Entities.UserRole.ADMIN)
                                {
                                    await AdminLoggedInMenu(user.FullName, user.UserID);
                                }
                                else if (user.Role == Entities.UserRole.CANDIDATE)
                                {
                                    await CandidateMenu(user.FullName, user.UserID);
                                }
                                else if (user.Role == Entities.UserRole.BARBER)
                                {
                                    await BarberMenu(user.FullName, user.UserID);
                                }
                                else
                                {
                                    // Other roles can be handled here
                                }
                            }
                            else
                            {
                                Console.WriteLine("Dang nhap that bai. Email hoac mat khau khong dung.");
                            }
                            break;
                        case "0":
                            exit = true;
                            break;
                        default:
                            Console.WriteLine("Lua chon khong hop le. Vui long thu lai.");
                            break;
                    }
                }
                await Task.CompletedTask;
            }

            static async Task ViewOrders()
            {
                var orders = orderService.GetAllOrders();
                if (orders.Count == 0)
                {
                    Console.WriteLine("Khong co don hang nao.");
                    Console.WriteLine("Press any key to return...");
                    Console.ReadKey();
                    return;
                }
                Console.WriteLine("Danh sach tat ca don hang:");
                Console.WriteLine("--------------------------------------------------------------------------------");
                Console.WriteLine("STT | OrderID                             | CustomerID                        | OrderDate           | TotalAmount | Status    ");
                Console.WriteLine("--------------------------------------------------------------------------------");
                int index = 1;
                foreach (var order in orders)
                {
                    Console.WriteLine($"{index,-4}| {order.OrderID,-35} | {order.CustomerID,-30} | {order.OrderDate,-19} | {order.TotalAmount,11} | {order.Status}");
                    index++;
                }
                Console.WriteLine("--------------------------------------------------------------------------------");
                Console.WriteLine("Press any key to return...");
                Console.ReadKey();
                await Task.CompletedTask;
            }

            static async Task BuyProducts(string customerId)
            {
                Console.WriteLine("------------------------------------------------");
                Console.WriteLine("|                 MUA SAN PHAM                  |");
                Console.WriteLine("------------------------------------------------");

                var products = inventoryService.GetAllProducts();
                if (products.Count == 0)
                {
                    Console.WriteLine("Khong co san pham nao.");
                    return;
                }

                var cart = new Dictionary<string, int>();
                bool shopping = true;

                while (shopping)
                {
                    Console.WriteLine("ID | Ten San Pham           | Gia       | Con Lai");
                    Console.WriteLine("---|------------------------|-----------|--------");
                    for (int i = 0; i < products.Count; i++)
                    {
                        var p = products[i];
                        Console.WriteLine($"{i + 1,-3}| {p.ProductName,-22} | {p.Price,-9} | {p.QuantityInStock}");
                    }
                    Console.WriteLine();
                    Console.Write("Nhap ID san pham muon them vao gio hang (nhap 'PAY' de thanh toan, 'CART' de xem gio hang, '0' de quay lai): ");
                    string input = Console.ReadLine()?.Trim().ToUpper();

                    if (input == "0")
                    {
                        shopping = false;
                        break;
                    }
                    else if (input == "PAY")
                    {
                        if (cart.Count == 0)
                        {
                            Console.WriteLine("Gio hang rong. Khong co don hang nao duoc tao.");
                            continue;
                        }

                        Console.WriteLine("Xac nhan don hang:");
                        decimal total = 0;

                        // Display cart in table format
                        Console.WriteLine("------------------------------------------------");
                        Console.WriteLine("|                      GIO HANG                  |");
                        Console.WriteLine("------------------------------------------------");
                        Console.WriteLine("STT | Ten San Pham           | So Luong | Don Gia  | Thanh Tien");
                        Console.WriteLine("----|------------------------|----------|----------|------------");

                        int stt = 1;
                        foreach (var item in cart)
                        {
                            var product = products.Find(p => p.ProductID == item.Key);
                            if (product != null)
                            {
                                decimal itemTotal = product.Price * item.Value;
                                total += itemTotal;
                                Console.WriteLine($"{stt,-4}| {product.ProductName,-22} | {item.Value,-8} | {product.Price,-8} | {itemTotal,-10}");
                                stt++;
                            }
                        }
                        Console.WriteLine("------------------------------------------------");
                        Console.WriteLine($"TONG CONG: {total}");
                        Console.WriteLine();
                        Console.WriteLine("[1. Xac nhan] [0. Huy]");
                        Console.Write("Nhap lua chon cua ban: ");
                        string confirm = Console.ReadLine();

                        if (confirm == "1")
                        {
                            decimal totalAmount = 0;
                            foreach (var item in cart)
                            {
                                var product = products.Find(p => p.ProductID == item.Key);
                                if (product != null)
                                {
                                    totalAmount += product.Price * item.Value;
                                }
                            }

                            Console.WriteLine("------------------------------------------------");
                            Console.WriteLine("|                 THANH TOAN                    |");
                            Console.WriteLine("------------------------------------------------");
                            Console.WriteLine($"Tong so tien can thanh toan: {totalAmount}");
                            Console.Write("Dia chi giao hang: ");
                            string shippingAddress = Console.ReadLine();
                            Console.Write("Phuong thuc thanh toan (Vi du: Tien mat, Chuyen khoan): ");
                            string paymentMethod = Console.ReadLine();
                            Console.Write("Ghi chu don hang (neu co): ");
                            string orderNotes = Console.ReadLine();

                            Console.WriteLine();
                            Console.WriteLine("[1. Xac nhan thanh toan] [0. Huy]");
                            Console.Write("Nhap lua chon cua ban: ");
                            string paymentConfirm = Console.ReadLine();

                            if (paymentConfirm == "1")
                            {
                                var order = new ConsoleApp1.Entities.Order
                                {
                                    OrderID = Guid.NewGuid().ToString(),
                                    CustomerID = customerId,
                                    ShippingAddress = shippingAddress,
                                    PaymentMethod = paymentMethod,
                                    Notes = orderNotes,
                                    TotalAmount = totalAmount
                                };

                                foreach (var item in cart)
                                {
                                    var product = products.Find(p => p.ProductID == item.Key);
                                    if (product != null)
                                    {
                                        var orderItem = new ConsoleApp1.Entities.OrderItem
                                        {
                                            ProductID = product.ProductID,
                                            ProductNameSnapshot = product.ProductName,
                                            Quantity = item.Value,
                                            PriceAtOrder = product.Price
                                        };
                                        order.AddOrderItem(orderItem);
                                    }
                                }

                                bool success = orderService.CreateOrder(order);
                                if (success)
                                {
                                    bool statusUpdated = orderService.UpdateOrderStatus(order.OrderID, ConsoleApp1.Entities.OrderStatus.PAID);
                                    if (statusUpdated)
                                    {
                                        Console.WriteLine("Dat hang thanh cong va da cap nhat trang thai thanh toan.");
                                        // Reduce stock quantity for each product
                                        foreach (var item in cart)
                                        {
                                            var product = products.Find(p => p.ProductID == item.Key);
                                            if (product != null)
                                            {
                                                product.QuantityInStock -= item.Value;
                                                if (product.QuantityInStock < 0)
                                                {
                                                    product.QuantityInStock = 0;
                                                }
                                                inventoryService.UpdateExistingProduct(product);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Dat hang thanh cong nhung cap nhat trang thai that bai.");
                                    }
                                    cart.Clear();
                                }
                                else
                                {
                                    Console.WriteLine("Dat hang that bai.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Huy don hang.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Huy don hang.");
                        }
                    }
                    else if (input == "CART")
                    {
                        if (cart.Count == 0)
                        {
                            Console.WriteLine("Gio hang rong.");
                        }
                        else
                        {
                            // Display cart in table format
                            Console.WriteLine("------------------------------------------------");
                            Console.WriteLine("|                      GIO HANG                  |");
                            Console.WriteLine("------------------------------------------------");
                            Console.WriteLine("STT | Ten San Pham           | So Luong | Don Gia  | Thanh Tien");
                            Console.WriteLine("----|------------------------|----------|----------|------------");

                            int stt = 1;
                            decimal total = 0;
                            foreach (var item in cart)
                            {
                                var product = products.Find(p => p.ProductID == item.Key);
                                if (product != null)
                                {
                                    decimal itemTotal = product.Price * item.Value;
                                    total += itemTotal;
                                    Console.WriteLine($"{stt,-4}| {product.ProductName,-22} | {item.Value,-8} | {product.Price,-8} | {itemTotal,-10}");
                                    stt++;
                                }
                            }
                            Console.WriteLine("------------------------------------------------");
                            Console.WriteLine($"TONG CONG: {total}");
                            Console.WriteLine();
                            Console.WriteLine("Nhap ID san pham de xoa khoi gio, 'PAY' de thanh toan, '0' de tiep tuc mua sam: ");
                            string cartInput = Console.ReadLine()?.Trim().ToUpper();

                            if (cartInput == "0")
                            {
                                continue;
                            }
                            else if (cartInput == "PAY")
                            {
                                input = "PAY";
                                continue;
                            }
                            else
                            {
                                // Remove product from cart
                                if (int.TryParse(cartInput, out int removeIndex) && removeIndex >= 1 && removeIndex <= products.Count)
                                {
                                    var productToRemove = products[removeIndex - 1];
                                    if (cart.ContainsKey(productToRemove.ProductID))
                                    {
                                        int quantity = cart[productToRemove.ProductID];
                                        cart.Remove(productToRemove.ProductID);

                                        // Increase stock quantity
                                        productToRemove.QuantityInStock += quantity;
                                        Console.WriteLine("Da xoa san pham khoi gio hang.");
                                    }
                                    else
                                    {
                                        Console.WriteLine("San pham khong co trong gio hang.");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Lua chon khong hop le hoac san pham khong co trong gio hang.");
                                }
                            }
                        }
                    }
                    else
                    {
                        // Add product to cart
                        if (!int.TryParse(input, out int productIndex) || productIndex < 1 || productIndex > products.Count)
                        {
                            Console.WriteLine("San pham khong ton tai.");
                            continue;
                        }
                        var product = products[productIndex - 1];
                        if (product.QuantityInStock <= 0)
                        {
                            Console.WriteLine("San pham da het hang.");
                            continue;
                        }
                        Console.Write($"Nhap so luong muon them (con lai {product.QuantityInStock}): ");
                        string quantityInput = Console.ReadLine();
                        if (!int.TryParse(quantityInput, out int quantity) || quantity < 1)
                        {
                            Console.WriteLine("So luong khong hop le.");
                            continue;
                        }
                        if (quantity > product.QuantityInStock)
                        {
                            Console.WriteLine("So luong vuot qua so luong con lai.");
                            continue;
                        }
                        if (cart.ContainsKey(product.ProductID))
                        {
                            cart[product.ProductID] += quantity;
                        }
                        else
                        {
                            cart[product.ProductID] = quantity;
                        }
                        // Removed immediate stock reduction here to avoid double reduction
                        // product.QuantityInStock -= quantity;
                        Console.WriteLine("Da them san pham vao gio hang.");
                    }
                }
            }
        }
    }
