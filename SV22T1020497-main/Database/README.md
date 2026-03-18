# LiteCommerceDB

Project hien dang duoc cau hinh de doc du lieu tu:

`Server=.\PHUTUAN;Database=LiteCommerceDB;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;`

Hai file SQL nguon:

- `C:\Users\Admin\Downloads\LiteCommerceDB_CreateTables.sql`
- `C:\Users\Admin\Downloads\LiteCommerceDB_Update2026 (1).sql`

Neu database `LiteCommerceDB` chua ton tai tren instance `PHUTUAN`, co the import theo thu tu:

```powershell
sqlcmd -S .\PHUTUAN -E -i "C:\Users\Admin\Downloads\LiteCommerceDB_CreateTables.sql"
sqlcmd -S .\PHUTUAN -E -i "C:\Users\Admin\Downloads\LiteCommerceDB_Update2026 (1).sql"
```

Neu ban dang dung instance khac, sua lai `ConnectionStrings:LiteCommerceDB` trong:

- `SV22T1020497.Admin/appsettings.json`
