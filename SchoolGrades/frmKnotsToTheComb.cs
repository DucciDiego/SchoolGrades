﻿using SchoolGrades.BusinessObjects;
using System;
using System.Windows.Forms;

namespace SchoolGrades
{
    public partial class frmKnotsToTheComb : Form
    {
        private Question chosenQuestion = new Question();
        private frmMicroAssessment grandparentForm; 

        //private Class currentClass;
        private Student currentStudent;
        private SchoolSubject currentSubject;
        private string currentIdSchoolYear;
        private int currentIdGrade;

        bool isLoading = true;

        internal Question ChosenQuestion { get; private set; }

        public frmKnotsToTheComb(frmMicroAssessment GrandparentForm, int? IdStudent, SchoolSubject SchoolSubject, string Year)
        {
            InitializeComponent();
            currentStudent = Commons.dl.GetStudent(IdStudent);
            lblStudent.Text = currentStudent.LastName + " " + currentStudent.FirstName; 
            currentIdSchoolYear = Year;
            currentSubject = SchoolSubject;
            grandparentForm = GrandparentForm; 

            // fills the lookup tables' combos
            cmbSchoolSubject.DisplayMember = "Name";
            cmbSchoolSubject.ValueMember = "idSchoolSubject";
            cmbSchoolSubject.DataSource = Commons.dl.GetListSchoolSubjects(true);

            currentSubject = SchoolSubject; 
            ChosenQuestion = null; 
        }

        private void FrmKnotsToTheComb_Load(object sender, EventArgs e)
        {
            cmbSchoolSubject.SelectedValue = currentSubject.IdSchoolSubject; 

            RefreshData(); 
        }
        private void RefreshData()
        {
            dgwQuestions.DataSource = Commons.dl.GetUnfixedGrades(currentStudent, currentSubject.IdSchoolSubject, 60);
        }
        private void DgwQuestions_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void DgwQuestions_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                DataGridViewRow r = dgwQuestions.Rows[e.RowIndex];
                txtQuestionText.Text = (string)r.Cells["Text"].Value;
                currentIdGrade = Safe.Int(r.Cells["IdQuestion"].Value;
            }
        }
        private void DgwQuestions_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                dgwQuestions.Rows[e.RowIndex].Selected = true; 
            }
        }

        private void DgwQuestions_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // choose this question
            // !!!! TODO !!!!
        }

        private void BtnFix_Click(object sender, EventArgs e)
        {
            if (dgwQuestions.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selezionare la domanda che è stata riparata");
                return; 
            }
            DataGridViewRow r = dgwQuestions.SelectedRows[0];
            currentIdGrade = Safe.Int(r.Cells["IdGrade"].Value;
            if (MessageBox.Show("La domanda '" + (string)r.Cells["Text"].Value + "' è stata riparata?","Riparazione domanda",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Commons.bl.FixQuestionInGrade(currentIdGrade);
                RefreshData(); 
            }
        }

        private void btnChoose_Click(object sender, EventArgs e)
        {
            if (dgwQuestions.SelectedRows.Count > 0)
            {
                //int key = int.Parse(dgwQuestions.SelectedRows[0].Cells[6].Value.ToString());
                int key = Safe.Int( dgwQuestions.SelectedRows[0].Cells[6].Value;
                if (grandparentForm != null)
                {
                    // form called by student's assessment form 
                    grandparentForm.CurrentQuestion = Commons.bl.GetQuestionById(key);
                    grandparentForm.DisplayCurrentQuestion(); 
                }
            }
            else
            {
                MessageBox.Show("Scegliere una domanda nella griglia");
                return;
            }
        }

        private void cmbSchoolSubject_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.BackColor = Commons.ColorFromNumber(currentSubject);
        }
    }
}
