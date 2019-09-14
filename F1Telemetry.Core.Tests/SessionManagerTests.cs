using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using F1Telemetry.Core.SessionManagement;
using F1Telemetry.Core.SessionManagement.Events;
using NUnit.Framework;

namespace F1Telemetry.Core.Tests
{
    [TestFixture]
    public class SessionManagerTests
    {
        private class TestEventPublisher : IEventPublisher
        {

            public TestEventPublisher(IEnumerable<Event> events)
            {
                Events = events.ToObservable().Publish().RefCount();
            }
            public TestEventPublisher(IObservable<Event> events)
            {
                Events = events;
            }

            public IObservable<Event> Events { get; }
        }

        private static readonly IEnumerable<Event> TestEvents = new Event[]
        {
            new SessionStart(1, DateTime.Now, 0f),
            new SessionStart(2, DateTime.Now, 0f),
            new SessionEnd(2, DateTime.Now, 0.1f),
            new SessionEnd(1, DateTime.Now, 0.1f)
        };

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Creates_Single_Instance_Of_Session_When_Multiple_Subscribers_Connect()
        {
            var sm = new SessionManager(new TestEventPublisher(TestEvents));
            var sem = new CountdownEvent(3);
            sm.Sessions.Subscribe(s => { }, () => { sem.Signal(); });
            sm.Sessions.Subscribe(s => { }, () => { sem.Signal(); });
            sm.Sessions.Subscribe(s => { }, () => { sem.Signal(); });
            sem.Wait();
            Assert.AreEqual(2, sm.SessionCount);
        }

        [Test]
        public void Subscribers_See_All_Sessions_Started_After_SessionManager_Was_Created()
        {
            var subject = new Subject<Event>();
            var eventPublisher = new TestEventPublisher(subject.AsObservable());
            subject.OnNext(new SessionStart(1, DateTime.Now, 0f));
            var sm = new SessionManager(eventPublisher);
            subject.OnNext(new SessionStart(2, DateTime.Now, 0f));
            subject.OnNext(new SessionStart(3, DateTime.Now, 0f));

            var finalObservable = sm.Sessions.ToListObservable();
            subject.OnCompleted();
            Assert.AreEqual(2, finalObservable.Count, "Expected 2 sessions to be published");
            Assert.AreEqual(2, finalObservable[0].SessionId, "Expected sessions 2 to be published");
            Assert.AreEqual(3, finalObservable[1].SessionId, "Expected sessions 2 to be published");
        }

        [Test]
        public async Task Session_Replays_Last_Event_Of_Each_Type()
        {
            const int sessionId = 1;
            var events = new Event[]
            {
                new SessionStart(sessionId, DateTime.Now, 0f),
                new WeatherInfo(Weather.Clear, 22, 23, sessionId, DateTime.Now, 1f), 
                new WeatherInfo(Weather.HeavyRain, 22, 23, sessionId, DateTime.Now, 2f), 
                new SessionTimeLeft(TimeSpan.FromSeconds(120), sessionId, DateTime.Now, 5f), 
                new SessionTimeLeft(TimeSpan.FromSeconds(50), sessionId, DateTime.Now, 5f), 
                new SessionDuration(TimeSpan.FromHours(2), sessionId, DateTime.Now, 5f), 
                new SessionDuration(TimeSpan.FromHours(3), sessionId, DateTime.Now, 5f), 
                new SessionPause(sessionId, DateTime.Now, 6f), 
                new SessionResume(sessionId, DateTime.Now, 7f), 
                new SessionPause(sessionId, DateTime.Now, 7.1f), 
                new SessionResume(sessionId, DateTime.Now, 8f), 
                new TrackInfo(Track.AbuDhabi, 5555, sessionId, DateTime.Now, 8.1f), 
                new SessionEnd(sessionId, DateTime.Now, 50f), 
            };
            var observable = events.ToObservable().Publish();
            var sm = new SessionManager(new TestEventPublisher(observable));
            observable.Connect();

            await sm.Sessions.Select(s => s.LifecycleEvents.OfType<SessionEnd>()).LastAsync();

            ListObservable<WeatherInfo> wi = null;
            ListObservable<SessionTimeLeft> stl = null;
            ListObservable<SessionDuration> sd = null;
            ListObservable<TrackInfo> ti = null;
            ListObservable<SessionLifecycle> le = null;

            sm.Sessions.Subscribe(
                s =>
                {
                    wi = s.WeatherEvents.ToListObservable();
                    ti = s.TrackInfoEvents.ToListObservable();
                    sd = s.DurationEvents.ToListObservable();
                    stl = s.TimeLeftEvents.ToListObservable();
                    le = s.LifecycleEvents.ToListObservable();
                },
                ex => Assert.Fail("OnError {0}", ex));

            Assert.AreEqual(1, wi.Count);
            Assert.AreEqual(1, ti.Count);
            Assert.AreEqual(1, sd.Count);
            Assert.AreEqual(1, stl.Count);
            Assert.AreEqual(3, le.Count);
            Assert.AreEqual(Weather.HeavyRain, wi.Value.Weather);
            Assert.AreEqual(TimeSpan.FromSeconds(50), stl.Value.TimeLeft);
            Assert.AreEqual(TimeSpan.FromHours(3), sd.Value.Duration);
            Assert.AreEqual(Track.AbuDhabi, ti.Value.Track);
            Assert.IsInstanceOf<SessionStart>(le[0]);
            Assert.IsInstanceOf<SessionResume>(le[1]);
            Assert.IsInstanceOf<SessionEnd>(le[2]);
        }

        [Test]
        public void Session_Publishes_Only_Distinct_Events()
        {
            const int sessionId = 1;
            var events = new Event[]
            {
                new SessionStart(sessionId, DateTime.Now, 0f),
                new WeatherInfo(Weather.Clear, 22, 23, sessionId, DateTime.Now, 1f), 
                new WeatherInfo(Weather.Clear, 22, 23, sessionId, DateTime.Now, 2f), 
                new SessionTimeLeft(TimeSpan.FromSeconds(120), sessionId, DateTime.Now, 3f), 
                new SessionTimeLeft(TimeSpan.FromSeconds(120), sessionId, DateTime.Now, 4f), 
                new SessionDuration(TimeSpan.FromHours(2), sessionId, DateTime.Now, 5f), 
                new SessionDuration(TimeSpan.FromHours(2), sessionId, DateTime.Now, 6f), 
                new SessionPause(sessionId, DateTime.Now, 6f), 
                new SessionPause(sessionId, DateTime.Now, 7f), 
                new SessionResume(sessionId, DateTime.Now, 8f), 
                new SessionResume(sessionId, DateTime.Now, 9f), 
                new TrackInfo(Track.AbuDhabi, 5555, sessionId, DateTime.Now, 10f),
                new TrackInfo(Track.AbuDhabi, 5555, sessionId, DateTime.Now, 11f),
                new SessionEnd(sessionId, DateTime.Now, 50f), 
            };
            var observable = events.ToObservable().Publish();
            var sm = new SessionManager(new TestEventPublisher(observable));

            ListObservable<WeatherInfo> wi = null;
            ListObservable<SessionTimeLeft> stl = null;
            ListObservable<SessionDuration> sd = null;
            ListObservable<TrackInfo> ti = null;
            ListObservable<SessionLifecycle> le = null;

            var completed = new AutoResetEvent(false);
            sm.Sessions.Subscribe(
                s =>
                {
                    wi = s.WeatherEvents.ToListObservable();
                    ti = s.TrackInfoEvents.ToListObservable();
                    sd = s.DurationEvents.ToListObservable();
                    stl = s.TimeLeftEvents.ToListObservable();
                    le = s.LifecycleEvents.ToListObservable();
                },
                ex => Assert.Fail("OnError {0}", ex),
                () => completed.Set());

            observable.Connect();
            completed.WaitOne();

            Assert.AreEqual(1, wi.Count);
            Assert.AreEqual(1, ti.Count);
            Assert.AreEqual(1, sd.Count);
            Assert.AreEqual(1, stl.Count);
            Assert.AreEqual(4, le.Count);
        }
    }
}