using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcBlog
{
    public class ImgHandler
    {
        public static readonly List<string> AllowedFileExtensions =
        new List<string> { ".jpg", ".jpeg", ".png","gif" };

        public static readonly string ImgUploadFolder = "~/ImgUpload/";

        public static readonly string MappedUploadFolder = HttpContext.Current.Server.MapPath(ImgUploadFolder);
    }
}