using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HttpService
{
    /// <summary>
    /// 코드 + 메시지를 포함한 예외 정보를 제공합니다.
    /// </summary>
    public class ServiceException : Exception
    {
        public const string DEFAULT_CODE = "100";

        /// <summary>
        /// ServiceException 클래스의 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="code">코드</param>
        /// <param name="message">메시지</param>
        /// <param name="innerException">내부 예외</param>
        public ServiceException(string code, string message, Exception innerException) : base(message, innerException)
        {
            Code = code;
        }

        /// <summary>
        /// ServiceException 클래스의 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="code">코드</param>
        /// <param name="message">메시지</param>
        public ServiceException(string code, string message) : this(code ,message, null)
        {
        }

        /// <summary>
        /// ServiceException 클래스의 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="message">메시지</param>
        public ServiceException(string message) : this(DEFAULT_CODE, message, null)
        {
        }

        /// <summary>
        /// ServiceException 클래스의 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="message">메시지</param>
        /// <param name="innerException">내부 예외</param>
        public ServiceException(string message, Exception innerException) : this(DEFAULT_CODE, message, innerException)
        {
        }

        /// <summary>
        /// 코드
        /// </summary>
        public string Code { get; }
    }
}
