using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SoftUni.Data;
using SoftUni.Models;

namespace SoftUni
{
    public class StartUp
    {
        static void Main(string[] args)
        {
            var context = new SoftUniContext();

            var result = "";

            using (context)
            {
                result = RemoveTown(context);
            }

            Console.WriteLine(result);
        }

        public static string GetEmployeesFullInformation(SoftUniContext context)
        {
            var sb = new StringBuilder();
            
                foreach (var employee in context.Employees)
                {
                    sb.AppendLine($"{employee.FirstName} {employee.LastName} {employee.MiddleName} {employee.JobTitle} {employee.Salary:f2}");
                }            

            return sb.ToString().Trim();
        }

        public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
        {
            var sb = new StringBuilder();

            var employees = context.Employees.Select(e => new
            {
                e.FirstName,
                e.Salary
            })
                .Where(e => e.Salary > 50000)
                .OrderBy(e => e.FirstName);

            foreach (var emp in employees)
            {
                sb.AppendLine($"{emp.FirstName} - {emp.Salary:f2}");
            }

            return sb.ToString().Trim();
        }

        public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
        {
            var employees = context.Employees.Include(e => e.Department)
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.Department.Name,
                    e.Salary
                }).Where(e => e.Name == "Research and Development")
                .OrderBy(e => e.Salary)
                .ThenByDescending(e => e.FirstName);

            var sb = new StringBuilder();

            foreach (var emp in employees)
            {
                sb.AppendLine($"{emp.FirstName} {emp.LastName} from {emp.Name} - ${emp.Salary:f2}");
            }

            return sb.ToString().Trim();
        }

        public static string AddNewAddressToEmployee(SoftUniContext context)
        {
            var address = new Address
            {
                AddressText = "Vitoshka 15",
                TownId = 4
            };

            context.Addresses.Add(address);

            var empl = context.Employees.FirstOrDefault(e => e.LastName == "Nakov");

            empl.Address = address;

            context.SaveChanges();

            var employees = context.Employees.Include(e => e.Address)
                .Select(e => new
                {
                    e.Address.AddressText,
                    e.AddressId
                })
                .OrderByDescending(e => e.AddressId)
                .Take(10);

            var sb = new StringBuilder();

            foreach (var item in employees)
            {
                sb.AppendLine(item.AddressText);
            }

            return sb.ToString().Trim();
        }

        public static string GetEmployeesInPeriod(SoftUniContext context)
        {
            var employess = context.Employees
                .Where(e => e.EmployeesProjects.Any(ep => ep.Project.StartDate.Year >= 2001 && ep.Project.StartDate.Year <= 2003))
                .Select(e => new
                {
                    EmployeeFullName = e.FirstName + " " + e.LastName,
                    ManagerFullName = e.Manager.FirstName + " " + e.Manager.LastName,
                    Projects = e.EmployeesProjects.Select(ep => new
                    {
                        ProjectName = ep.Project.Name,
                        StartDate = ep.Project.StartDate,
                        EndDate = ep.Project.EndDate
                    })
                })                
                .Take(10)
                .ToList();

            var sb = new StringBuilder();

            foreach (var employee in employess)
            {
                sb.AppendLine($"{employee.EmployeeFullName} - Manager: {employee.ManagerFullName}");

                foreach (var item in employee.Projects)
                {

                    if (item.EndDate == null)
                    {
                        sb.AppendLine($"--{item.ProjectName} - {item.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)} - not finished");
                    }
                    else
                        sb.AppendLine($"--{item.ProjectName} - {item.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)} - {item.EndDate.Value.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)}");
                }
            }

            return sb.ToString().Trim();
        }

        public static string GetAddressesByTown(SoftUniContext context)
        {
            var addresses = context.Addresses
                .Select(a => new
            {
                Text = a.AddressText,
                Town = a.Town.Name,
                Employees = a.Employees.Count
            })
                .OrderByDescending(ad => ad.Employees)
                .ThenBy(ad => ad.Town)
                .ThenBy(ad => ad.Text)
                .Take(10);

            var sb = new StringBuilder();

            foreach (var address in addresses)
            {
                sb.AppendLine($"{address.Text}, {address.Town} - {address.Employees} employees");
            }

            return sb.ToString().Trim();
        }

        public static string GetEmployee147(SoftUniContext context)
        {
            var employee = context.Employees.Single(e => e.EmployeeId == 147);

            var sb = new StringBuilder();

            sb.AppendLine($"{employee.FirstName} {employee.LastName} - {employee.JobTitle}");

            var projects = new List<string>();

            foreach (var project in context.EmployeesProjects.Where(e => e.EmployeeId == 147))
            {
                var proj = context.Projects.Single(p => p.ProjectId == project.ProjectId);

                projects.Add(proj.Name);
            }

            projects.Sort();            

            foreach (var pro in projects)
            {
                sb.AppendLine(pro);
            }

            return sb.ToString().Trim();
        }

        public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
        {
            var departments = context.Departments.Where(d => d.Employees.Count > 5)
                .Select(d => new
                {
                    Name = d.Name,
                    Manager = d.Manager.FirstName + " " + d.Manager.LastName,
                    Employees = d.Employees
                })
                .OrderBy(d => d.Employees.Count)
                .ThenBy(d => d.Name);

            var sb = new StringBuilder();

            foreach (var dep in departments)
            {
                sb.AppendLine($"{dep.Name} – {dep.Manager}");

                var employees = dep.Employees
                    .Select(e => new
                    {
                        e.FirstName,
                        e.LastName,
                        e.JobTitle
                    })
                    .OrderBy(e => e.FirstName).ThenBy(e => e.LastName);

                foreach (var emp in employees)
                {
                    sb.AppendLine($"{emp.FirstName} {emp.LastName} - {emp.JobTitle}");
                }
            }

            return sb.ToString().Trim();
        }

        public static string GetLatestProjects(SoftUniContext context)
        {
            var projects = context.Projects
                .Select(p => new
                {
                    Name = p.Name,
                    Description = p.Description,
                    StartDate = p.StartDate
                })
                .OrderByDescending(p => p.StartDate)
                .Take(10)
                .OrderBy(p => p.Name);

            var sb = new StringBuilder();

            foreach (var proj in projects)
            {
                sb.AppendLine(proj.Name);
                sb.AppendLine(proj.Description);
                sb.AppendLine($"{proj.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)}");
            }

            return sb.ToString().Trim();
        }

        public static string IncreaseSalaries(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(e => e.Department.Name == "Engineering" || e.Department.Name == "Tool Design" || e.Department.Name == "Marketing" || e.Department.Name == "Information Services");

            foreach (var employee in employees)
            {
                employee.Salary = employee.Salary * (decimal)1.12;
            }

            context.SaveChanges();

            var result = employees.Select(e => new
            {
                e.FirstName,
                e.LastName,
                e.Salary
            }).OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName);

            var sb = new StringBuilder();

            foreach (var emp in result)
            {
                sb.AppendLine($"{emp.FirstName} {emp.LastName} (${emp.Salary:f2})");
            }

            return sb.ToString().Trim();
        }

        public static string GetEmployeesByFirstNameStartingWithSa(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(e => e.FirstName.StartsWith("Sa"))
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.JobTitle,
                    e.Salary
                })
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName);

            var sb = new StringBuilder();

            foreach (var emp in employees)
            {
                sb.AppendLine($"{emp.FirstName} {emp.LastName} - {emp.JobTitle} - (${emp.Salary:f2})");
            }

            return sb.ToString().Trim();
        }

        public static string DeleteProjectById(SoftUniContext context)
        {
            var projects = context.EmployeesProjects.Where(ep => ep.ProjectId == 2);

            foreach (var proj in projects)
            {
                context.EmployeesProjects.Remove(proj);
            }

            context.SaveChanges();

            var project = context.Projects.Single(e => e.ProjectId == 2);

            context.Projects.Remove(project);

            context.SaveChanges();

            var sb = new StringBuilder();

            var result = context.Projects
                .Select(p => new
                {
                    p.Name
                })
                .Take(10);

            foreach (var res in result)
            {
                sb.AppendLine(res.Name);
            }

            return sb.ToString().Trim();
        }

        public static string RemoveTown(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(e => e.Address.Town.Name == "Seattle");

            foreach (var employee in employees)
            {
                employee.AddressId = null;
            }

            context.SaveChanges();

            var addresses = context.Addresses
                .Where(a => a.Town.Name == "Seattle");

            var count = addresses.Count();

            foreach (var address in addresses)
            {
                context.Addresses.Remove(address);
            }

            context.SaveChanges();

            var seattle = context.Towns.Single(t => t.Name == "Seattle");

            context.Towns.Remove(seattle);

            context.SaveChanges();

            return $"{count} addresses in Seattle were deleted";
        }
    }
}
