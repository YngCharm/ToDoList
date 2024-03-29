using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
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

namespace ToDoList
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SqlConnection sqlConnection = new SqlConnection();
        SqlCommand sqlCommand = new SqlCommand();
        SqlDataReader reader = null;
        TextBox selectedTextBox;
        public MainWindow()
        {
            InitializeComponent();
            sqlConnection.ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB; Initial Catalog=ToDoList; Integrated Security=SSPI;";
            sqlConnection.Open();
            outputNoteTable();
            noteTitle.Visibility = Visibility.Collapsed;
            noteContent.Visibility = Visibility.Collapsed;
            messageText.Text = "";
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            TextBox newTextBox = new TextBox();
            newTextBox.IsReadOnly = true;
            newTextBox.Height = 40;
            newTextBox.Margin = new Thickness(5);

            newTextBox.BorderThickness = new Thickness(1);
            newTextBox.BorderBrush = Brushes.Black;

            if (noteTitle.Text != "" && noteContent.Text != "")
            {
                string addInDBQuerry = @"INSERT INTO NoteTable (Title, Content) VALUES (@Title, @Content)";

                using (SqlCommand command = new SqlCommand(addInDBQuerry, sqlConnection))
                {
                    // Добавление параметров
                    command.Parameters.AddWithValue("@Title", noteTitle.Text);
                    command.Parameters.AddWithValue("@Content", noteContent.Text);

                    // Выполнение запроса
                    command.ExecuteNonQuery();
                    newTextBox.Text = noteTitle.Text;
                    noteList.Children.Add(newTextBox);
                    SaveButton.IsEnabled = false;
                    messageText.Text = "Запись сохранена";


                }
            }
            else if (noteTitle.Text == "" && noteContent.Text != "")
            {
                int thirdSpaceIndex = noteContent.Text.IndexOf(' ', noteContent.Text.IndexOf(' ', noteContent.Text.IndexOf(' ') + 1) + 1);
                string newName = noteContent.Text.Substring(0, thirdSpaceIndex);

                string addInDBQuerry = @"INSERT INTO NoteTable (Title, Content) VALUES (@Title, @Content)";

                using (SqlCommand command = new SqlCommand(addInDBQuerry, sqlConnection))
                {
                    // Добавление параметров
                    command.Parameters.AddWithValue("@Title", newName);
                    command.Parameters.AddWithValue("@Content", noteContent.Text);

                    // Выполнение запроса
                    command.ExecuteNonQuery();
                    newTextBox.Text = newName;
                    noteList.Children.Add(newTextBox);
                    SaveButton.IsEnabled = false;
                    messageText.Text = "Запись сохранена";

                }
            }
        }

        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            noteTitle.Visibility = Visibility.Visible;
            noteContent.Visibility = Visibility.Visible;
            noteTitle.Text = "";
            noteContent.Text = "";
            SaveButton.IsEnabled = true;


        }

        private void outputNoteTable()
        {
            string query = @"SELECT Title, Id FROM NoteTable";

            sqlCommand = new SqlCommand(query, sqlConnection);

            reader = sqlCommand.ExecuteReader();

            while (reader.Read())
            {
                messageText.Text = "Запись сохранена";
                TextBox newTextBox = new TextBox();
                newTextBox.Text = $"{reader["Title"]}";
                newTextBox.Tag = reader["Id"];// Здесь указываете имена столбцов, которые хотите отобразить
                newTextBox.IsReadOnly = true;
                newTextBox.Height = 40;
                newTextBox.Margin = new Thickness(5);

                newTextBox.BorderThickness = new Thickness(1);
                newTextBox.BorderBrush = Brushes.Black;
                noteList.Children.Add(newTextBox);
                newTextBox.PreviewMouseDown += TextBox_Click;


            }
            reader.Close();

        }
        // Обработчик события для щелчка на TextBox

        private void TextBox_Click(object sender, MouseButtonEventArgs e)
        {
            noteTitle.Visibility = Visibility.Visible;
            noteContent.Visibility = Visibility.Visible;
            TextBox textBox = sender as TextBox;

            if (textBox != null)
            {
                int noteId = (int)textBox.Tag;
                selectedTextBox = textBox;

                string selectTitileQuery = @"SELECT Title FROM NoteTable WHERE Id =" + noteId;
                string title = null;
                using (SqlCommand command = new SqlCommand(selectTitileQuery, sqlConnection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            title = reader.GetString(0);
                            noteTitle.Text = title;
                        }
                    }
                }
                string selectConentQuery = @"SELECT Content FROM NoteTable WHERE Id =" + noteId;
                string content = null;
                using (SqlCommand command = new SqlCommand(selectConentQuery, sqlConnection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            content = reader.GetString(0);
                            noteContent.Text = content;
                        }
                    }
                }
            }
        }


        private void Complete_Button_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTextBox != null)
            {
                selectedTextBox.Background = Brushes.Green; // Применяем зеленый фон к выбранному TextBox
            }

        }

        private void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTextBox != null)
            {
                int noteId = (int)selectedTextBox.Tag;
                DeleteNoteFromDatabase(noteId);

                // Удаляем TextBox из StackPanel
                noteList.Children.Remove(selectedTextBox);
                selectedTextBox = null; // Очищаем выбранный TextBox после удаления
            }
        }
        private void DeleteNoteFromDatabase(int noteId)
        {
            // Создаем SQL-запрос для удаления записи из базы данных с указанным идентификатором
            string deleteQuery = @"DELETE FROM NoteTable WHERE Id=" + noteId;

            // Создаем и открываем подключение к базе данных
            using (SqlCommand command = new SqlCommand(deleteQuery, sqlConnection))
            {
                // Выполняем SQL-запрос
                int rowsAffected = command.ExecuteNonQuery();

                // Проверяем, были ли затронуты какие-либо строки в базе данных
                if (rowsAffected > 0)
                {
                    // Если были затронуты строки, выводим сообщение об успешном удалении записи
                    messageText.Text = "Заметка удалена";
                }
            }
        }
    }
}