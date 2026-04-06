namespace SV22T1020497.Admin.AppCodes
{
    /// <summary>
    /// Bieu dien du lieu tra ve cua cac API
    /// </summary>
    public class ApiResult
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public ApiResult(int code, string message = "")
        {
            Code = code;
            Message = message;
        }

        /// <summary>
        /// Ma ket qua tra ve (qui uoc 0 tuc la loi hoac khong thanh cong)
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Thong bao loi (neu co)
        /// </summary>
        public string Message { get; set; } = "";
    }
}
