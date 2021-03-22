using System;

namespace Umbraco.Core.Logging
{
    public static class Logger2Extensions
    {
        public static void Debug<T0, T1, T2>(this ILogger logger, Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Debug(reporting, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
            }
            else
            {
                logger.Debug(reporting, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
            }
        }

        public static void Debug<T0, T1>(this ILogger logger, Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Debug(reporting, messageTemplate, propertyValue0, propertyValue1);
            }
            else
            {
                logger.Debug(reporting, messageTemplate, propertyValue0, propertyValue1);
            }
        }

        public static void Debug<T0>(this ILogger logger, Type reporting, string messageTemplate, T0 propertyValue0)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Debug(reporting, messageTemplate, propertyValue0);
            }
            else
            {
                logger.Debug(reporting, messageTemplate, propertyValue0);
            }
        }

        public static void Error<T0, T1, T2>(this ILogger logger, Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Error(reporting, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
            }
            else
            {
                logger.Error(reporting, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
            }
        }

        public static void Error<T0, T1>(this ILogger logger, Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Error(reporting, exception, messageTemplate, propertyValue0, propertyValue1);
            }
            else
            {
                logger.Error(reporting, exception, messageTemplate, propertyValue0, propertyValue1);
            }
        }

        public static void Error<T0>(this ILogger logger, Type reporting, Exception exception, string messageTemplate, T0 propertyValue0)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Error(reporting, exception, messageTemplate, propertyValue0);
            }
            else
            {
                logger.Error(reporting, exception, messageTemplate, propertyValue0);
            }
        }

        public static void Fatal<T0, T1, T2>(this ILogger logger, Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Fatal(reporting, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
            }
            else
            {
                logger.Fatal(reporting, exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
            }
        }

        public static void Fatal<T0, T1>(this ILogger logger, Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Fatal(reporting, exception, messageTemplate, propertyValue0, propertyValue1);
            }
            else
            {
                logger.Fatal(reporting, exception, messageTemplate, propertyValue0, propertyValue1);
            }
        }

        public static void Fatal<T0>(this ILogger logger, Type reporting, Exception exception, string messageTemplate, T0 propertyValue0)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Fatal(reporting, exception, messageTemplate, propertyValue0);
            }
            else
            {
                logger.Fatal(reporting, exception, messageTemplate, propertyValue0);
            }
        }

        public static void Info<T0, T1, T2>(this ILogger logger, Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Info(reporting, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
            }
            else
            {
                logger.Info(reporting, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
            }
        }

        public static void Info<T0, T1>(this ILogger logger, Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Info(reporting, messageTemplate, propertyValue0, propertyValue1);
            }
            else
            {
                logger.Info(reporting, messageTemplate, propertyValue0, propertyValue1);
            }
        }

        public static void Info<T0>(this ILogger logger, Type reporting, string messageTemplate, T0 propertyValue0)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Info(reporting, messageTemplate, propertyValue0);
            }
            else
            {
                logger.Info(reporting, messageTemplate, propertyValue0);
            }
        }

        public static void Verbose<T0, T1, T2>(this ILogger logger, Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Verbose(reporting, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
            }
            else
            {
                logger.Verbose(reporting, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
            }
        }

        public static void Verbose<T0, T1>(this ILogger logger, Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Verbose(reporting, messageTemplate, propertyValue0, propertyValue1);
            }
            else
            {
                logger.Verbose(reporting, messageTemplate, propertyValue0, propertyValue1);
            }
        }

        public static void Verbose<T0>(this ILogger logger, Type reporting, string messageTemplate, T0 propertyValue0)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Verbose(reporting, messageTemplate, propertyValue0);
            }
            else
            {
                logger.Verbose(reporting, messageTemplate, propertyValue0);
            }
        }

        public static void Warn<T0, T1, T2>(this ILogger logger, Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Warn(reporting, messageTemplate, exception, propertyValue0, propertyValue1, propertyValue2);
            }
            else
            {
                logger.Warn(reporting, messageTemplate, exception, propertyValue0, propertyValue1, propertyValue2);
            }
        }

        public static void Warn<T0, T1>(this ILogger logger, Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Warn(reporting, messageTemplate, exception, propertyValue0, propertyValue1);
            }
            else
            {
                logger.Warn(reporting, messageTemplate, exception, propertyValue0, propertyValue1);
            }
        }

        public static void Warn<T0>(this ILogger logger, Type reporting, Exception exception, string messageTemplate, T0 propertyValue0)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Warn(reporting, messageTemplate, exception, propertyValue0);
            }
            else
            {
                logger.Warn(reporting, messageTemplate, exception, propertyValue0);
            }
        }

        public static void Warn<T0>(this ILogger logger, Type reporting, string message, T0 propertyValue0)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Warn(reporting, message, propertyValue0);
            }
            else
            {
                logger.Warn(reporting, message, propertyValue0);
            }
        }

       //
        public static void Error<T, T0>(this ILogger logger, Exception exception, string messageTemplate, T0 propertyValue0)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Error(typeof(T), exception, messageTemplate, propertyValue0);
            }
            else
            {
                logger.Error(typeof(T), exception, messageTemplate, propertyValue0);
            }
        }
        
        public static void Error<T, T0,T1>(this ILogger logger, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Error(typeof(T), exception, messageTemplate, propertyValue0, propertyValue1);
            }
            else
            {
                logger.Error(typeof(T), exception, messageTemplate, propertyValue0, propertyValue1);
            }
        }
        
        public static void Error<T, T0, T1,T2>(this ILogger logger, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Error(typeof(T), exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
            }
            else
            {
                logger.Error(typeof(T), exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
            }
        }
        public static void Error<T,T0>(this ILogger logger, string messageTemplate, T0 propertyValue0)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Error(typeof(T), messageTemplate, propertyValue0);
            }
            else
            {
                logger.Error(typeof(T), messageTemplate, propertyValue0);
            }
        }
        public static void Error<T, T0,T1>(this ILogger logger, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Error(typeof(T), messageTemplate, propertyValue0, propertyValue1);
            }
            else
            {
                logger.Error(typeof(T), messageTemplate, propertyValue0, propertyValue1);
            }
        }
        public static void Error<T, T0, T1,T2>(this ILogger logger, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Error(typeof(T), messageTemplate, propertyValue0, propertyValue1, propertyValue2);
            }
            else
            {
                logger.Error(typeof(T), messageTemplate, propertyValue0, propertyValue1, propertyValue2);
            }
        }
        public static void Warn<T, T0>(this ILogger logger, Exception exception, string messageTemplate, T0 propertyValue0)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Warn(typeof(T), exception, messageTemplate, propertyValue0);
            }
            else
            {
                logger.Warn(typeof(T), exception, messageTemplate, propertyValue0);
            }
        }

        public static void Warn<T, T0, T1>(this ILogger logger, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Warn(typeof(T), exception, messageTemplate, propertyValue0, propertyValue1);
            }
            else
            {
                logger.Warn(typeof(T), exception, messageTemplate, propertyValue0, propertyValue1);
            }
        }

        public static void Warn<T, T0, T1, T2>(this ILogger logger, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Warn(typeof(T), exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
            }
            else
            {
                logger.Warn(typeof(T), exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
            }
        }

        public static void Warn<T, T0>(this ILogger logger, string messageTemplate, T0 propertyValue0)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Warn(typeof(T), messageTemplate, propertyValue0);
            }
            else
            {
                logger.Warn(typeof(T), messageTemplate, propertyValue0);
            }
        }
        public static void Warn<T, T0, T1>(this ILogger logger, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Warn(typeof(T), messageTemplate, propertyValue0, propertyValue1);
            }
            else
            {
                logger.Warn(typeof(T), messageTemplate, propertyValue0, propertyValue1);
            }
        }
        public static void Warn<T, T0, T1, T2>(this ILogger logger, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Warn(typeof(T), messageTemplate, propertyValue0, propertyValue1, propertyValue2);
            }
            else
            {
                logger.Warn(typeof(T), messageTemplate, propertyValue0, propertyValue1, propertyValue2);
            }
        }

       
        public static void Info<T, T0>(this ILogger logger, string messageTemplate, T0 propertyValue0)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Info(typeof(T), messageTemplate, propertyValue0);
            }
            else
            {
                logger.Info(typeof(T), messageTemplate, propertyValue0);
            }
        }
        public static void Info<T, T0, T1>(this ILogger logger, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Info(typeof(T), messageTemplate, propertyValue0, propertyValue1);
            }
            else
            {
                logger.Info(typeof(T), messageTemplate, propertyValue0, propertyValue1);
            }
        }
        public static void Info<T, T0, T1, T2>(this ILogger logger, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Info(typeof(T), messageTemplate, propertyValue0, propertyValue1, propertyValue2);
            }
            else
            {
                logger.Info(typeof(T), messageTemplate, propertyValue0, propertyValue1, propertyValue2);
            }
        }
        public static void Debug<T, T0>(this ILogger logger, string messageTemplate, T0 propertyValue0)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Debug(typeof(T), messageTemplate, propertyValue0);
            }
            else
            {
                logger.Debug(typeof(T), messageTemplate, propertyValue0);
            }
        }
        public static void Debug<T, T0, T1>(this ILogger logger, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Debug(typeof(T), messageTemplate, propertyValue0, propertyValue1);
            }
            else
            {
                logger.Debug(typeof(T), messageTemplate, propertyValue0, propertyValue1);
            }
        }
        public static void Debug<T, T0, T1, T2>(this ILogger logger, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
        {
            if (logger is ILogger2 logger2)
            {
                logger2.Debug(typeof(T), messageTemplate, propertyValue0, propertyValue1, propertyValue2);
            }
            else
            {
                logger.Debug(typeof(T), messageTemplate, propertyValue0, propertyValue1, propertyValue2);
            }
        }
    }
}
