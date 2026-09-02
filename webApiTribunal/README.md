scaffold

dotnet ef dbcontext scaffold "Name=ConnectionStrings:DefaultConnection" Microsoft.EntityFrameworkCore.SqlServer --output-dir Models/Entities --context-dir Models/Entities --context DBContext -f --no-pluralize -t AppsAccessLog -t AppsAccessToken