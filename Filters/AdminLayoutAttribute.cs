using System.Web.Mvc;

namespace Proyec_Agricola_Web.Filters
{
    public class AdminLayoutAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.Controller as Controller;
            if (controller != null)
            {
                controller.ViewBag.Layout = "~/Views/Shared/_LayoutAdmin.cshtml";
            }
            base.OnActionExecuting(filterContext);
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var viewResult = filterContext.Result as ViewResult;
            if (viewResult != null && string.IsNullOrEmpty(viewResult.MasterName))
            {
                viewResult.MasterName = "~/Views/Shared/_LayoutAdmin.cshtml";
            }
            base.OnResultExecuting(filterContext);
        }
    }
}
