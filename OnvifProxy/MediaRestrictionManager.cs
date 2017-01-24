using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Timers;

namespace OnvifProxy
{
    public static class MediaRestrictionManager
    {
        public static ConcurrentBag<MediaRestrictions.RestrictionType> RestrictionCollection;
        public static void AddRestriction(MediaRestrictions.RestrictionType incomingRestriction)
        {
            //check if already exist
            foreach (MediaRestrictions.RestrictionType restrictionFromCollection in RestrictionCollection)
            {
                if (incomingRestriction == restrictionFromCollection)
                    throw new ApplicationException();
            }
            RestrictionCollection.Add(incomingRestriction);
        }

        public static void RemoveRestriction(MediaRestrictions.RestrictionType incomingRestriction)
        {
            foreach(MediaRestrictions.RestrictionType restrictionFromCollection in RestrictionCollection )
            {
                if (incomingRestriction == restrictionFromCollection)
                    RestrictionCollection.TryTake(out incomingRestriction);
            }
        }
    }

    public class Restriction
    {
        private const int RESTRICTION_TIMEOUT = 0;

        public MediaRestrictions.RestrictionType restriction;
        System.Timers.Timer restrictionTimer;
        Restriction(MediaRestrictions.RestrictionType restriction)
        {
            //calculate timer settings from restriction begin/end time
            //setup timer
            restrictionTimer = new System.Timers.Timer(RESTRICTION_TIMEOUT);
            restrictionTimer.Elapsed += new ElapsedEventHandler(OnRestrictionTimerTimeout);
            restrictionTimer.Enabled = true;
            restrictionTimer.AutoReset = false;
        }

        void OnRestrictionTimerTimeout(object source, ElapsedEventArgs e)
        {
            MediaRestrictionManager.RemoveRestriction(restriction);
        }
    }

    //delegate void OnRestrictionTimerTimeout();
}
