using System;
using System.Collections.Generic;
using System.Text;

namespace AgendaTattoo.Data.Entities
{
    public static class Roles
    {
        public const string Owner = "Owner";

        /// <summary>Tatuador vinculado ao estúdio — acesso à própria agenda e aos clientes do estúdio.</summary>
        public const string Artist = "Artist";

        public static readonly string[] All = [Owner, Artist];
    }
}
