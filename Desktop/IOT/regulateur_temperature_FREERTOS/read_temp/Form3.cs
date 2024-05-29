using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Mail;
using Microsoft.VisualBasic;

namespace read_temp
{
    public partial class Form3 : Form
    {
        
        public Form3()
        {
            InitializeComponent();
            this.MaximizeBox = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string destinataire = textBox1.Text;


            string expediteur = "yahya.frioui@etudiant-enit.utm.tn";
            string stmp =label1.Text;

            // Configuration du client SMTP (utilisez le serveur SMTP de votre fournisseur de messagerie)
            SmtpClient clientSmtp = new SmtpClient("smtp.gmail.com");
            clientSmtp.Port = 587;
            clientSmtp.Credentials = new NetworkCredential(expediteur, stmp);
            clientSmtp.EnableSsl = true;
           

            // Création du message
            MailMessage message = new MailMessage(expediteur, destinataire, "Adresse & mot de passe", "Adresse: ENIT \n mot de passe : ENIT ");

            try
            {
                // Envoi de l'e-mail
                clientSmtp.Send(message);
                MessageBox.Show("E-mail envoyé avec succès !");
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'envoi de l'e-mail : {ex.Message}");
            }
            finally
            {
                // Libérer les ressources
                message.Dispose();
                this.Hide();
                this.Close();
            }
        


    }

       
    }
}
