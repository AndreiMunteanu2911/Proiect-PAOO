using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms.DataVisualization.Charting;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Reflection.Metadata;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace ProiectPAOO
{
    public partial class MainForm : Form
    {
        private string currentUser;
        private string? currentEthnicityFilter = null;
        private string? currentCityFilter = null;
        private int? currentMinAgeFilter = null;
        private int? currentMaxAgeFilter = null;
        private PopulationManager dbInteract;

        public MainForm(string username)
        {
            InitializeComponent();
            currentUser = username;
            dbInteract = new PopulationManager(currentUser);

            SetupMenu();
            LoadPersons();
        }
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Name = "MainForm";
            Panel dataGridViewPanel = new Panel();
            dataGridViewPanel.Dock = DockStyle.Fill;
            dataGridViewPanel.Name = "dataGridViewPanel";
            this.Controls.Add(dataGridViewPanel);

            this.ResumeLayout(false);
        }

        private void SetupMenu()
        {
            MenuStrip menuStrip = new MenuStrip();
            ToolStripMenuItem editMenu = new ToolStripMenuItem("Edit");
            ToolStripMenuItem searchMenu = new ToolStripMenuItem("Search");
            ToolStripMenuItem resetFiltersItem = new ToolStripMenuItem("Reset filters");
            resetFiltersItem.Click += (sender, e) =>
            {
                currentEthnicityFilter = null;
                currentCityFilter = null;
                currentMinAgeFilter = null;
                currentMaxAgeFilter = null;
                LoadPersons();
            };
            searchMenu.DropDownItems.Add(resetFiltersItem);
            if (currentUser == "admin")
            {
                ToolStripMenuItem historicMenuItem = new ToolStripMenuItem("Usage History");
                historicMenuItem.Click += (sender, e) => ShowUsageHistory();
                editMenu.DropDownItems.Add(historicMenuItem);
                ToolStripMenuItem addPersonItem = new ToolStripMenuItem("Add Person");
                addPersonItem.Click += (sender, e) => ShowAddPersonForm();
                editMenu.DropDownItems.Add(addPersonItem);
                ToolStripMenuItem updatePersonItem = new ToolStripMenuItem("Update Person");
                updatePersonItem.Click += (sender, e) => ShowUpdatePersonForm();
                editMenu.DropDownItems.Add(updatePersonItem);
                ToolStripMenuItem deletePersonItem = new ToolStripMenuItem("Delete Person");
                deletePersonItem.Click += (sender, e) => ShowDeletePersonForm();
                editMenu.DropDownItems.Add(deletePersonItem);
            }
            ToolStripMenuItem searchByIdItem = new ToolStripMenuItem("Search by ID");
            searchByIdItem.Click += (sender, e) => SearchPersonById();
            searchMenu.DropDownItems.Add(searchByIdItem);
            ToolStripMenuItem searchByAgeRangeItem = new ToolStripMenuItem("Search by Age Range");
            searchByAgeRangeItem.Click += (sender, e) => SearchByAgeRange();
            searchMenu.DropDownItems.Add(searchByAgeRangeItem);
            ToolStripMenuItem searchByEthnicityItem = new ToolStripMenuItem("Search by Ethnicity");
            searchByEthnicityItem.Click += (sender, e) => SearchByEthnicity();
            searchMenu.DropDownItems.Add(searchByEthnicityItem);
            ToolStripMenuItem searchByCityItem = new ToolStripMenuItem("Search by City");
            searchByCityItem.Click += (sender, e) => SearchByCity();
            searchMenu.DropDownItems.Add(searchByCityItem);
            ToolStripMenuItem logoutItem = new ToolStripMenuItem("Logout");
            logoutItem.Click += (sender, e) =>
            {
                PopulationManager.LogUsage(currentUser, "Logout", "N/A");
                this.Close();
                new LoginForm().Show();
            };
            editMenu.DropDownItems.Add(logoutItem);
            ToolStripMenuItem exportMenu = new ToolStripMenuItem("Export/Import");
            ToolStripMenuItem exportToCsvItem = new ToolStripMenuItem("Export to CSV");
            exportToCsvItem.Click += (sender, e) => ExportToCsv();
            exportMenu.DropDownItems.Add(exportToCsvItem);
            ToolStripMenuItem exportToPdfItem = new ToolStripMenuItem("Export to PDF");
            exportToPdfItem.Click += (sender, e) => ExportToPdf();
            exportMenu.DropDownItems.Add(exportToPdfItem);

            if (currentUser == "admin")
            {
                ToolStripMenuItem importFromCsvItem = new ToolStripMenuItem("Import from CSV");
                importFromCsvItem.Click += (sender, e) => ImportFromCsv();
                exportMenu.DropDownItems.Add(importFromCsvItem);
            }
            ToolStripMenuItem graphsMenu = new ToolStripMenuItem("Graphs");
            ToolStripMenuItem generatePieChartItem = new ToolStripMenuItem("Generate Pie Chart by Ethnicity");
            generatePieChartItem.Click += (sender, e) => GeneratePieChartByEthnicity();
            graphsMenu.DropDownItems.Add(generatePieChartItem);
            ToolStripMenuItem generateHistogramItem = new ToolStripMenuItem("Generate Histogram by Age");
            generateHistogramItem.Click += (sender, e) => GenerateAgeHistogram();
            graphsMenu.DropDownItems.Add(generateHistogramItem);
            menuStrip.Items.Add(editMenu);
            menuStrip.Items.Add(searchMenu);
            menuStrip.Items.Add(graphsMenu);
            menuStrip.Items.Add(exportMenu);
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }

        private void ImportFromCsv()
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "CSV files (*.csv)|*.csv";
                    openFileDialog.Title = "Import from CSV";
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        List<Person> persons = new List<Person>();
                        List<Address> addresses = new List<Address>();

                        using (StreamReader reader = new StreamReader(openFileDialog.FileName))
                        {
                            string line;
                            bool isFirstLine = true;
                            while ((line = reader.ReadLine()) != null)
                            {
                                if (isFirstLine)
                                {
                                    isFirstLine = false;
                                    continue;
                                }

                                string[] values = line.Split(',');

                                Address address = new Address(
                                    0,
                                    values[3],
                                    values[4],
                                    values[5],
                                    values[6]
                                );

                                Person person = new Person(
                                    0,
                                    values[1],
                                    int.Parse(values[2]),
                                    0,
                                    values[7],
                                    values[8],
                                    values[9]
                                );

                                addresses.Add(address);
                                persons.Add(person);
                            }
                        }

                        dbInteract.ClearDatabase();
                        for (int i = 0; i < persons.Count; i++)
                        {
                            dbInteract.AddPerson(persons[i], addresses[i]);
                        }

                        MessageBox.Show("Data imported successfully from CSV.", "Import from CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadPersons();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error importing from CSV: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ExportToCsv()
        {
            try
            {
                List<Person> persons = dbInteract.FilterAndSortPersons(null, null, null, null, "name ASC");

                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "CSV files (*.csv)|*.csv";
                    saveFileDialog.Title = "Save as CSV";
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                        {
                            writer.WriteLine("ID,Name,Age,Street,City,State,PostalCode,BirthPlace,CNP,Ethnicity");

                            foreach (var person in persons)
                            {
                                Address address = dbInteract.GetAddress(person.AddressId);
                                writer.WriteLine($"{person.Id},{person.Name},{person.Age},{address?.Street},{address?.City},{address?.State},{address?.PostalCode},{person.BirthPlace},{person.CNP},{person.Ethnicity}");
                            }
                        }
                        MessageBox.Show("Data exported successfully to CSV.", "Export to CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting to CSV: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void ExportToPdf()
        {
            try
            {
                List<Person> persons = dbInteract.FilterAndSortPersons(null, null, null, null, "name ASC");

                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "PDF files (*.pdf)|*.pdf";
                    saveFileDialog.Title = "Save as PDF";
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        using (FileStream stream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                        {
                            PdfWriter writer = new PdfWriter(stream);
                            PdfDocument pdfDoc = new PdfDocument(writer);
                            iText.Layout.Document document = new iText.Layout.Document(pdfDoc, iText.Kernel.Geom.PageSize.A4);
                            document.SetMargins(20, 20, 20, 20);

                            Table table = new Table(UnitValue.CreatePercentArray(new float[] { 1, 2, 1, 2, 2, 2, 2, 2, 2, 2 })).UseAllAvailableWidth();
                            table.AddHeaderCell("ID");
                            table.AddHeaderCell("Name");
                            table.AddHeaderCell("Age");
                            table.AddHeaderCell("Street");
                            table.AddHeaderCell("City");
                            table.AddHeaderCell("State");
                            table.AddHeaderCell("PostalCode");
                            table.AddHeaderCell("BirthPlace");
                            table.AddHeaderCell("CNP");
                            table.AddHeaderCell("Ethnicity");

                            foreach (var person in persons)
                            {
                                Address address = dbInteract.GetAddress(person.AddressId);
                                table.AddCell(person.Id.ToString());
                                table.AddCell(person.Name);
                                table.AddCell(person.Age.ToString());
                                table.AddCell(address?.Street ?? "N/A");
                                table.AddCell(address?.City ?? "N/A");
                                table.AddCell(address?.State ?? "N/A");
                                table.AddCell(address?.PostalCode ?? "N/A");
                                table.AddCell(person.BirthPlace);
                                table.AddCell(person.CNP);
                                table.AddCell(person.Ethnicity);
                            }

                            document.Add(table);
                            document.Close();
                        }
                        MessageBox.Show("Data exported successfully to PDF.", "Export to PDF", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting to PDF: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        private void GeneratePieChartByEthnicity()
        {
            try
            {
                List<Person> persons = dbInteract.FilterAndSortPersons(null, null, null, null, "name ASC");
                var ethnicityGroups = persons.GroupBy(p => p.Ethnicity)
                                             .Select(g => new { Ethnicity = g.Key, Count = g.Count() })
                                             .ToList();

                Chart chart = new Chart();
                chart.Dock = DockStyle.Fill;

                ChartArea chartArea = new ChartArea();
                chart.ChartAreas.Add(chartArea);

                Series series = new Series
                {
                    Name = "Ethnicity",
                    IsVisibleInLegend = true,
                    ChartType = SeriesChartType.Pie
                };

                chart.Series.Add(series);

                foreach (var group in ethnicityGroups)
                {
                    series.Points.AddXY(group.Ethnicity, group.Count);
                }

                Form chartForm = new Form();
                chartForm.Text = "Pie Chart by Ethnicity";
                chartForm.Size = new Size(800, 600);
                chartForm.Controls.Add(chart);
                chartForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating pie chart: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerateAgeHistogram()
        {
            try
            {
                List<Person> persons = dbInteract.FilterAndSortPersons(null, null, null, null, "name ASC");
                var ageGroups = persons.GroupBy(p => p.Age)
                                       .Select(g => new { Age = g.Key, Count = g.Count() })
                                       .ToList();

                Chart chart = new Chart();
                chart.Dock = DockStyle.Fill;

                ChartArea chartArea = new ChartArea();
                chart.ChartAreas.Add(chartArea);

                Series series = new Series
                {
                    Name = "Age",
                    IsVisibleInLegend = true,
                    ChartType = SeriesChartType.Column
                };

                chart.Series.Add(series);

                foreach (var group in ageGroups)
                {
                    series.Points.AddXY(group.Age, group.Count);
                }

                Form chartForm = new Form();
                chartForm.Text = "Age Histogram";
                chartForm.Size = new Size(800, 600);
                chartForm.Controls.Add(chart);
                chartForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating age histogram: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPersons()
        {
            try
            {
                List<Person> persons = dbInteract.FilterAndSortPersons(
                    currentEthnicityFilter,
                    currentMinAgeFilter,
                    currentMaxAgeFilter,
                    currentCityFilter,
                    "name ASC"
                );

                DisplayPersons(persons);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading persons: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void DisplayPersons(List<Person> persons)
        {
            Panel dataGridViewPanel = this.Controls.OfType<Panel>().FirstOrDefault(p => p.Name == "dataGridViewPanel");
            if (dataGridViewPanel == null) return;
            dataGridViewPanel.Controls.Clear();
            DataGridView dataGridView = new DataGridView();
            dataGridView.Dock = DockStyle.Fill;
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.ReadOnly = true;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView.MultiSelect = false;
            dataGridView.ColumnHeadersVisible = true;
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", SortMode = DataGridViewColumnSortMode.Automatic });
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Name", SortMode = DataGridViewColumnSortMode.Automatic });
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Age", HeaderText = "Age", SortMode = DataGridViewColumnSortMode.Automatic });
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Street", HeaderText = "Street", SortMode = DataGridViewColumnSortMode.Automatic });
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "City", HeaderText = "City", SortMode = DataGridViewColumnSortMode.Automatic });
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "State", HeaderText = "State", SortMode = DataGridViewColumnSortMode.Automatic });
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "PostalCode", HeaderText = "Postal Code", SortMode = DataGridViewColumnSortMode.Automatic });
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "BirthPlace", HeaderText = "Birthplace", SortMode = DataGridViewColumnSortMode.Automatic });
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "CNP", HeaderText = "CNP", SortMode = DataGridViewColumnSortMode.Automatic });
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Ethnicity", HeaderText = "Ethnicity", SortMode = DataGridViewColumnSortMode.Automatic });
            foreach (var person in persons)
            {
                Address address = dbInteract.GetAddress(person.AddressId);
                dataGridView.Rows.Add(
                    person.Id,
                    person.Name,
                    person.Age,
                    address?.Street ?? "N/A",
                    address?.City ?? "N/A",
                    address?.State ?? "N/A",
                    address?.PostalCode ?? "N/A",
                    person.BirthPlace,
                    person.CNP,
                    person.Ethnicity
                );
            }

            dataGridViewPanel.Controls.Add(dataGridView);
        }




        private void ShowUsageHistory()
        {
            try
            {
                List<UsageHistory> history = PopulationManager.GetUsageHistory();

                Form historyForm = new Form();
                historyForm.Text = "Usage History";
                historyForm.Size = new Size(800, 400);

                DataGridView gridView = new DataGridView();
                gridView.Dock = DockStyle.Fill;
                gridView.AllowUserToAddRows = false;
                gridView.AllowUserToDeleteRows = false;
                gridView.ReadOnly = true;
                gridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                gridView.Columns.Add("Id", "ID");
                gridView.Columns.Add("DateTime", "Date/Time");
                gridView.Columns.Add("User", "User");
                gridView.Columns.Add("Operation", "Operation");
                gridView.Columns.Add("SqlCommand", "SQL Command");

                foreach (var entry in history)
                {
                    gridView.Rows.Add(
                        entry.Id,
                        entry.DateTime,
                        entry.User,
                        entry.Operation,
                        entry.SqlCommand
                    );
                }

                historyForm.Controls.Add(gridView);
                historyForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving usage history: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SearchPersonById()
        {
            string idStr = Prompt.ShowDialog("Enter ID of the person to search:", "Search by ID");
            if (!string.IsNullOrEmpty(idStr))
            {
                try
                {
                    int id = int.Parse(idStr);
                    Person person = dbInteract.GetPerson(id);
                    if (person != null)
                    {
                        Address address = dbInteract.GetAddress(person.AddressId);
                        DisplayPerson(person, address);
                        PopulationManager.LogUsage(currentUser, "Search by ID", "N/A");
                    }
                    else
                    {
                        MessageBox.Show("Person not found.", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (FormatException)
                {
                    MessageBox.Show("Invalid ID format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error searching for person: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void DisplayPerson(Person person, Address address)
        {
            List<Person> persons = new List<Person> { person };
            DisplayPersons(persons);
        }

        private void SearchByAgeRange()
        {
            string minAgeStr = Prompt.ShowDialog("Enter minimum age:", "Search by Age Range");
            if (string.IsNullOrEmpty(minAgeStr)) return;

            string maxAgeStr = Prompt.ShowDialog("Enter maximum age:", "Search by Age Range");
            if (string.IsNullOrEmpty(maxAgeStr)) return;

            try
            {
                int minAge = int.Parse(minAgeStr);
                int maxAge = int.Parse(maxAgeStr);

                if (minAge <= maxAge)
                {
                    currentMinAgeFilter = minAge;
                    currentMaxAgeFilter = maxAge;
                    PopulationManager.LogUsage(currentUser, "Search by Age Range", "N/A");
                    LoadPersons();
                }
                else
                {
                    MessageBox.Show("Minimum age must be less than or equal to maximum age.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter valid age values.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error searching by age range: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SearchByEthnicity()
        {
            string ethnicity = Prompt.ShowDialog("Enter ethnicity to search:", "Search by Ethnicity");
            if (!string.IsNullOrEmpty(ethnicity))
            {
                currentEthnicityFilter = ethnicity;
                try
                {
                    PopulationManager.LogUsage(currentUser, "Search by Ethnicity", "N/A");
                    LoadPersons();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error searching by ethnicity: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please enter an ethnicity.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SearchByCity()
        {
            string city = Prompt.ShowDialog("Enter city to search:", "Search by City");
            if (!string.IsNullOrEmpty(city))
            {
                currentCityFilter = city;
                try
                {
                    PopulationManager.LogUsage(currentUser, "Search by City", "N/A");
                    LoadPersons();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error searching by city: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please enter a city.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowAddPersonForm()
        {
            Form addForm = new Form();
            addForm.Text = "Add Person";
            addForm.Size = new Size(400, 500);
            addForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            addForm.MaximizeBox = false;
            addForm.MinimizeBox = false;
            addForm.StartPosition = FormStartPosition.CenterParent;

            TableLayoutPanel tableLayout = new TableLayoutPanel();
            tableLayout.Dock = DockStyle.Fill;
            tableLayout.ColumnCount = 2;
            tableLayout.RowCount = 10;
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            string[] labels = { "Name:", "Age:", "Street:", "City:", "Country:", "Postal Code:", "Ethnicity:", "Birthplace:", "CNP:" };
            TextBox[] textBoxes = new TextBox[9];

            for (int i = 0; i < labels.Length; i++)
            {
                Label label = new Label();
                label.Text = labels[i];
                label.Anchor = AnchorStyles.Left;
                tableLayout.Controls.Add(label, 0, i);

                textBoxes[i] = new TextBox();
                textBoxes[i].Dock = DockStyle.Fill;
                tableLayout.Controls.Add(textBoxes[i], 1, i);
            }
            TableLayoutPanel buttonPanel = new TableLayoutPanel();
            buttonPanel.ColumnCount = 2;
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            buttonPanel.Dock = DockStyle.Fill;

            Button okButton = new Button();
            okButton.Text = "OK";
            okButton.Dock = DockStyle.Fill;
            okButton.Click += (sender, e) =>
            {
                try
                {
                    if (textBoxes[8].Text.Length != 13 || !textBoxes[8].Text.All(char.IsDigit))
                    {
                        MessageBox.Show("CNP must be exactly 13 digits.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    Address address = new Address(0,
                         textBoxes[2].Text,
                         textBoxes[3].Text,
                        textBoxes[4].Text,
                        textBoxes[5].Text
                    );

                    Person person = new Person(
                    
                        0,
                        Name = textBoxes[0].Text,
                         int.Parse(textBoxes[1].Text),
                         0,
                        textBoxes[7].Text,
                         textBoxes[8].Text,
                         textBoxes[6].Text
                    );

                    dbInteract.AddPerson(person, address);
                    PopulationManager.LogUsage(currentUser, "Add Person", "N/A");

                    addForm.DialogResult = DialogResult.OK;
                    addForm.Close();

                    LoadPersons();
                }
                catch (FormatException)
                {
                    MessageBox.Show("Please enter valid values for all fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error adding person: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            Button cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.Dock = DockStyle.Fill;
            cancelButton.Click += (sender, e) =>
            {
                addForm.DialogResult = DialogResult.Cancel;
                addForm.Close();
            };

            buttonPanel.Controls.Add(okButton, 0, 0);
            buttonPanel.Controls.Add(cancelButton, 1, 0);

            tableLayout.Controls.Add(buttonPanel, 0, 9);
            tableLayout.SetColumnSpan(buttonPanel, 2);

            addForm.Controls.Add(tableLayout);
            addForm.ShowDialog();
        }


        private void ShowUpdatePersonForm()
        {
            string idStr = Microsoft.VisualBasic.Interaction.InputBox("Enter ID of the person to update:", "Update Person", "");

            if (!string.IsNullOrEmpty(idStr))
            {
                try
                {
                    int id = int.Parse(idStr);
                    Person person = dbInteract.GetPerson(id);
                    if (person != null)
                    {
                        Address address = dbInteract.GetAddress(person.AddressId);

                        Panel panel = new Panel();
                        panel.Controls.Add(new Label() { Text = "Name:" });
                        TextBox nameField = new TextBox() { Text = person.Name };
                        panel.Controls.Add(nameField);

                        panel.Controls.Add(new Label() { Text = "Age:" });
                        TextBox ageField = new TextBox() { Text = person.Age.ToString() };
                        panel.Controls.Add(ageField);

                        panel.Controls.Add(new Label() { Text = "Street:" });
                        TextBox streetField = new TextBox() { Text = address.Street };
                        panel.Controls.Add(streetField);

                        panel.Controls.Add(new Label() { Text = "City:" });
                        TextBox cityField = new TextBox() { Text = address.City };
                        panel.Controls.Add(cityField);

                        panel.Controls.Add(new Label() { Text = "Country:" });
                        TextBox countryField = new TextBox() { Text = address.State };
                        panel.Controls.Add(countryField);

                        panel.Controls.Add(new Label() { Text = "Postal Code:" });
                        TextBox zipField = new TextBox() { Text = address.PostalCode };
                        panel.Controls.Add(zipField);

                        panel.Controls.Add(new Label() { Text = "Birthplace:" });
                        TextBox birthplaceField = new TextBox() { Text = person.BirthPlace };
                        panel.Controls.Add(birthplaceField);

                        panel.Controls.Add(new Label() { Text = "CNP:" });
                        TextBox cnpField = new TextBox() { Text = person.CNP };
                        panel.Controls.Add(cnpField);

                        panel.Controls.Add(new Label() { Text = "Ethnicity:" });
                        TextBox ethnicityField = new TextBox() { Text = person.Ethnicity };
                        panel.Controls.Add(ethnicityField);
                        TableLayoutPanel layoutPanel = new TableLayoutPanel();
                        layoutPanel.RowCount = 9;
                        layoutPanel.ColumnCount = 2;
                        layoutPanel.Controls.Add(new Label() { Text = "Name:" }, 0, 0);
                        layoutPanel.Controls.Add(nameField, 1, 0);
                        layoutPanel.Controls.Add(new Label() { Text = "Age:" }, 0, 1);
                        layoutPanel.Controls.Add(ageField, 1, 1);
                        layoutPanel.Controls.Add(new Label() { Text = "Street:" }, 0, 2);
                        layoutPanel.Controls.Add(streetField, 1, 2);

                        layoutPanel.Controls.Add(new Label() { Text = "City:" }, 0, 3);
                        layoutPanel.Controls.Add(cityField, 1, 3);
                        layoutPanel.Controls.Add(new Label() { Text = "Country:" }, 0, 4);
                        layoutPanel.Controls.Add(countryField, 1, 4);
                        layoutPanel.Controls.Add(new Label() { Text = "Postal Code:" }, 0, 5);
                        layoutPanel.Controls.Add(zipField, 1, 5);
                        layoutPanel.Controls.Add(new Label() { Text = "Birthplace:" }, 0, 6);
                        layoutPanel.Controls.Add(birthplaceField, 1, 6);
                        layoutPanel.Controls.Add(new Label() { Text = "CNP:" }, 0, 7);
                        layoutPanel.Controls.Add(cnpField, 1, 7);
                        layoutPanel.Controls.Add(new Label() { Text = "Ethnicity:" }, 0, 8);
                        layoutPanel.Controls.Add(ethnicityField, 1, 8);

                        Form updateForm = new Form();
                        updateForm.Text = "Update Person";
                        updateForm.Size = new Size(400, 500);
                        updateForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                        updateForm.MaximizeBox = false;
                        updateForm.MinimizeBox = false;
                        updateForm.StartPosition = FormStartPosition.CenterParent;
                        updateForm.Controls.Add(layoutPanel);

                        DialogResult result = updateForm.ShowDialog();
                        if (result == DialogResult.OK)
                        {
                            if (cnpField.Text.Length != 13 || !System.Text.RegularExpressions.Regex.IsMatch(cnpField.Text, @"^\d{13}$"))
                            {
                                MessageBox.Show("CNP must be exactly 13 digits.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            Address updatedAddress = new Address(0, streetField.Text, cityField.Text, countryField.Text, zipField.Text);
                            person.Name = nameField.Text;
                            person.Age = int.Parse(ageField.Text);
                            person.BirthPlace = birthplaceField.Text;
                            person.CNP = cnpField.Text;
                            person.Ethnicity = ethnicityField.Text;

                            dbInteract.UpdatePerson(person, updatedAddress);
                            LoadPersons();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Person not found.");
                    }
                }
                catch (SqlException ex)
                {
                    ex.ToString();
                }
            }
        }

        private void ShowDeletePersonForm()
        {
            string idStr = Microsoft.VisualBasic.Interaction.InputBox("Enter ID of the person to delete:", "Delete Person", "");

            if (!string.IsNullOrEmpty(idStr))
            {
                try
                {
                    int id = int.Parse(idStr);
                    Person person = dbInteract.GetPerson(id);
                    if (person != null)
                    {
                        DialogResult confirm = MessageBox.Show(
                            "Are you sure you want to delete " + person.Name + "?",
                            "Confirm Deletion",
                            MessageBoxButtons.YesNo);

                        if (confirm == DialogResult.Yes)
                        {
                            dbInteract.DeletePerson(id);
                            LoadPersons();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Person not found.");
                    }
                }
                catch (SqlException ex)
                {
                    ex.ToString();
                }
                catch (FormatException ex)
                {
                    MessageBox.Show("Invalid ID format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
/*
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                MainForm window = new MainForm("admin");
                window.Show();
                Application.Run();
            }
            catch (Exception e)
            {
                e.ToString();
            }}
*/
        
    }
}
