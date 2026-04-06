using SV22T1020497.Admin.Models;

namespace SV22T1020497.Admin.AppCodes
{
    public static class OrderDraftService
    {
        private const string DRAFT = "OrderCreateDraft";

        public static OrderCreateDraft GetDraft()
        {
            var draft = ApplicationContext.GetSessionData<OrderCreateDraft>(DRAFT);
            if (draft == null)
            {
                draft = new OrderCreateDraft();
                ApplicationContext.SetSessionData(DRAFT, draft);
            }

            return draft;
        }

        public static void SaveDraft(OrderCreateDraft draft)
        {
            ApplicationContext.SetSessionData(DRAFT, draft ?? new OrderCreateDraft());
        }

        public static void ClearDraft()
        {
            ApplicationContext.SetSessionData(DRAFT, new OrderCreateDraft());
        }
    }
}
