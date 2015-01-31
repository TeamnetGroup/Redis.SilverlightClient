using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

using Redis.SilverlightClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using Redis.SilverlightClient.Sockets;

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
                var connection = new SocketConnection("127.0.0.1", 4525, TaskPoolScheduler.Default);
                await connection.AsPublisher().PublishMessage("test-alert", textBoxMessage.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        void MainPageLoaded(object sender, RoutedEventArgs e)
        {
            var connection = new SocketConnection("127.0.0.1", 4525, TaskPoolScheduler.Default);
            var currentSyncronizationContext = SynchronizationContext.Current;

            connection.AsSubscriber()
                .Subscribe("test-alert")
                .ObserveOn(currentSyncronizationContext)
                .Subscribe(message =>
                 {
                    listBoxAlerts.Items.Add(new ListBoxItem { Content = message.Content });
                 },
                 ex =>
                 {
                      MessageBox.Show(ex.ToString());
                 });
        }
    }
}
