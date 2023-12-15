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
using XF.Material.Forms.UI;
using XF.Material.Forms.UI.Internals;
using Button = Xamarin.Forms.Button;

namespace FormGenerator
{
    public partial class MainPage : TabbedPage
    {
        private Forms _forms;
        private Forms _defaultForms;
        private Button _resetButton, _submitButton;
        public MainPage(BluetoothDevice device)
        {
            InitializeComponent();
            Address.Text = $"{device.Name}: {device.Address}";
            
            var r = Task.Run(GetFormsFromJson);
            r.Wait();
            if (r.Exception == null && r.IsCompleted && r.Status == TaskStatus.RanToCompletion && r.Result != null && r.Result.forms.Count > 0)
            {
                _forms = r.Result;
                var defaultFormsJson = JsonConvert.SerializeObject(_forms);
                _defaultForms = JsonConvert.DeserializeObject<Forms>(defaultFormsJson);
                CreateFormsFromJson();
            }
            else
            {
                DisplayAlert("Error", "Something goes wrong", "OK");
            }
        }

        private async Task<Forms> GetFormsFromJson()
        {
            try
            {
                var json = Encoding.UTF8.GetString(await JsonService.BluetoothInput());
                var forms = JsonConvert.DeserializeObject<Forms>(json);
                return forms;
            }
            catch (Exception ex)
            {
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


                var activityIndicator = new ActivityIndicator
                {
                    IsRunning = true,
                    IsVisible = false,
                    Color = Color.FromHex("#5C4798"),
                };
                var resultMessage = new Label
                {
                    Text = "Saved and Sent",
                    TextColor = Color.Green,
                    FontSize = 10,
                    HorizontalOptions = LayoutOptions.Center,
                    IsVisible = false
                };
                foreach (var member in form.Members)
                {
                    switch (member.Type)
                    {
                        case "text":

                            var entry = new MaterialTextField
                            {
                                Placeholder = member.Label,
                                InputType = MaterialTextFieldInputType.Default,
                                BackgroundColor = Color.Transparent, 
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
                                ThumbColor = Color.FromHex("#5C4798"),
                                OnColor = Color.FromHex("#7d6ab2"),
                            };

                            sw.Toggled += (sender, e) =>
                            {
                                OnChanged(sender, e, member, activityIndicator, resultMessage);
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
                stackLayout.Children.Add(activityIndicator);
                stackLayout.Children.Add(resultMessage);
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

                            _submitButton  = new Xamarin.Forms.Button
                            {
                                Text = "Save",
                                TextColor = Color.White,
                                BackgroundColor = Color.Green,
                                CornerRadius = 5,
                                TextTransform = TextTransform.None
                            };

                            _submitButton.Clicked += (sender, e) =>
                            {
                                SaveAction(sender, e, activityIndicator,resultMessage);
                            };
                            sl1.Children.Add(_submitButton);

                            break;

                        case "reset":

                            _resetButton = new Xamarin.Forms.Button
                            {
                                Text = "Reset",
                                TextColor = Color.White,
                                BackgroundColor = Color.OrangeRed,
                                CornerRadius = 5,
                                TextTransform = TextTransform.None,
                            };

                            _resetButton.Clicked += ResetAction;

                            sl1.Children.Add(_resetButton);

                            break;
                    }
                }
                stackLayout.Children.Add(sl1);
            }
        }


        private void OnChanged(object sender,EventArgs e,Member member, ActivityIndicator indicator = null, Label resultMessage = null)
        {
            switch (sender)
            {
                case MaterialTextField entry:
                {
                    var txt = entry.Text;
                    member.Value = txt;
                    break;
                }
                case Switch sw:
                {
                    var status = sw.IsToggled;
                    member.Set = status;
                    if((bool)member.Autosend) SaveAction(sender,e, indicator, resultMessage);
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
        
        private async void SaveAction(object sender,EventArgs e, ActivityIndicator indicator, Label resultMessage)
        {
            resultMessage.IsVisible = false;
            if(!JsonService._bluetoothAdapter.IsEnabled)
            {
                var alert = await DisplayAlert("Error", "Bluetooth is disabled", "Enable bluetooth", "Cancel");
                if(alert) JsonService.OpenBluetoothSettings();
                return;
            }
            indicator.IsVisible = true;
            _resetButton.IsVisible = false;
            
            _submitButton.IsVisible = false;
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var json = JsonConvert.SerializeObject(_forms, serializerSettings);
                
            var r = await JsonService.BluetoothCommand(json);
            if (r)
            {
                resultMessage.Text = "Saved and Sent";
                resultMessage.TextColor = Color.Green;
            }
            else
            {
                resultMessage.Text = "Something goes wrong";
                resultMessage.TextColor = Color.Red;
            }
            indicator.IsVisible = false;
            _submitButton.IsVisible = true;
            _resetButton.IsVisible = true;
            resultMessage.IsVisible = true;
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