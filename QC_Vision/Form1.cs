using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.IO;
using System.Drawing.Printing;



namespace QC_Vision
{

    
    public partial class OperatorScreen : Form
    {

        private int printedPages = 0;


        public OperatorScreen()
        {

            InitializeComponent();
        }
        

        //Load the data from a cubbyhole button. All buttons are linked to this function, sender argument is used to determine which cubby is pressed
        private void loadCubbyData(object sender, EventArgs e)
        {
            //Clear defect list
            defectList.Items.Clear();
            double offset = 0;
            double adj_factor = 0;

            cavityNumber.ImageLocation = "";

            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();


            //Get sender data
            Button tempButton = (Button)sender;


            //Check if a cavity in the tray was selected
            if (tempButton.BackColor == Color.White)
            {
                this.Cursor = Cursors.Default;
                return;
            }

            //Establish database connection
            DBConnect database = new DBConnect();


            MySqlDataReader dataReader = database.Select("Select * from unpivoted_parts_table where trayuniqueid = \"" + this.trayComboBox.SelectedItem.ToString() + "\" and cubbyholenumber = \"" + tempButton.Text + "\" and passfail > 0" );

            //Load defect list
            while (dataReader.Read())
            {
                if(dataReader.IsDBNull(15))
                {
                    offset = 0;
                }
                else
                {
                    offset = dataReader.GetDouble("offset");
                }

                if (dataReader.IsDBNull(13))
                {
                    adj_factor = 1;
                }
                else
                {
                    adj_factor = dataReader.GetDouble("Adjustment_factor");
                }

                defectList.Items.Add(dataReader.GetString("Measurement") + " : " + Math.Round(dataReader.GetDouble("Result") * adj_factor + offset, 3));
            }

            dataReader.Close();



            //Load timestamp. this cannot be done as the previous reader, as a null is returned on empty defects
            dataReader = database.Select("Select * from unpivoted_parts_table where trayuniqueid = \"" + this.trayComboBox.SelectedItem.ToString() + "\" and cubbyholenumber = \"" + tempButton.Text + "\" limit 1");
            dataReader.Read();

            ;


            //Load photo to picture box
            cavityNumber.ImageLocation = getImageLocation(dataReader.GetString("timestamp"));

            database.CloseConnection();

            this.Cursor = Cursors.Default;

        }

        //Automatically load data when machine combo box is activated
        private void machineComboBox_DropDown(object sender, System.EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            //Clear and connect to DB
            this.machineComboBox.Items.Clear();
            DBConnect database = new DBConnect();

            //Read all relevant data into the combobox
            MySqlDataReader dataReader = database.Select("Select distinct moulderid from unpivoted_parts_table order by moulderid asc");
            while(dataReader.Read())
            {
                this.machineComboBox.Items.Add(dataReader.GetString("moulderid"));

            }

            dataReader.Close();
            database.CloseConnection();
            this.Cursor = Cursors.Default;

        }


        //Select a tray and automatically load data
        private void trayComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            //Load tray data into reader
            DBConnect database = new DBConnect();
            MySqlDataReader dataReader = database.Select("select cubbyholenumber, partid, sum(PassFail) from unpivoted_parts_table where trayuniqueid = \"" + this.trayComboBox.SelectedItem.ToString() + "\" group by cubbyholenumber;");

            //Count cavities
            int cavities = 0;

            //Clear defect list
            defectList.Items.Clear();

            //Reset all buttons to gray
            for (int i = 1; i < 65; i++)
            {
                Button tempButton = Controls.Find("cubby" + i, true).FirstOrDefault() as Button;
                tempButton.BackColor = Color.White;
            }

            //Check all cubby holes for defects, change colour if defect detected
            //
            //TODO: Change tone of defect buttons depending on number of defects
            while (dataReader.Read())
            {
                cavities++;
                partLabel.Text = dataReader.GetString("partid");
                if (dataReader.GetInt32("sum(PassFail)") == 0)
                    {
                    Button tempButton = Controls.Find("cubby" + dataReader.GetInt32("cubbyholenumber"), true).FirstOrDefault() as Button;
                    tempButton.BackColor = Color.LightGreen;

                }
                else
                {
                    Button tempButton = Controls.Find("cubby" + dataReader.GetInt32("cubbyholenumber"), true).FirstOrDefault() as Button;
                    tempButton.BackColor = Color.Red;
                }
            }
            cavityLabel.Text = cavities + " cavities";

            trayNumber.Text = "Tray Number: " + this.trayComboBox.SelectedItem.ToString().Substring(0, this.trayComboBox.SelectedItem.ToString().IndexOf(":"));

            cavityNumber.ImageLocation = "";

            dataReader.Close();

            database.CloseConnection();
            this.Cursor = Cursors.Default;
        }


        //When machine is selected
        private void machineComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

            this.Cursor = Cursors.WaitCursor;

            foreach(Control c in this.Controls)
            {
                c.Enabled = false;
            }

            Application.DoEvents();
            DBConnect database = new DBConnect();

            this.trayComboBox.Items.Clear();

            //Populate tray combo box
            MySqlDataReader dataReader = database.Select("select distinct trayuniqueid from unpivoted_parts_table where moulderid = \"" + this.machineComboBox.SelectedItem.ToString() + "\" order by timestamp desc; ");
            while (dataReader.Read())
            {
                this.trayComboBox.Items.Add(dataReader.GetString("trayuniqueid"));

            }

            //Select first item in the tray combo box. Automatically loads tray data
            this.trayComboBox.SelectedIndex = 0;

            dataReader.Close();
            database.CloseConnection();
            foreach (Control c in this.Controls)
            {
                c.Enabled = true;
            }

            this.Cursor = Cursors.Default;

        }

        private void cavityLabel_Click(object sender, EventArgs e)
        {

        }

        private void printDefects_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();


            if (trayComboBox.Text == "")
            {
                
                MessageBox.Show("Select a tray before printing");
                this.Cursor = Cursors.Default;
                return;
                
            }

            foreach (Control c in this.Controls)
            {
                c.Enabled = false;
            }



            try
            {

                printedPages = 0;
                PrintDocument pd = new PrintDocument();

                //Configure the default setttings of the document
                Margins margins = new Margins(1, 37, 1, 37);
                pd.DefaultPageSettings.Margins = margins;
                pd.PrinterSettings.PrinterName = "\\\\saturn\\lp-QCvision";
                pd.DefaultPageSettings.Color = true;
                pd.DefaultPageSettings.Landscape = true;


                //Add data to page
                pd.PrintPage += new PrintPageEventHandler(this.pd_PrintPage);


                //Print page
                pd.Print();
            }

            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


            foreach (Control c in this.Controls)
            {
                c.Enabled = true;
            }

            this.Cursor = Cursors.Default;

        }

        private string getImageLocation(string timestamp)
        {
            string filepath = "";
            string lazyHolder = "";
            string tempHolder = "";
            bool lastDir = true;
            /*this section of code is for the image search. The images are broken up into subfolders, with approx 100 images per sub folder. Each image is timestamped at the time taken, and each subfolder is timestamped with time created
             * The code iterates through the subfolders to find the folder immediately following the timestamp. After this is found, it indicates the timestamp is in the folder immediately before this one. It then iterates through the pictures
             * in a similar manner.
             * 
             * NB: this is a terribly designed timestamping/filing system, but it's what the machine was configured with. Consider refactoring ASAP.
             * 
             */

            //Find whether part is a stem or housing
            if (partLabel.Text.Substring(0, 2) == "04")
            {
                filepath = "Q:\\StemCavityNumber";
            }
            else
            {
                filepath = "Q:\\HousingFrontCavityNumber";
            }


            //Unify timestamp format
            timestamp.Replace(" ", "_");


            //Iterate through folders
            foreach (string d in Directory.GetDirectories(filepath))
            {

                tempHolder = d.Substring(d.LastIndexOf("\\") + 1);
                if (tempHolder.CompareTo(timestamp) < 0)
                {
                    lazyHolder = tempHolder;

                }
                else
                {
                    filepath = filepath + "\\" + lazyHolder;
                    lastDir = false;
                    break;
                }
            }


            //Flag to detect if picture is in latest folder
            if (lastDir)
            {
                filepath = filepath + "\\" + lazyHolder;
            }

            lastDir = true;


            //Iterate through files
            foreach (string d in Directory.GetFiles(filepath))
            {

                tempHolder = d.Substring(d.LastIndexOf("\\") + 1);
                if (tempHolder.CompareTo(timestamp) < 0)
                {
                    lazyHolder = tempHolder;
                }
                else
                {
                    //this section checks if the timestamp detected is within 5 seconds of the timestamp of the part. Due to the way the timestamps are assigned to the part vs the picture, there can
                    //be a desynced, with the timestamp slightly before or after the part. If the parts are displaying the picture of the part, consider lowering the threshold from 5.

                    if (Math.Abs(Int32.Parse(tempHolder.Substring(17, 2)) - Int32.Parse(timestamp.Substring(17, 2))) < 5)
                    {
                        filepath = filepath + "\\" + tempHolder;
                    }
                    else
                    {
                        filepath = filepath + "\\" + lazyHolder;
                    }
                    lastDir = false;
                    break;
                }
            }


            //Flag to detect if picture is last picture
            if (lastDir)
            {
                filepath = filepath + "\\" + lazyHolder;
            }

            return filepath;
        }

        private void pd_PrintPage(object sender, PrintPageEventArgs ev)
        {





            
            Pen blackPen = new Pen(Color.Black, 1);
            Font printFont = new Font("Arial", 8);
            int currentCubby = 0;
            int printedCubbys = 0;
            int currentDefects = 0;
            double offset = 0;
            double adj_factor = 0;


            ev.Graphics.DrawRectangle(blackPen, ev.MarginBounds);


            //Draw horizontal lines
            for(int i = 1; i < 7; i++)
            {

                Point p1 = new Point(1, ev.MarginBounds.Height / 7 * i);
                Point p2 = new Point(ev.MarginBounds.Width, ev.MarginBounds.Height / 7 * i);
                ev.Graphics.DrawLine(blackPen, p1, p2);
            }

            //Draw vertical lines
            for (int i = 1; i < 3; i++)
            {

                Point p1 = new Point(ev.MarginBounds.Width / 3 * i, 1);
                Point p2 = new Point(ev.MarginBounds.Width / 3 * i, ev.MarginBounds.Height);
                ev.Graphics.DrawLine(blackPen, p1, p2);
            }

            //Add tray guide to bottom right corner
            Image image = Image.FromFile("Q:\\StemCavityNumber\\tray diagram.png");
            PointF ulCorner = new PointF(ev.MarginBounds.Width / 3 * 2, ev.MarginBounds.Height / 7 * 4);
            PointF urCorner = new PointF(ev.MarginBounds.Width, ev.MarginBounds.Height / 7 * 4);
            PointF llCorner = new PointF(ev.MarginBounds.Width / 3 * 2, ev.MarginBounds.Height);

            PointF[]  destPara =  { ulCorner, urCorner, llCorner};

            ev.Graphics.DrawImage(image, destPara);


            //Populate header
            ev.Graphics.DrawString("Part Number: " + partLabel.Text, new Font("Arial", 14), Brushes.Black, new PointF(10, 10));
            ev.Graphics.DrawString("Machine Number: " + machineComboBox.Text, new Font("Arial", 14), Brushes.Black, new PointF(10, 10 + new Font("Arial", 14).GetHeight(ev.Graphics)));

            ev.Graphics.DrawString("Total Cavities: " + cavityLabel.Text, new Font("Arial", 14), Brushes.Black, new PointF(ev.MarginBounds.Width / 3 * 1 + 10, 10));
            ev.Graphics.DrawString("Tray Number: " + trayNumber.Text, new Font("Arial", 14), Brushes.Black, new PointF(ev.MarginBounds.Width / 3 * 1 + 10, 10 + new Font("Arial", 14).GetHeight(ev.Graphics)));

            ev.Graphics.DrawString("Tray ID: " + trayComboBox.Text, new Font("Arial", 14), Brushes.Black, new PointF(ev.MarginBounds.Width / 3 * 2 + 10, 10));
            ev.Graphics.DrawString("Time Printed: " + DateTime.Now.ToString("hh:mm:ss tt dd/mm/yyyy"), new Font("Arial", 14), Brushes.Black, new PointF(ev.MarginBounds.Width / 3 * 2 + 10, 10 + new Font("Arial", 14).GetHeight(ev.Graphics)));


            DBConnect database = new DBConnect();
            MySqlDataReader dataReader = database.Select("select * from unpivoted_parts_table where trayuniqueid = \"" + this.trayComboBox.SelectedItem.ToString() + "\" and passfail = 1 order by cubbyholenumber asc;");

            dataReader.Read();

            //Read data 15 times per previous printed page
            for (int i = 0; i < printedPages * 15+1;)
            {
                

                if (dataReader.GetInt32("cubbyholenumber") == currentCubby) {
                    dataReader.Read();
                }
                else
                {

                    currentCubby = dataReader.GetInt32("cubbyholenumber");
                    i++;
                }
            }
            //Reset cubby number
            currentCubby = 0;


            //Performing a do/while loop instead of while loop as there is no way to reverse data reader
            do
            {

                //Actions to take if a new cubby is detected
                if (dataReader.GetInt32("cubbyholenumber") != currentCubby)
                {
                    printedCubbys++;

                    //If the maximum number of cubbys per pages has been reached
                    if (printedCubbys > 15)
                    {
                        ev.HasMorePages = true;
                        break;
                    }
                    currentDefects = 0;
                    currentCubby = dataReader.GetInt32("cubbyholenumber");
                    ulCorner.X = ev.MarginBounds.Width / 3 * (int)(printedCubbys / 6.1);
                    ulCorner.Y = ev.MarginBounds.Height / 7 * ((printedCubbys % 6 == 0) ? 6 : printedCubbys % 6);
                    urCorner.X = ev.MarginBounds.Width / 3 * (int)(printedCubbys / 6.1) + ev.MarginBounds.Width / 9;
                    urCorner.Y = ulCorner.Y;
                    llCorner.X = ulCorner.X;
                    llCorner.Y = ev.MarginBounds.Height / 7 * (1 + ((printedCubbys % 6 == 0) ? 6 : printedCubbys % 6));
                    image = Image.FromFile(getImageLocation(dataReader.GetString("timestamp")));
                    PointF[] imageLoc = { ulCorner, urCorner, llCorner };
                    ev.Graphics.DrawImage(image, imageLoc);
                    urCorner.X += 3;
                    ev.Graphics.DrawString("Cubbyhole: " + currentCubby, printFont, Brushes.Black, urCorner);
                }


                //If runs out of room to print defects
                if (currentDefects++ > 6)
                {
                    urCorner.Y += printFont.GetHeight(ev.Graphics);
                    ev.Graphics.DrawString("More defects not listed" + currentCubby, printFont, Brushes.Black, urCorner);
                }

                //Print the defect
                else
                {
                    //Load the correct values for offsets and adjustment factors
                    if (dataReader.IsDBNull(15))
                    {
                        offset = 0;
                    }
                    else
                    {
                        offset = dataReader.GetDouble("offset");
                    }

                    if (dataReader.IsDBNull(13))
                    {
                        adj_factor = 1;
                    }
                    else
                    {
                        adj_factor = dataReader.GetDouble("Adjustment_factor");
                    }


                    //Print the defect
                    urCorner.Y += printFont.GetHeight(ev.Graphics);

                    ev.Graphics.DrawString(dataReader.GetString("Measurement") + " : " + Math.Round(dataReader.GetDouble("Result") * adj_factor + offset, 3), printFont, Brushes.Black, urCorner);
                }

            } while (dataReader.Read());


            printedPages++;

            


            dataReader.Close();

            database.CloseConnection();



        }
    }
}


//Database connection class

public class DBConnect
{
    private MySqlConnection connection;
    private string server;
    private string database;
    private string uid;
    private string password;
    private string connectionString;

    public DBConnect()
    {
        Initialize();
    }

    ~DBConnect()
    {
        CloseConnection();
    }

    private void Initialize()
    {
        //Database connection information
        server = "192.168.192.49";
        database = "qcvision";
        uid = "QCStaff";
        password = "Precision";

        connectionString = "SERVER=" + server + ";" + "DATABASE=" +
        database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";" + "SSLmode=none;";

        connection = new MySqlConnection(connectionString);

        OpenConnection();

    }
    //open connection to database
    private bool OpenConnection()
    {
        {
            try
            {
                connection.Open();

                return true;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        MessageBox.Show("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        MessageBox.Show("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }
    }

    //Close connection
    public bool CloseConnection()
    {
        try
        {
            connection.Close();


            return true;
        }
        catch (MySqlException ex)
        {
            MessageBox.Show(ex.Message);
            return false;
        }
    }


    //Select statement
    public MySqlDataReader Select(string query)
    {
        MySqlDataReader dataReader = null;
        //Open connection


        //Create Command
        MySqlCommand cmd = new MySqlCommand(query, connection);
        //Create a data reader and Execute the command
        dataReader = cmd.ExecuteReader();


        return dataReader;

    }


}

