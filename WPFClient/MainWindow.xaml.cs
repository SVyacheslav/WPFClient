using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.AspNetCore.SignalR.Client;


namespace WPFClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<Assignment> assignments = new ObservableCollection<Assignment>(); //  Список заданий
        HubConnection connection;

        public MainWindow()
        {
            InitializeComponent();
            connection = new HubConnectionBuilder()
                    .WithUrl("https://localhost:5001/taskHub")
                    .Build();

            #region snippet_ClosedRestart
            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };
            #endregion

            tbl.ItemsSource = assignments;

            //SendRequest("", "https://localhost:5001/Home/InitialDb");

            UpdateDb();
            StartedOn.IsEnabled = false;
            CompletedOn.IsEnabled = false;
            Delete.IsEnabled = false;
        }

        void SendRequest(string jsonData, string url)
        {
            byte[] body = Encoding.UTF8.GetBytes(jsonData);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = body.Length;
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(body, 0, body.Length);
                stream.Close();
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                string result = reader.ReadToEnd();
                var jsonResponse = JsonConvert.DeserializeObject<List<Assignment>>(result);

                assignments.Clear();

                foreach (var jr in jsonResponse)
                {
                    Assignment Assignment = new Assignment();
                    Assignment.Id = jr.Id;
                    Assignment.Description = jr.Description;
                    Assignment.StartedOn = jr.StartedOn;
                    Assignment.CompletedOn = jr.CompletedOn;
                    assignments.Add(Assignment); 
                }
            }

            response.Close();
            return;
        }

        // Загрузка данных из БД
        async void UpdateDb()
        {
            connection.On<IEnumerable<Assignment>>("ReceiveDb", assignmentsSendDb =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    assignments.Clear();

                    foreach (var item in assignmentsSendDb)
                    {
                        Assignment Assignment = new Assignment
                        {
                            Id = item.Id,
                            Description = item.Description,
                            StartedOn = item.StartedOn,
                            CompletedOn = item.CompletedOn
                        };
                        assignments.Add(Assignment);
                    }
                    
                });
            });
            try
            {
                await connection.StartAsync();
                await connection.InvokeAsync("UpdateDbAsync");
            }
            catch (Exception ex)
            {
                Status.Text = ex.Message;
            }
        } 

        Assignment selectedAssignmentButton = new Assignment(); // Переменная выбора пункта задания

        private void Tbl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Assignment selectedAssignment = (Assignment)tbl.SelectedItem;
            selectedAssignmentButton.Id = selectedAssignment.Id;
            selectedAssignmentButton.Description = selectedAssignment.Description;
            selectedAssignmentButton.StartedOn = selectedAssignment.StartedOn;
            selectedAssignmentButton.CompletedOn = selectedAssignment.CompletedOn;
            StartedOn.IsEnabled = true;
            CompletedOn.IsEnabled = true;
            Delete.IsEnabled = true;
        }


        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            Assignment assignment = new Assignment
            {
                Id = 0,
                Description = txb.Text,
                StartedOn = null,
                CompletedOn = null
            };
                     
            connection.On<IEnumerable<Assignment>, string>("ReceiveAdd", (assignmentsAdd, messageAdd) =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    assignments.Clear();

                    foreach (var item in assignmentsAdd)
                    {
                        Assignment Assignment = new Assignment
                        {
                            Id = item.Id,
                            Description = item.Description,
                            StartedOn = item.StartedOn,
                            CompletedOn = item.CompletedOn
                        };
                        assignments.Add(Assignment);
                    }
                    Status.Text = messageAdd;
                });
            });
           
            try
            {
                await connection.StartAsync();
                await connection.InvokeAsync("AddAsync", assignment);
            }
            catch (Exception ex)
            {
                Status.Text = ex.Message;
            }
            UpdateDb();
        }

       
        private async void StartedOn_Click(object sender, RoutedEventArgs e)
        {
                       
            Assignment assignment = new Assignment
            {
                Id = selectedAssignmentButton.Id,
                Description = selectedAssignmentButton.Description,
                StartedOn = selectedAssignmentButton.StartedOn,
                CompletedOn = selectedAssignmentButton.CompletedOn
            };

            connection.On<IEnumerable<Assignment>, string>("ReceiveStartedOn", (assignmentsStartedOn, messageStartedOn) =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    assignments.Clear();

                    foreach (var item in assignmentsStartedOn)
                    {
                        Assignment Assignment = new Assignment
                        {
                            Id = item.Id,
                            Description = item.Description,
                            StartedOn = item.StartedOn,
                            CompletedOn = item.CompletedOn
                        };
                        assignments.Add(Assignment);
                    }
                    Status.Text = messageStartedOn;
                });
            });

            try
            {
                await connection.StartAsync();
                await connection.InvokeAsync("StartedOnAsync", assignment);
            }
            catch (Exception ex)
            {
                Status.Text = ex.Message;
            }
            StartedOn.IsEnabled = false;
            CompletedOn.IsEnabled = false;
            Delete.IsEnabled = false;
            UpdateDb();
        }

        private async void CompletedOn_Click(object sender, RoutedEventArgs e)
        {
            Assignment assignment = new Assignment
            {
                Id = selectedAssignmentButton.Id,
                Description = selectedAssignmentButton.Description,
                StartedOn = selectedAssignmentButton.StartedOn,
                CompletedOn = selectedAssignmentButton.CompletedOn
            };

            connection.On<IEnumerable<Assignment>, string>("ReceiveCompletedOn", (assignmentsCompletedOn, messageCompletedOn) =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    assignments.Clear();

                    foreach (var item in assignmentsCompletedOn)
                    {
                        Assignment Assignment = new Assignment
                        {
                            Id = item.Id,
                            Description = item.Description,
                            StartedOn = item.StartedOn,
                            CompletedOn = item.CompletedOn
                        };
                        assignments.Add(Assignment);
                    }
                    Status.Text = messageCompletedOn;
                });
            });

            try
            {
                await connection.StartAsync();
                await connection.InvokeAsync("CompletedOnAsync", assignment);
            }
            catch (Exception ex)
            {
                Status.Text = ex.Message;
            }
            StartedOn.IsEnabled = false;
            CompletedOn.IsEnabled = false;
            Delete.IsEnabled = false;
            UpdateDb();
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
           
            int? id = selectedAssignmentButton.Id;

            connection.On<IEnumerable<Assignment>, string>("ReceiveDeleteTaskDb", (assignmentsDeleteTaskDb, messageDeleteTaskD) =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    assignments.Clear();

                    foreach (var item in assignmentsDeleteTaskDb)
                    {
                        Assignment Assignment = new Assignment
                        {
                            Id = item.Id,
                            Description = item.Description,
                            StartedOn = item.StartedOn,
                            CompletedOn = item.CompletedOn
                        };
                        assignments.Add(Assignment);
                    }
                   Status.Text = messageDeleteTaskD;
                });
            });

            try
            {
                await connection.StartAsync();
                await connection.InvokeAsync("DeleteTaskDbAsync", id);
            }
            catch (Exception ex)
            {
                Status.Text = ex.Message;
            }
            StartedOn.IsEnabled = false;
            CompletedOn.IsEnabled = false;
            Delete.IsEnabled = false;
            UpdateDb();
        }

        
    }
}





