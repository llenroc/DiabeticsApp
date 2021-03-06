﻿using System;
using Android.App;
using Android.Content;
using Android.Media;
using Android.Support.V4.App;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using Android.Widget;
using Diabetes.localDB;
using Xamarin.Forms;

namespace Diabetes.Droid
{
    [BroadcastReceiver]
    [IntentFilter(new string[] { "android.intent.action.BOOT_COMPLETED" }, Priority = (int)IntentFilterPriority.LowPriority)]
    public class AlarmReceiver : BroadcastReceiver
    {

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action.Equals("android.intent.action.BOOT_COMPLETED"))
			{
            var message = intent.GetStringExtra("message");
            var title = intent.GetStringExtra("title");



            //Show toast here
            //Toast.MakeText(context, "Hello it's me ", ToastLength.Short).Show();
            var extras = intent.Extras;

            if (extras != null && !extras.IsEmpty)
            {
                NotificationManager manager_ = context.GetSystemService(Context.NotificationService) as NotificationManager;
                var notificationId = extras.GetInt("NotificationIdKey", -1);
                if (notificationId != -1)
                {
                    manager_.Cancel(notificationId);
                }
            }

            //Create intent for action 1 (TAKE)
            var actionIntent1 = new Intent();
            actionIntent1.SetAction("ARCHIVE");
            var pIntent1 = PendingIntent.GetBroadcast(context, 0, actionIntent1, PendingIntentFlags.CancelCurrent);

            //Create intent for action 2 (REPLY)
            var actionIntent2 = new Intent();
            actionIntent2.SetAction("REPLY");
            var pIntent2 = PendingIntent.GetBroadcast(context, 0, actionIntent2, PendingIntentFlags.CancelCurrent);

            Intent resultIntent = context.PackageManager.GetLaunchIntentForPackage(context.PackageName);
      
            var contentIntent = PendingIntent.GetActivity(context, 0, resultIntent, PendingIntentFlags.CancelCurrent);

            var pending = PendingIntent.GetActivity(context, 0,
                resultIntent,
                PendingIntentFlags.CancelCurrent);
            //seting an alarm
            MedicationDatabase db = new MedicationDatabase();
            var alarm_list = db.GetAlarmList();
            //Debug.WriteLine(" Time -- : "+ m.ToString());
            var title_ = "Diabetics App";
            var message_ = "\"Hello  we remind you of taking your inslin dosage now";
            foreach (var list in alarm_list)
            {
                var hour = Int32.Parse(list.Substring(0, 2));
                var minute = Int32.Parse(list.Substring(3, 2));
                Intent myintent = new Intent(Xamarin.Forms.Forms.Context, typeof(AlarmReceiver));
                myintent.PutExtra("message", message_);
                myintent.PutExtra("title", title_);
                PendingIntent pendingintent = PendingIntent.GetBroadcast(Xamarin.Forms.Forms.Context, 0, myintent, PendingIntentFlags.UpdateCurrent);

                Java.Util.Date date = new Java.Util.Date();
                Java.Util.Calendar cal = Java.Util.Calendar.Instance;
                cal.TimeInMillis = Java.Lang.JavaSystem.CurrentTimeMillis();
                cal.Set(Java.Util.CalendarField.HourOfDay, hour);
                cal.Set(Java.Util.CalendarField.Minute, minute);
                cal.Set(Java.Util.CalendarField.Second, 0);
                AlarmManager alarmManager = Xamarin.Forms.Forms.Context.GetSystemService(Android.Content.Context.AlarmService) as AlarmManager;
                alarmManager.Set(AlarmType.RtcWakeup, cal.TimeInMillis, pendingintent);
            }

            // Instantiate the Big Text style:
            Notification.BigTextStyle textStyle = new Notification.BigTextStyle();


            var builder =
                new Notification.Builder(context)
                                .AddAction(Resource.Drawable.tick_notify, "ARCHIVE", pIntent1)
                                .AddAction(Resource.Drawable.cancel_notify, "REPLY", pIntent2)
                                .SetSmallIcon(Resource.Drawable.ic_launcher)
                                .SetContentTitle("Diabetics Reminder")
                                .SetDefaults(NotificationDefaults.Sound)
                                .SetStyle(new Notification
                                .BigTextStyle()
                                .SetSummaryText("")
                                .SetBigContentTitle(title)
                                .BigText(message)
             ).SetDefaults(NotificationDefaults.All);

            builder.SetContentIntent(pending);

            var notification = builder.Build();


            var manager = NotificationManager.FromContext(context);
            manager.Notify(10010, notification);
        }
        }
    }

    [BroadcastReceiver]
    [IntentFilter(new string[] { "ARCHIVE", "REPLY" })]
    public class CustomActionReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {

            switch (intent.Action)
            {
                case "ARCHIVE":
                    try
                    {
                        MedicationDatabase db = new MedicationDatabase();
                        db.addtracktaken("true");
                        Toast.MakeText(context, "DOSAGE TAKEN", ToastLength.Short).Show();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.StackTrace);
                    }
                    break;
                case "REPLY":
                    try
                    {
                        Toast.MakeText(context, "ARCHIVE", ToastLength.Short).Show();
                        MedicationDatabase db = new MedicationDatabase();
						db.addtrackmissed("true");
                        Toast.MakeText(context, "DOSAGE MISSED", ToastLength.Short).Show();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.StackTrace);
                    }
                    break;
            }

            var extras = intent.Extras;

            if (extras != null && !extras.IsEmpty)
            {
                NotificationManager manager = context.GetSystemService(Context.NotificationService) as NotificationManager;
                var notificationId = extras.GetInt("NotificationIdKey", -1);
                if (notificationId != -1)
                {
                    manager.Cancel(notificationId);
                }
            }
        }
    }
}