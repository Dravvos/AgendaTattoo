using System;
using System.Collections.Generic;
using System.Text;

namespace AgendaTattoo.BLL.Security
{
    public class FrontendOptions
    {
        public const string SectionName = "Frontend";

        /// <summary>URL base do frontend, sem barra final (ex.: "https://app.flashbook.com.br"). Usada para montar links enviados por e-mail.</summary>
        public string BaseUrl { get; set; } = string.Empty;

    }
}
