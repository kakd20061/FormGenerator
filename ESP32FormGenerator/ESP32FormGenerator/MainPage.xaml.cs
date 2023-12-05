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
using Xamarin.Essentials;
using Xamarin.Forms;
using Newtonsoft;
using Newtonsoft.Json.Serialization;

namespace ESP32FormGenerator
{
    public partial class MainPage : TabbedPage
    {
        private Forms _forms;
        private Forms _defaultForms;
        public MainPage(BluetoothDevice device)
        {
            InitializeComponent();
            esp32Address.Text = $"{device.Name}: {device.Address}";
            
            var r = Task.Run(GetFormsFromJson);
            r.Wait();
            if (r.Exception == null && r.IsCompleted && r.Status == TaskStatus.RanToCompletion && r.Result != null && r.Result.forms.Count > 0)
            {
                _forms = r.Result;
                var defaultFormsJson = JsonConvert.SerializeObject(_forms);
                _defaultForms = JsonConvert.DeserializeObject<Forms>(defaultFormsJson);
                CreateFormsFromJson();
            }
        }

        private async Task<Forms> GetFormsFromJson()
        {
            try
            {
                var json = Encoding.UTF8.GetString(await JsonService.BluetoothInput());
                // const string testingjson = "\n{\n  \"forms\": [\n    {\n      \"name\": \"test_form\",\n      \"title\": \"Formularz Testowy\",\n\n      \"members\": [\n        {\n          \"type\": \"label\",\n          \"name\": \"temperature\",\n          \"value\": \"25C\"\n        },\n        {\n          \"type\": \"text\",\n          \"label\": \"Imię\",\n          \"name\": \"name\",\n          \"value\": \"Jan\"\n        },\n        {\n          \"type\": \"password\",\n          \"label\": \"Hasło\",\n          \"name\": \"password\",\n          \"value\": \"\"\n        },\n        {\n          \"type\": \"state\",\n          \"label\": \"Zasilanie\",\n          \"name\": \"power_supply\",\n          \"value\": true\n        },\n        {\n          \"type\": \"binswitch\",\n          \"label\": \"Ogrzewanie\",\n          \"name\": \"heating\",\n          \"set\": false,\n          \"autosend\": true\n        },\n        {\n          \"type\": \"select\",\n          \"label\": \"Sieci WiFi\",\n          \"name\": \"ssid\",\n          \"set\": 1,\n          \"value\": [\n            {\n              \"value\": \"10\",\n              \"label\": \"opcja 1\"\n            },\n            {\n              \"value\": \"12\",\n              \"label\": \"opcja 2\"\n            },\n            {\n              \"value\": \"12\",\n              \"label\": \"opcja 3\"\n            }\n          ]\n        }\n      ],\n      \"button\": [\n        {\n          \"type\": \"save\",\n          \"label\": \"Zapisz\",\n          \"name\": \"save\"\n        },\n        {\n          \"type\": \"reset\",\n          \"label\": \"Resetuj\",\n          \"name\": \"reset\"\n        }\n      ]\n    },\n\n        {\n      \"name\": \"test_form\",\n      \"title\": \"Formularz Testowy2\",\n\n      \"members\": [\n        {\n          \"type\": \"label\",\n          \"name\": \"temperature\",\n          \"value\": \"25C\"\n        },\n        {\n          \"type\": \"text\",\n          \"label\": \"Imię\",\n          \"name\": \"name\",\n          \"value\": \"Edyta\"\n        },\n        {\n          \"type\": \"password\",\n          \"label\": \"Hasło\",\n          \"name\": \"password\",\n          \"value\": \"\"\n        },\n        {\n          \"type\": \"state\",\n          \"label\": \"Zasilanie\",\n          \"name\": \"power_supply\",\n          \"value\": true\n        },\n        {\n          \"type\": \"binswitch\",\n          \"label\": \"Ogrzewanie\",\n          \"name\": \"heating\",\n          \"set\": false,\n          \"autosend\": true\n        },\n        {\n          \"type\": \"select\",\n          \"label\": \"Sieci WiFi\",\n          \"name\": \"ssid\",\n          \"set\": 1,\n          \"value\": [\n            {\n              \"value\": \"10\",\n              \"label\": \"opcja 1\"\n            },\n            {\n              \"value\": \"12\",\n              \"label\": \"opcja 2\"\n            },\n            {\n              \"value\": \"12\",\n              \"label\": \"opcja 3\"\n            }\n          ]\n        }\n      ],\n      \"button\": [\n        {\n          \"type\": \"save\",\n          \"label\": \"Zapisz\",\n          \"name\": \"save\"\n        },\n        {\n          \"type\": \"reset\",\n          \"label\": \"Resetuj\",\n          \"name\": \"reset\"\n        }\n      ]\n    }\n  ]\n}\n\n";
                var forms = JsonConvert.DeserializeObject<Forms>(json);
                return forms;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error","Cannot get forms from ESP32","OK");
                return new Forms(new List<Form>());
            }
        }

        private void CreateFormsFromJson()
        {
            foreach (var form in _forms.forms)
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

                            entry.TextChanged += (sender, e) =>
                            {
                                OnChanged(sender, e, member);
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

                            sw.Toggled += (sender, e) =>
                            {
                                OnChanged(sender, e, member);
                            };

                            sl.Children.Add(label);
                            sl.Children.Add(sw);

                            stackLayout.Children.Add(sl);

                            break;

                        case "select":
                            var values = JsonConvert.DeserializeObject<ItemsList>("{" + $"value:{member.Value}" + "}");

                            var picker = new Picker
                            {
                                Title = member.Label,
                                Margin = new Thickness(0, 20)
                            };

                            picker.ItemsSource = values.Value.Select(n => n.Label).ToList();

                            picker.SelectedIndexChanged += (sender, e) =>
                            {
                                OnChanged(sender, e, member);
                            };

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

                            password.TextChanged += (sender, e) =>
                            {
                                OnChanged(sender, e, member);
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

                            button.Clicked += SaveAction;

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

                            button1.Clicked += ResetAction;

                            sl1.Children.Add(button1);

                            break;
                    }
                }
                stackLayout.Children.Add(sl1);
            }
        }


        private void OnChanged(object sender,EventArgs e,Member member)
        {
            switch (sender)
            {
                case Entry entry:
                {
                    var txt = entry.Text;
                    member.Value = txt;
                    break;
                }
                case Switch sw:
                {
                    var status = sw.IsToggled;
                    member.Set = status;
                    if((bool)member.Autosend) SaveAction(sender,e);
                    break;
                }
                case Picker picker:
                {
                    var index = picker.SelectedIndex;
                    member.Set = index;
                    break;
                }
            } 
        }
        
        private async void SaveAction(object sender,EventArgs e)
        {
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var json = JsonConvert.SerializeObject(_forms, serializerSettings);
                
            var r =await JsonService.BluetoothCommand(json);
            if(r) await DisplayAlert("ESP32", "Saved and Sent", "OK");
            else await DisplayAlert("ESP32", "Something goes wrong", "OK");
        }
        private void ResetAction(object sender, EventArgs e)
        {
            var defaultFormsJson = JsonConvert.SerializeObject(_defaultForms);
            _forms = JsonConvert.DeserializeObject<Forms>(defaultFormsJson);
            Children.Clear();
            CreateFormsFromJson();
        }

    }
}