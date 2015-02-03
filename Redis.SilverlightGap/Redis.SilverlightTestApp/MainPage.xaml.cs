using Redis.SilverlightClient;
using Redis.SilverlightClient.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Redis.SilverlightTestApp
{
    public partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            this.Loaded += this.MainPageLoaded;
            this.buttonSendMessage.Click += buttonSendMessage_Click;
        }

        async void buttonSendMessage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var connection = new SocketConnection("127.0.0.1", 4525, Scheduler.Default))
                {
                    var publisher = connection.AsPublisher();

                    await publisher.PublishMessage("alert1", textBoxMessage.Text);
                    //await publisher.PublishMessage("alert2", textBoxMessage.Text);
                }

                using (var connection = new SocketConnection("127.0.0.1", 4525, Scheduler.Default))
                {
                    var cacheClient = connection.AsCacheClient();

                    //await cacheClient.SetValue("alert1", textBoxMessage.Text);
                    var result  = await cacheClient.GetValues(new string[]{"alert1", "alert3"});
                    await cacheClient.Del("alert1");
                    var result2 = await cacheClient.GetValues(new string[] { "alert1", "alert3" });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        void MainPageLoaded(object sender, RoutedEventArgs e)
        {
            Subscribe();
        }

        async void Subscribe()
        {
            var connection = new SocketConnection("127.0.0.1", 4525, Scheduler.Default);
            var currentSyncronizationContext = SynchronizationContext.Current;
            var subscriber = connection.AsSubscriber();

            try
            {
                var channelMessagesReceived = 0;
                var channelsSubscription = await subscriber.Subscribe("alert1");
                channelsSubscription
                    .Buffer(TimeSpan.FromSeconds(1))
                    .ObserveOn(currentSyncronizationContext)
                    .Subscribe(messages =>
                    {
                        channelMessagesReceived += messages.Count;
                        channelMessages.Text = channelMessagesReceived.ToString();
                        //listBoxAlerts.Items.Add(new ListBoxItem { Content = message.ChannelName + ":" + message.Content });
                    },
                    ex =>
                    {
                        MessageBox.Show(ex.ToString());
                    });

                var patternChannelMessagesReceived = 0;
                var channelsPatternSubscription = await subscriber.PSubscribe("alert*");
                channelsPatternSubscription
                    .Buffer(TimeSpan.FromSeconds(1))
                    .ObserveOn(currentSyncronizationContext)
                    .Subscribe(messages =>
                    {
                        patternChannelMessagesReceived += messages.Count;
                        channelPatternMessages.Text = patternChannelMessagesReceived.ToString();
                        //listBoxAlerts.Items.Add(new ListBoxItem { Content = message.Pattern + ":" + message.Content });
                    },
                    ex =>
                    {
                        MessageBox.Show(ex.ToString());
                    });
            }
            catch (AggregateException ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
