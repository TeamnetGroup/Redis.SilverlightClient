using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

using Redis.SilverlightClient;
using System.Threading.Tasks;

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

        void buttonSendMessage_Click(object sender, RoutedEventArgs e)
        {
            var publisher = new RedisPublisher("127.0.0.1", 4525, TaskPoolScheduler.Default);
            publisher.PublishMessage("test-alert", textBoxMessage.Text);
        }

        void MainPageLoaded(object sender, RoutedEventArgs e)
        {
            RedisSubscriber.SubscribeToChannel("127.0.0.1", 4525, "test-alert")
                .ObserveOn(SynchronizationContext.Current)
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
