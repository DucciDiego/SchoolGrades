﻿using SchoolGrades.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SchoolGrades
{
    public partial class frmImages : Form
    {
        // TODO manage an order of the images of the lesson (database and code) 

        List<BusinessObjects.Image> listImages;
        private Lesson currentLesson;
        private Class currentClass;
        private BusinessObjects.Image currentImage; 
        private SchoolSubject currentSubject;
        private bool imageChanged = false; 

        ImagesFormType type;
        private string lessonImagesPath;
        private int currentIndexInImages;
        private SchoolPeriod currentSchoolPeriod;
        private string oldImage;

        public enum ImagesFormType
        {
            NormalManagement,
            ShowImage,
        }
        public frmImages(ImagesFormType Type, Lesson Lesson, Class Class, 
            List<SchoolGrades.BusinessObjects.Image> Images, SchoolSubject Subject)
        {
            InitializeComponent();

            listImages = Images;
            currentLesson = Lesson;
            currentClass = Class;
            currentSubject = Subject; 

            type = Type;

            lessonImagesPath = currentClass.SchoolYear +
                    currentClass.Abbreviation + "\\Lessons" +
                    "\\" + currentLesson.IdSchoolSubject;
        }
        private void frmImages_Load(object sender, EventArgs e)
        {
            txtLessonDate.Text = currentLesson.Date.ToString();
            txtSchoolYear.Text = currentClass.SchoolYear;
            txtClass.Text = currentClass.Abbreviation;

            txtSchoolSubject.Text = currentLesson.IdSchoolSubject;
            txtLessonCode.Text = currentLesson.IdLesson.ToString(); ;
            txtLessonDesc.Text = currentLesson.Note;

            refreshUi(currentIndexInImages); 

            if (type == ImagesFormType.ShowImage)
            {
                picImage.Location = new System.Drawing.Point(0, 0);
                picImage.Size = this.Size;
                currentImage = listImages[0];
                loadCurrentImage();
                //picImage.Load(Commons.PathImages + "\\"+ listImages[0].RelativePathAndFilename);
            }
            else if (type == ImagesFormType.NormalManagement)
            {
                dgwLessonsImages.DataSource = listImages;
            }
            if (listImages.Count > 0)
            {
                currentImage = listImages[0];
                loadCurrentImage();
            }
            else
            {
                currentImage = new BusinessObjects.Image();
            }

            if (currentSubject != null)
            {
                int col = Safe.Int(currentSubject.Color;
                this.BackColor = Commons.ColorFromNumber(currentSubject);
                rdbAutoRename_CheckedChanged(null, null);
            }
        }
        private void loadCurrentImage()
        {
            try
            {
                picImage.Load(Commons.PathImages + "\\" + currentImage.RelativePathAndFilename);
                txtCaption.Text = currentImage.Caption;
            }
            catch {
                Console.Beep(); 
            }; 
        }
        private void txtPathImportImage_TextChanged(object sender, EventArgs e)
        {

        }
        private void txtPathImportImage_DoubleClick(object sender, EventArgs e)
        {
            if(txtPathImportImage.Text != "")
                Commons.ProcessStartLink(txtPathImportImage.Text);
        }
        private void btnPathImportImage_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = txtPathImportImage.Text;
            DialogResult r = folderBrowserDialog1.ShowDialog();
            if (r == System.Windows.Forms.DialogResult.OK)
            {
                txtPathImportImage.Text = folderBrowserDialog1.SelectedPath;
            }
        }
        private void btnChooseFileImage_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = ""; 
            openFileDialog1.InitialDirectory = txtPathImportImage.Text;
            DialogResult r = openFileDialog1.ShowDialog();
            if (r == System.Windows.Forms.DialogResult.OK)
            {
                txtFileImportImage.Text = Path.GetFileName(openFileDialog1.FileName);
                txtPathImportImage.Text = Path.GetDirectoryName(openFileDialog1.FileName);
            }
            if (openFileDialog1.FileName != "")
            {
                try
                {
                    picImage.Load(openFileDialog1.FileName);
                    List<string> captions = Commons.bl.GetCaptionsOfThisImage(txtFileImportImage.Text);
                    if (captions.Count > 0)
                        // this SHOULD take the last caption that this image has had
                        // (but if it doesn't it doesn't matter much)
                        txtCaption.Text = captions[captions.Count - 1];
                    else
                        txtCaption.Text = "";
                    string currentImage = Path.Combine(txtPathImportImage.Text, txtFileImportImage.Text); 
                    if (currentImage != oldImage)
                    {
                        imageChanged = true; 
                    }
                    oldImage = currentImage; 
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Immagine non caricata.\nFormato non supportato?"); 
                }
            }
        }
        private void btnAddImage_Click(object sender, EventArgs e)
        {
            if (txtFileImportImage.Text != "")
            {
                string sourcePathAndFileName = txtPathImportImage.Text + "\\" +
                    txtFileImportImage.Text;
                // if the chosen file is already in the image path, the program
                // will avoid copying it under the image path and will link to the 
                // existing file instead 
                if (txtPathImportImage.Text.Contains(Commons.PathImages))
                {
                    // chosen file is inside the images path 
                    // does not copy and rename the file
                    // this spares HDD space on teachers' machine 
                    justLinkFileToLesson(sourcePathAndFileName);
                }
                else
                {
                    // chosen file is outside the images path 
                    copyFileToImagesAndLinkToLessons(sourcePathAndFileName);
                }
                try
                {
                    picImage.Load(sourcePathAndFileName);
                }
                catch
                {
                    Console.Beep();
                };
                // goto the last in the grid (the one just added) 
                currentIndexInImages = dgwLessonsImages.Rows.Count;
                if (currentIndexInImages < 0)
                    currentIndexInImages = 0;
                refreshUi(currentIndexInImages);
                // load data from the last 
                currentImage = ((List<BusinessObjects.Image>)dgwLessonsImages.DataSource)[dgwLessonsImages.Rows.Count - 1];
                loadCurrentImage();
            }
        }
        private void btnRemoveImage_Click(object sender, EventArgs e)
        {
            // values before 
            int oldIndex = currentIndexInImages;

            if (dgwLessonsImages.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selezionare nella griglia un'immagine da cancellare");
                return;
            }
            picImage.Image = null;
            // read from grid the data of the image to delete
            currentImage = ((List<BusinessObjects.Image>)dgwLessonsImages.DataSource)[dgwLessonsImages.SelectedRows[0].Index];
            DialogResult r = MessageBox.Show(" (Sì) Cancella anche il FILE dell'immagine '" + currentImage.Caption + "';" +
                "\n (No) Cancella il solo collegamento a questa lezione; " +
                "\n (Annulla) Non cancella nulla.",
                "Cancellazione Immagine", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (r == DialogResult.Yes)
            {
                Commons.bl.RemoveImageFromLesson(currentLesson, currentImage, true);
            }
            else if (r == DialogResult.No)
                Commons.bl.RemoveImageFromLesson(currentLesson, currentImage, false);
            else
                return; 

            if (currentIndexInImages > 0)
                currentIndexInImages = oldIndex - 1;
            try
            {
                currentImage = ((List<BusinessObjects.Image>)dgwLessonsImages.DataSource)[currentIndexInImages];
            }
            catch { }
            loadCurrentImage();
            refreshUi(currentIndexInImages);
        }
        private void btnSubFolderStorage_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = Commons.PathImages + "\\" + txtSubFolderStorage.Text;
            DialogResult r = folderBrowserDialog1.ShowDialog();
            if (r == System.Windows.Forms.DialogResult.OK)
            {
                string subPath = folderBrowserDialog1.SelectedPath;
                subPath = subPath.Remove(0, Commons.PathImages.Length + 1);
                txtSubFolderStorage.Text = subPath;
            }
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (dgwLessonsImages.SelectedRows != null)
            {
                int localIndex = dgwLessonsImages.SelectedRows[0].Index;
                currentImage.Caption = txtCaption.Text;
                if (imageChanged)
                {
                    string currentFile = Path.Combine(txtPathImportImage.Text, txtFileImportImage.Text);
                    currentImage.RelativePathAndFilename = currentFile.Remove(0, Commons.PathImages.Length + 1); 
                }
                Commons.bl.SaveImage(currentImage);
                imageChanged = false; 
                currentIndexInImages = localIndex;
                refreshUi(currentIndexInImages);
                currentImage = ((List<BusinessObjects.Image>)dgwLessonsImages.DataSource)[localIndex];
                loadCurrentImage();
                //txtCaption.Text = currentImage.Caption;
                //DgwLessonsImages.Rows[localIndex].Selected = false;
                //DgwLessonsImages.Rows[localIndex].Selected = true;
            }
        }
        private void justLinkFileToLesson(string sourcePathAndFileName)
        {
            BusinessObjects.Image currentImage = Commons.bl.FindImageWithGivenFile(sourcePathAndFileName);
            // if the image that reference to the file isn't anymore in the database 
            // create a new image that references to this file 
            // (eg. if the lesson has been deleted from the database together with its images, but 
            // the file is (somehow) still there) 
            if (currentImage == null)
            {
                currentImage = new BusinessObjects.Image();
                currentImage.IdImage = 0;
                currentImage.RelativePathAndFilename = sourcePathAndFileName.Remove(0,Commons.PathImages.Length + 1);
            }
            currentImage.Caption = txtCaption.Text;
            Commons.bl.LinkOneImageToLesson(currentImage, currentLesson);
        }
        private void copyFileToImagesAndLinkToLessons(string sourcePathAndFileName)
        {
            string ext = Path.GetExtension(sourcePathAndFileName);
            if (rdbAutoRename.Checked)
                lessonImagesPath = txtSubFolderStorage.Text + "\\"; // + currentLesson.IdSchoolSubject;
            string destinationFileName = "";
            string destinationPathAndFileName = "";

            if (rdbAutoRename.Checked)
            {
                string tempFileName;
                if (chkMantainOldFileName.Checked)
                    tempFileName = ((DateTime)currentLesson.Date).ToString("yyyy-MM-dd") + "_" +
                        currentLesson.IdSchoolSubject + "-xggR" +
                        "_" + txtFileImportImage.Text;
                else
                {
                    tempFileName = ((DateTime)currentLesson.Date).ToString("yyyy-MM-dd") + "_L_" +
                    currentClass.Abbreviation + currentClass.SchoolYear +
                    currentLesson.IdSchoolSubject + "-xggR";
                    tempFileName += ext;
                }
                int i = 1;
                do
                {
                    destinationPathAndFileName = Commons.PathImages + "\\" +
                        lessonImagesPath + 
                        tempFileName.Replace("xggR", (i++).ToString("00"));
                } while (File.Exists(destinationPathAndFileName));
                destinationFileName = tempFileName.Replace("xggR", (--i).ToString("00"));
                currentImage.RelativePathAndFilename = lessonImagesPath +  destinationFileName;
            }
            else if (rdbManualRename.Checked)
            {
                destinationFileName = txtFileImportImage.Text;
                destinationPathAndFileName = Commons.PathImages + "\\" +
                    lessonImagesPath +  destinationFileName;
                if (File.Exists(destinationPathAndFileName))
                {
                    MessageBox.Show("Il file " + destinationPathAndFileName + " esiste già.");
                    return;
                }
                currentImage.RelativePathAndFilename = lessonImagesPath +  destinationFileName;
            }

            if (!File.Exists(sourcePathAndFileName))
            {
                MessageBox.Show("Il file " + sourcePathAndFileName + " non esiste!");
                return;
            }
            // if it doesn't exist, create the folder of the images of the lessons of the class
            if (!Directory.Exists(Commons.PathImages + "\\" + lessonImagesPath))
            {
                Directory.CreateDirectory(Commons.PathImages + "\\" + lessonImagesPath);
            }
            File.Copy(sourcePathAndFileName, destinationPathAndFileName);
            //currentImage.IdImage = int.MaxValue; 
            currentImage.Caption = txtCaption.Text;
            currentImage.IdImage = 0; // to force creation of a new record
            Commons.bl.LinkOneImageToLesson(currentImage, currentLesson);
        }
        private void picImage_Click(object sender, EventArgs e)
        {

        }
        private void picImage_DoubleClick(object sender, EventArgs e)
        {
            Commons.ProcessStartLink(Path.Combine(Commons.PathImages,  
                currentImage.RelativePathAndFilename));
        }
        private void rdbAutoRename_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbAutoRename.Checked)
            {
                lblSubFolderStorage.Visible = true;
                txtSubFolderStorage.Visible = true;
                btnSubFolderStorage.Visible = true;
                chkMantainOldFileName.Visible = true;
                this.Size = new System.Drawing.Size(1240, 768);
                txtSubFolderStorage.Text = lessonImagesPath;
            }
        }
        private void rdbManualRename_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbManualRename.Checked)
            {
                lblSubFolderStorage.Visible = false;
                txtSubFolderStorage.Visible = false;
                btnSubFolderStorage.Visible = false;
                chkMantainOldFileName.Visible = false;
                this.Size = new System.Drawing.Size(724, 768);
            }
        }
        private void txtSubFolderStorage_TextChanged(object sender, EventArgs e)
        {

        }
        private void txtSubFolderStorage_DoubleClick(object sender, EventArgs e)
        {
            string directory = Path.Combine(Commons.PathImages, txtSubFolderStorage.Text);
            if (txtSubFolderStorage.Text != "" && Directory.Exists(directory))
                Commons.ProcessStartLink(directory);
            else
                Console.Beep(); 
        }
        private void refreshUi(int IndexInImages)
        {
            // refresh images in grid
            List<BusinessObjects.Image> l = Commons.bl.GetListLessonsImages(currentLesson); 
            dgwLessonsImages.DataSource = l;
            if (l.Count > 0)
            {
                try
                {
                    dgwLessonsImages.Rows[IndexInImages].Selected = true;
                }
                catch
                {

                }
            }
            else
            {
                picImage.Image = null;
                txtCaption.Text = ""; 
            }
            txtFileImportImage.Text = ""; 
        }
        private void frmImages_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.WindowState = FormWindowState.Normal;
                this.TopMost = false;
                this.FormBorderStyle = FormBorderStyle.Sizable;
            }
        }
        private void previousImage()
        {
            if (listImages.Count > 0)
            {
                dgwLessonsImages.Rows[currentIndexInImages].Selected = false;
                if (currentIndexInImages == 0)
                    currentIndexInImages = listImages.Count;
                currentIndexInImages--;
                currentImage = (BusinessObjects.Image)listImages[currentIndexInImages];
                loadCurrentImage();
                dgwLessonsImages.Rows[currentIndexInImages].Selected = true;
            }
        }
        private void nextImage()
        {
            if (listImages.Count > 0)
            {
                dgwLessonsImages.Rows[currentIndexInImages].Selected = false;
                currentIndexInImages = ++currentIndexInImages % listImages.Count;
                currentImage = (BusinessObjects.Image)listImages[currentIndexInImages];
                loadCurrentImage();
                dgwLessonsImages.Rows[currentIndexInImages].Selected = true;
            }
        }
        private void lastImage()
        {
            if (listImages.Count > 0)
            {
                dgwLessonsImages.Rows[currentIndexInImages].Selected = false;
                currentIndexInImages = listImages.Count - 1;
                currentImage = (BusinessObjects.Image)listImages[currentIndexInImages];
                loadCurrentImage();
                dgwLessonsImages.Rows[currentIndexInImages].Selected = true;
            }
        }
        private void firstImage()
        {
            if (listImages.Count > 0)
            {
                dgwLessonsImages.Rows[currentIndexInImages].Selected = false;
                currentIndexInImages = 0;
                currentImage = (BusinessObjects.Image)listImages[currentIndexInImages];
                loadCurrentImage();
                dgwLessonsImages.Rows[currentIndexInImages].Selected = true;
            }
        }
        private void grpPeriodOfQuestionsTopics_Enter(object sender, EventArgs e) {}
        private void dgwLessonsImages_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void dgwLessonsImages_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //if (e.RowIndex > -1)
            //{
            //    DgwLessonsImages.Rows[e.RowIndex].Selected = true;
            //    currentIndexInImages = e.RowIndex;
            //    currentImage = ((List<DbClasses.Image>)DgwLessonsImages.DataSource)[e.RowIndex];
            //    LoadImage();
            //}
        }
        private void dgwLessonsImages_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                dgwLessonsImages.Rows[e.RowIndex].Selected = true;
                currentImage = ((List<BusinessObjects.Image>)dgwLessonsImages.DataSource)[e.RowIndex];
                currentIndexInImages = e.RowIndex;
                loadCurrentImage();
            }
        }
        private void dgwLessonsImages_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            string RelativePath = Path.GetDirectoryName(currentImage.RelativePathAndFilename); 
            Commons.ProcessStartLink(Path.Combine(Commons.PathImages , RelativePath));
        }
        private void btnNextImage_Click(object sender, EventArgs e)
        {
            nextImage(); 
        }
        private void btnPreviousImage_Click(object sender, EventArgs e)
        {
            previousImage(); 
        }
        private void btnFirstImage_Click(object sender, EventArgs e)
        {
            firstImage();
        }
        private void btnLastImage_Click(object sender, EventArgs e)
        {
            lastImage(); 
        }
        private void cmbSchoolPeriod_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentSchoolPeriod = (SchoolPeriod)(cmbSchoolPeriod.SelectedValue);
            if (currentSchoolPeriod.IdSchoolPeriodType != "N")
            {
                dtpStartPeriod.Value = (DateTime)currentSchoolPeriod.DateStart;
                dtpEndPeriod.Value = (DateTime)currentSchoolPeriod.DateFinish;
            }
            else if (currentSchoolPeriod.IdSchoolPeriod == "month")
            {
                dtpStartPeriod.Value = DateTime.Now.AddMonths(-1);
                dtpEndPeriod.Value = DateTime.Now;
            }
            else if (currentSchoolPeriod.IdSchoolPeriod == "week")
            {
                dtpStartPeriod.Value = DateTime.Now.AddDays(-7);
                dtpEndPeriod.Value = DateTime.Now;
            }
            else if (currentSchoolPeriod.IdSchoolPeriod == "year")
            {
                dtpStartPeriod.Value = DateTime.Now.AddYears(-1);
                dtpEndPeriod.Value = DateTime.Now;
            }
        }
    }
}
