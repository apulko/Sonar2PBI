using System;

namespace Sonar2PBI
{
    [Serializable]
    public class SonarqubeApiHealtyCheckException : Exception
    {
        public SonarqubeApiHealtyCheckException() { }
        public SonarqubeApiHealtyCheckException(string message) : base(message) { }
        public SonarqubeApiHealtyCheckException(string message, Exception inner) : base(message, inner) { }
        protected SonarqubeApiHealtyCheckException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    [Serializable]
    public class AzureDevopsApiHealtyCheckException : Exception
    {
        public AzureDevopsApiHealtyCheckException() { }
        public AzureDevopsApiHealtyCheckException(string message) : base(message) { }
        public AzureDevopsApiHealtyCheckException(string message, Exception inner) : base(message, inner) { }
        protected AzureDevopsApiHealtyCheckException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    [Serializable]
    public class KapConnectionHealtyCheckException : Exception
    {
        public KapConnectionHealtyCheckException() { }
        public KapConnectionHealtyCheckException(string message) : base(message) { }
        public KapConnectionHealtyCheckException(string message, Exception inner) : base(message, inner) { }
        protected KapConnectionHealtyCheckException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}