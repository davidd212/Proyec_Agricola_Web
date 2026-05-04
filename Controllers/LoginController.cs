using Proyec_Agricola_Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proyec_Agricola_Web.Controllers
{
    public class LoginController : Controller
    {
        private UsuarioDAL usuarioDAL = new UsuarioDAL();
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Autenticar(string nombreUsuario, string contraseña)
        {
            if (string.IsNullOrEmpty(nombreUsuario) || string.IsNullOrEmpty(contraseña))
            {
                ViewBag.Mensaje = "Por favor completa todos los campos.";
                return View("Index");
            }

            // Validar usuario 
            var user = usuarioDAL.ValidarUsuario(nombreUsuario, contraseña);
            if (user != null)
            {
                Session["UsuarioID"] = user.UsuarioID;
                Session["Nombre"] = user.Nombre;
                Session["TipoUsuario"] = user.TipoUsuario;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Mensaje = "Usuario o contraseña incorrectos.";
                return View("Index");
            }
        }

        public ActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registro(Usuario model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Mensaje = "Completa todos los campos correctamente.";
                return View();
            }

            // Validar tipo de usuario
            if (model.TipoUsuario != 1 && model.TipoUsuario != 2)
            {
                ViewBag.Mensaje = "Selecciona un tipo de usuario válido.";
                return View();
            }

            // Validar que el usuario no exista
            if (usuarioDAL.UsuarioExiste(model.Nombre, model.Email))
            {
                ViewBag.Mensaje = "El usuario o correo ya está registrado.";
                return View();
            }

            // Registrar el nuevo usuario
            bool registroExitoso = usuarioDAL.RegistrarUsuario(model);
            if (registroExitoso)
            {
                ViewBag.Mensaje = "Registro exitoso. Por favor inicia sesión.";
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Mensaje = "Error al registrar el usuario. Intenta nuevamente.";
                return View();
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index");
        }
    }
}