using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Web.Caching;


namespace PXWeb.Management
{
    public class LogFlusher
    {
        private List<string> _times;
        private static log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(LogFlusher));
        public void InitializeSchedualFlush()
        {
            _times = new List<string>();

            char[] separators = new char[] { ',', ';' }; // Comma and semicolon are allowed as separators
                                                         // string times = "13:30";
            string times = PXWeb.Settings.Current.Features.Api.LogFlush;
            string[] parts = times.Split(separators);

            Regex checktime = new Regex(@"^(?:0?[0-9]|1[0-9]|2[0-3]):[0-5][0-9]$");
            foreach (var time in parts)
            {
                if (checktime.IsMatch(time))
                {
                    _times.Add(time);
                }
            }

            _times.Sort((x, y) => string.Compare(x, y));
            SchedualNextFlush();
        }
        private void SchedualNextFlush()
        {
            DateTime now = DateTime.Now;
            DateTime? t = null;
            foreach (var time in _times)
            {
                DateTime dt = DateTime.ParseExact(time, "HH:mm", CultureInfo.InvariantCulture);
                if ((dt - now).Ticks > 0)
                {
                    t = dt;
                    break;
                }
            }

            if (!t.HasValue && _times.Count > 0)
            {
                t = DateTime.ParseExact(_times[0], "HH:mm", CultureInfo.InvariantCulture);
            }

            if (t.HasValue)
            {
                if (now > t.Value)
                {
                    t = t.Value.AddDays(1);
                }
                HttpRuntime.Cache.Add(Guid.NewGuid().ToString(), t.Value, null, t.Value, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, new CacheItemRemovedCallback(FlushBuffers));

            }
        }

        public void FlushBuffers(String k, Object v, CacheItemRemovedReason r)
        {
            log4net.Repository.ILoggerRepository rep = log4net.LogManager.GetRepository();
            foreach (log4net.Appender.IAppender appender in rep.GetAppenders())
            {
                var buffered = appender as log4net.Appender.BufferingAppenderSkeleton;
                if (buffered != null)
                {
                    buffered.Flush();
                    _logger.Info("Logbuffer flushed for " + buffered.ToString());
                }
            }
        }
    }
}