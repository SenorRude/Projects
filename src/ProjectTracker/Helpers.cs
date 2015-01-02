using ProjectTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProjectTracker
{
    public static class Helpers
    {
        public const string TOO_LONG = "This field exceeds the maximum length.";

        public static string GetCancelUrl(HttpRequestBase request, UrlHelper url)
        {
            if (request.UrlReferrer != null)
                return request.UrlReferrer.AbsolutePath;
            else
                return url.Action("Index");
        }

        public static List<Employee> GetSelectedEmployeesForSchool(HttpRequestBase request, IQueryable<Employee> allEmployees)
        {
            List<int> selectedIDs = new List<int>();
            if (request["Ambassadors"] != null)
            {
                foreach (int id in ParseIDs((string)request["Ambassadors"]))
                {
                    selectedIDs.Add(id);
                }
            }
            if (request["Mentors"] != null)
            {
                foreach (int id in ParseIDs((string)request["Mentors"]))
                {
                    selectedIDs.Add(id);
                }
            }
            return allEmployees.Where(i => selectedIDs.Contains(i.Emp_Id)).ToList();
        }

        public static List<int> ParseIDs(string listString)
        {
            return listString.Split(',').Select(int.Parse).ToList();
        }

        public static List<int> GetEmployeeIDs(ICollection<Employee> employees, Role.Enum role)
        {
            List<int> IDs = new List<int>();
            foreach (Employee e in employees)
            {
                if (e.Role_Id == (int)role)
                    IDs.Add(e.Emp_Id);
            }
            return IDs;
        }
    }
}