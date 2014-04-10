using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;


namespace Redis.SilverlightTestApp
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Redis
                .SilverlightClient.RedisSubscriber.SubscribeToChannel("127.0.0.1", 4525, "test-alert", Scheduler.Default)
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(message =>
                    {
                        listBoxAlerts.Items.Add(new ListBoxItem() { Content = message.Content });
                    },
                    ex =>
                    {
                        MessageBox.Show(ex.ToString());
                    });
        }
    }
}
