using System.Windows.Forms;

namespace read_temp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.MaximizeBox= false;
        }

        private void connexion_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "ENIT" && textBox2.Text=="ENIT")
            {

                this.Hide();
                Form2 f2 = new Form2(); ;
                f2.ShowDialog();
                //f2 = null;

            }
            else
            {
               
         
                    // Libérer les ressources
                    MessageBox.Show($"Adresse ou Mot de passe incorrecte ");
                
            }
        }

        

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
          
           // this.Close();
            Form3 f3 = new Form3(); ;
            f3.ShowDialog();           
           
        }

        
    }
}