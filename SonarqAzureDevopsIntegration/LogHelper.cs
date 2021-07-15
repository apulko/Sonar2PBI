using System;
using System.Diagnostics;

namespace Sonar2PBI
{
    public static class LogHelper
    {
        public static void Logla(ApiErrorResponse apiErrorResponse)
        {
            if (apiErrorResponse != null && apiErrorResponse.errors != null && apiErrorResponse.errors.Count > 0)
            {
                string errorMessage = apiErrorResponse.errors[0].msg;
                Logla(new InvalidOperationException(errorMessage));
            }
        }

        public static void Logla(Exception logException)
        {
            Logla(logException, string.Empty);
        }
        public static void Logla(Exception logException, string webHostHtmlMessage)
        {
            try
            {
                var error = new Elmah.Error(logException);
                if (!string.IsNullOrWhiteSpace(webHostHtmlMessage))
                    error.WebHostHtmlMessage = webHostHtmlMessage;

                Elmah.ErrorLog.GetDefault(null).Log(error);
                Debug.WriteLine(logException.Message);
            }
            catch (Exception)
            {
                //Bu aşamada bir şey yapmıyoruz.
            }
        }
    }
}