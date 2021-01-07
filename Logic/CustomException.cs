using System;
using System.Runtime.Serialization;
using System.Security;

namespace EFCodeGenerator.Logic
{
    [Serializable]
    public class CustomException : Exception, ISerializable
    {
        #region Constructors

        public CustomException()
        {
        }

        public CustomException(String message)
            : base(message)
        {
        }

        public CustomException(String message, Exception inner)
            : base(message, inner)
        {
        }

        protected CustomException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Interface (ISerializable)

        [SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            GetObjectData(info, context);
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        #endregion

        public static CustomException Create(String text, params Object[] args)
        {
            var message = string.Format(text, args);
            var ex = new CustomException(message);
            return ex;
        }

        public static CustomException Create(Exception inner, String text, params Object[] args)
        {
            var message = string.Format(text, args);
            var ex = new CustomException(message, inner);
            return ex;
        }
    }
}