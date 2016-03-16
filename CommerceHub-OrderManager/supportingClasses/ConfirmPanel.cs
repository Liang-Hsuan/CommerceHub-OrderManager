using System;
using System.Windows.Forms;

namespace CommerceHub_OrderManager.supportingClasses
{
    /* 
     * A panel that ask for user confirmation
     */
    public partial class ConfirmPanel : Form
    {
        /* constructor that initialize question string */
        public ConfirmPanel(string text)
        {
            InitializeComponent();

            questionLabel.Text = text;
        }

        /* get and set the text of question */
        public string Text
        {
            get
            {
                return questionLabel.Text;
            }

            set
            {
                questionLabel.Text = value;
            }
        }

        #region Buttons
        /* yes and no button clicks event */
        private void yesButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
        private void noButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
        #endregion
    }
}
