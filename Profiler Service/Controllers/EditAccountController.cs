using Microsoft.AspNetCore.Mvc;
using Profiler_Service.Models;
using System.Web.Helpers;

namespace Profiler_Service.Controllers
{
    public class EditAccountController : Controller
    {
        public const string Changed = "Пользователь успешно изменён!";
        public const string NonExists = "Пользователь не существует!";
        public const string WrongPass = "Неверный старый пароль!";
        public const string Error = "Ошибка при изменении учётной записи! Проверьте введённые данные.";
        public const string DBError = "Ошибка базы данных!";

        public string Msg { get; set; } = "";
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
        public string OldPassword { get; set; } = "";
        public string DisplayedName { get; set; } = "";
        public string Contacts { set; get; } = "";

        private readonly DataBridgeWeb usersDB = new();

        public ActionResult Index()
        {
            ViewBag.String = Msg;
            return View();
        }

        [HttpPost]
        public ActionResult Index(string Login, string NewPassword, string OldPassword, string DisplayedName, string Contacts)
        {
            if (Login != null && Login.Length != 0)
            {
                if (NewPassword == null)
                    NewPassword = OldPassword;
                User registering = new()
                {
                    Login = Login,
                    // HASH password
                    Password = Crypto.HashPassword(NewPassword),
                    DisplayedName = DisplayedName,
                    Contacts = Contacts
                };
                if (OldPassword != null && OldPassword.Length > 0)
                {
                    switch (usersDB.ChangeUser(registering, OldPassword))
                    {
                        case 0:
                            Msg = Changed;
                            break;
                        case 1:
                            Msg = WrongPass;
                            break;
                        case 2:
                            Msg = DBError;
                            break;
                        case 3:
                            Msg = NonExists;
                            break;
                        case -1:
                            Msg = Error;
                            break;
                    }
                }
                else
                {
                    Msg = WrongPass;
                }
            }
            else
                Msg = Error;
            ViewBag.String = Msg;
            return View();
        }
    }
}