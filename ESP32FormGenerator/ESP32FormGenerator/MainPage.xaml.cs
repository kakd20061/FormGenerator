using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Bluetooth;
using ESP32FormGenerator.Models;
using ESP32FormGenerator.Services;
using Newtonsoft.Json;
using Plugin.BLE.Abstractions.Contracts;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ESP32FormGenerator
{
    public partial class MainPage : TabbedPage
    {
        public MainPage(BluetoothDevice device)
        {
            InitializeComponent();
            esp32Address.Text = $"{device.Name}: {device.Address}";
            var t = Task.Run(() => CreateFormsFromJson());
            t.Wait();
        }
        public async Task CreateFormsFromJson()
        {
            var json = Encoding.UTF8.GetString(await JsonService.BluetoothInput());
            var forms = JsonConvert.DeserializeObject<Forms>(json);
            foreach (var form in forms.forms)
            {
                // Create the first tapped page
                var tappedPage = new ContentPage
                {
                    Title = form.Title,
                    Padding = new Thickness(20)
                };
                var scrollView = new ScrollView();

                var stackLayout = new StackLayout
                {

                };
                scrollView.Content = stackLayout;
                tappedPage.Content = scrollView;
                Children.Add(tappedPage);



                foreach (var member in form.Members)
                {
                    switch (member.Type)
                    {
                        case "text":
                            var entry = new Entry
                            {
                                Placeholder = member.Label,
                                Text = member.Value.ToString(),
                                Margin = new Thickness(0, 20),
                            };

                            stackLayout.Children.Add(entry);


                            break;
                        case "binswitch":
                            var sl = new StackLayout
                            {
                                Orientation = StackOrientation.Horizontal,
                                Margin = new Thickness(0, 20)
                            };
                            var label = new Label
                            {
                                Text = member.Label,
                                VerticalOptions = LayoutOptions.Center,
                                TextColor = Color.Black,
                                FontSize = 20
                            };
                            var sw = new Switch
                            {
                                IsToggled = (bool)member.Set,
                                VerticalOptions = LayoutOptions.Center,
                                ThumbColor = Color.DarkGreen,
                                OnColor = Color.Green,
                            };

                            sl.Children.Add(label);
                            sl.Children.Add(sw);

                            stackLayout.Children.Add(sl);

                            break;

                        case "select":
                            var test = member.Value;
                            var values = JsonConvert.DeserializeObject<ItemsList>("{" + $"value:{member.Value}" + "}");

                            var picker = new Picker
                            {
                                Title = member.Label,
                                Margin = new Thickness(0, 20)
                            };

                            picker.ItemsSource = values.Value.Select(n => n.Label).ToList();
                            stackLayout.Children.Add(picker);

                            break;

                        case "password":
                            var password = new Entry
                            {
                                IsPassword = true,
                                Placeholder = member.Label,
                                Text = member.Value.ToString(),
                                Margin = new Thickness(0, 20)
                            };

                            stackLayout.Children.Add(password);

                            break;


                        case "label":
                            var label1 = new Label
                            {
                                FormattedText = new FormattedString
                                {
                                    Spans = {
                                        new Span
                                        {
                                            Text = $"{member.Value}: ",
                                            FontSize = 20,
                                            ForegroundColor = Color.Black
                                        },
                                        new Span
                                        {
                                            Text = member.Value.ToString(),
                                            FontSize = 20,
                                            ForegroundColor = Color.Gray
                                        }
                                    }
                                },
                                Margin = new Thickness(0, 20)
                            };

                            stackLayout.Children.Add(label1);

                            break;

                        case "state":
                            var label2 = new Label
                            {
                                FormattedText = new FormattedString
                                {
                                    Spans = {
                                        new Span
                                        {
                                            Text = $"{member.Label}: ",
                                            FontSize = 20,
                                            ForegroundColor = Color.Black
                                        },
                                        new Span
                                        {
                                            Text = member.Value.ToString(),
                                            FontSize = 20,
                                            ForegroundColor = bool.Parse(member.Value.ToString()) == true ? Color.Green : Color.Red
                                        }
                                    }
                                },
                                Margin = new Thickness(0, 20)
                            };

                            stackLayout.Children.Add(label2);

                            break;

                        default:

                            break;
                    }
                }
                var sl1 = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 20)
                };
                foreach (var btn in form.Button)
                {
                    switch (btn.Type)
                    {
                        case "save":

                            var button = new Xamarin.Forms.Button
                            {
                                Text = "Save",
                                TextColor = Color.White,
                                BackgroundColor = Color.Green,
                                CornerRadius = 5,
                                TextTransform = TextTransform.None
                            };
                            sl1.Children.Add(button);

                            break;

                        case "reset":

                            var button1 = new Xamarin.Forms.Button
                            {
                                Text = "Reset",
                                TextColor = Color.White,
                                BackgroundColor = Color.OrangeRed,
                                CornerRadius = 5,
                                TextTransform = TextTransform.None,
                            };
                            sl1.Children.Add(button1);

                            break;
                        default:

                            break;
                    }
                }
                stackLayout.Children.Add(sl1);
            }
        }
    }
}