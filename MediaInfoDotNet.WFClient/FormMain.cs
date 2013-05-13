﻿/******************************************************************************
 * MediaInfo.NET - A fast, easy-to-use .NET wrapper for MediaInfo.
 * Use at your own risk, under the same license as MediaInfo itself.
 * Copyright (C) 2013 Carsten Schlote
 ******************************************************************************
 * Test Application for MediaInfo.Net
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MediaInfoDotNet.WFClient.Properties;
using System.IO;

namespace MediaInfoDotNet.WFClient
{
	public partial class FormMain : Form
	{
		BindingList<MediaFile> _mediafiles;
		[ListBindable(true)]
		public BindingList<MediaFile> MediaFileCollection {
			get { if (_mediafiles == null) _mediafiles = new BindingList<MediaFile>(); return _mediafiles; }
			protected set { _mediafiles = value; }
		}

		public FormMain() {
			InitializeComponent();
			loadAllStreamProps(null);
		}

		private void FormMain_Load(object sender, EventArgs e) {
			this.Size = Settings.Default.WinSize;
		}
		private void FormMain_FormClosing(object sender, FormClosingEventArgs e) {
			Settings.Default.WinSize = this.Size;
		}
		private void FormMain_FormClosed(object sender, FormClosedEventArgs e) {
			if (Settings.Default.SaveOnExit) {
				Settings.Default.Save();
			}
		}

		private void loadMediaFile(object sender, EventArgs e) {
			MediaInfoDotNet.MediaFile mf;
			DialogResult rc = openFileDialog1.ShowDialog();
			if (rc == System.Windows.Forms.DialogResult.OK) {
				Settings.Default.LastFile = openFileDialog1.FileName;
				mf = new MediaFile(openFileDialog1.FileName);
				//FIXME mediaFiles.Add (mf);
				int rc2 = bindingSourceMediaFiles.Add(mf);
				//listBox1.SelectedIndex = rc2;
				bindingSourceMediaFiles.Position = rc2;
			}
		}

		private void loadMediaFiles(object sender, EventArgs e) {
			folderBrowserDialog1.SelectedPath = Settings.Default.LastLocation;
			DialogResult rc = folderBrowserDialog1.ShowDialog();
			if (rc == System.Windows.Forms.DialogResult.OK) {
				Settings.Default.LastLocation = folderBrowserDialog1.SelectedPath;
				backgroundWorker1.RunWorkerAsync(folderBrowserDialog1.SelectedPath);
			}
		}

		#region LoadThePropertyGrids stuff

		private void loadPropertyGridWithStream(NumericUpDown nudobj, PropertyGrid pgobj, decimal max, Object obj) {
			if (nudobj != null) {
				if (max > 0) {
					nudobj.Enabled = true;
					nudobj.Maximum = max - 1;
					if (nudobj.Value < 0 || nudobj.Value >= max) nudobj.Value = 0;
				} else {
					nudobj.Value = 0;
					nudobj.Maximum = 0;
					nudobj.Enabled = false;
				}
			}
			if (pgobj != null) {
				if (obj != null) {
					pgobj.SelectedObject = obj;
					pgobj.Enabled = true;
				} else {
					pgobj.SelectedObject = null;
					pgobj.Enabled = false;
				}
			}
		}

		private void loadGeneralStreamProps() {
			MediaFile mf = selectedMediaFile; Object obj = null;
			if (mf != null) obj = mf.General;
			loadPropertyGridWithStream(null, propertyGridGeneral, 0, obj);
		}
		private void loadVideoStreamProps() {
			MediaFile mf = selectedMediaFile;
			if (mf != null) {
				int val = (int)numericUpDownVideo.Value; Object obj = null;
				if (mf.Video.ContainsKey(val))
					obj = mf.Video[val];
				loadPropertyGridWithStream(numericUpDownVideo, propertyGridVideo, mf.Video.Count, obj);
			}
		}
		private void loadAudioStreamProps() {
			MediaFile mf = selectedMediaFile;
			if (mf != null) {
				int val = (int)numericUpDownAudio.Value; Object obj = null;
				if (mf.Audio.ContainsKey(val))
					obj = mf.Audio[val];
				loadPropertyGridWithStream(numericUpDownAudio, propertyGridAudio, mf.Audio.Count, obj);
			}
		}
		private void loadTextStreamProps() {
			MediaFile mf = selectedMediaFile;
			if (mf != null) {
				int val = (int)numericUpDownText.Value; Object obj = null;
				if (mf.Text.ContainsKey(val))
					obj = mf.Text[val];
				loadPropertyGridWithStream(numericUpDownText, propertyGridText, mf.Text.Count, obj);
			}
		}
		private void loadImageStreamProps() {
			MediaFile mf = selectedMediaFile;
			if (mf != null) {
				int val = (int)numericUpDownImage.Value; Object obj = null;
				if (mf.Image.ContainsKey(val))
					obj = mf.Image[val];
				loadPropertyGridWithStream(numericUpDownImage, propertyGridImage, mf.Image.Count, obj);
			}
		}
		private void loadOtherStreamProps() {
			MediaFile mf = selectedMediaFile;
			if (mf != null) {
				int val = (int)numericUpDownOther.Value; Object obj = null;
				if (mf.Other.ContainsKey(val))
					obj = mf.Other[val];
				loadPropertyGridWithStream(numericUpDownOther, propertyGridOther, mf.Other.Count, obj);
			}
		}
		private void loadMenuStreamProps() {
			MediaFile mf = selectedMediaFile;
			if (mf != null) {
				int val = (int)numericUpDownMenus.Value; Object obj = null;
				if (mf.Menu.ContainsKey(val))
					obj = mf.Menu[val];
				loadPropertyGridWithStream(numericUpDownMenus, propertyGridMenus, mf.Menu.Count, obj);
			}
		}
		private MediaFile selectedMediaFile; //FIXME
		private void loadAllStreamProps(MediaFile mf) {
			selectedMediaFile = mf;
			textBoxInform.Text = mf != null ? mf.General.miInform() : "No data.";

			propertyGridMediaFile.SelectedObject = mf;
			propertyGridMediaFile.Enabled = (mf == null) ? false : true;

			loadGeneralStreamProps();
			loadVideoStreamProps();
			loadAudioStreamProps();
			loadTextStreamProps();
			loadImageStreamProps();
			loadOtherStreamProps();
			loadMenuStreamProps();
		}

		#endregion

		#region WinForm Events Callbacks

		private void openFileToolStripMenuItem_Click(object sender, EventArgs e) {
			loadMediaFile(sender, e);
		}

		private void openFolderToolStripMenuItem_Click(object sender, EventArgs e) {
			loadMediaFiles(sender, e);
		}

		private void closeAllToolStripMenuItem_Click(object sender, EventArgs e) {
			if (MessageBox.Show(
				"Delete all scanned entries?",
				"Delete all entries",
				MessageBoxButtons.OKCancel,
				MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.OK) {
				this.bindingSourceMediaFiles.Clear();
			}
		}

		private void closeFileToolStripMenuItem_Click(object sender, EventArgs e) {
			if (listBox1.SelectedIndex >= 0) {
				this.bindingSourceMediaFiles.Remove(MediaFileCollection[listBox1.SelectedIndex]);
			}
		}

		private void numericUpDown1_ValueChanged(object sender, EventArgs e) {
			loadVideoStreamProps();
		}
		private void numericUpDown2_ValueChanged(object sender, EventArgs e) {
			loadAudioStreamProps();
		}
		private void numericUpDown3_ValueChanged(object sender, EventArgs e) {
			loadTextStreamProps();
		}
		private void numericUpDown4_ValueChanged(object sender, EventArgs e) {
			loadImageStreamProps();
		}
		private void numericUpDown5_ValueChanged(object sender, EventArgs e) {
			loadOtherStreamProps();
		}
		private void numericUpDown6_ValueChanged(object sender, EventArgs e) {
			loadMenuStreamProps();
		}
		private void saveSettingsNowToolStripMenuItem_Click(object sender, EventArgs e) {
			Settings.Default.Save();
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
			this.Close();
		}

		private void exportToolStripMenuItem_Click(object sender, EventArgs e) {
			System.Diagnostics.Debug.WriteLine("Booh");
		}

		private void editPreferencesToolStripMenuItem_Click(object sender, EventArgs e) {
			FormPrefs prefs = new FormPrefs();
			prefs.ShowDialog();
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
			MessageBox.Show(
				"This is a simple demo app to test and develop the MediaInfo.Net\n" +
				"classes.\n\n" +
				"The original code was written by Charles N. Burns\n" +
				"Modification and changes by Carsten Schlote\n\n",
				"Demo App for MediaInfo.Net",
				MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		FormHistogram formHistogram;
		private void histogramInformOutputToolStripMenuItem_Click(object sender, EventArgs e) {
			System.Diagnostics.Debug.WriteLine("Booh");
			formHistogram = new FormHistogram(this.MediaFileCollection);
			formHistogram.Show();
		}

		private void bindingSource1_ListChanged(object sender, ListChangedEventArgs e) {
			MediaFileCollection = (BindingList<MediaFile>)(bindingSourceMediaFiles.List);
		}

		private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
			MediaFile mf = null;
			if (bindingSourceMediaFiles.List.Count > 0 && listBox1.SelectedIndex >= 0 && listBox1.SelectedIndex < bindingSourceMediaFiles.List.Count) {
				//mf = MediaFileCollection[listBox1.SelectedIndex];
				mf = (MediaFile)listBox1.SelectedItem;
			}
			loadAllStreamProps(mf);
		}

		#endregion

		#region OpenDirectory Backgroundworker

		private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e) {
			MediaInfoDotNet.MediaFile mf;
			string selpath = (string)e.Argument;
			backgroundWorker1.ReportProgress(0, "Scanning directory...");
			string[] files = Directory.GetFiles(selpath, "*.*", SearchOption.AllDirectories);
			float progstep = 100.0f / files.Count(); float progress = 0.0f;
			foreach (string file in files) {
				progress += progstep;
				mf = new MediaFile(file);
				if (mf.hasStreams) {
					backgroundWorker1.ReportProgress((int)(progress), mf);
				} else {
					backgroundWorker1.ReportProgress((int)(progress), null);
				}
				if (backgroundWorker1.CancellationPending)
					break;
			}

		}
		List<MediaFile> newfiles = new List<MediaFile>();

		private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			if (e.UserState is String) {
				toolStripStatusLabel1.Text = (string)e.UserState;
			}
			if (e.UserState is MediaFile) {
				toolStripStatusLabel1.Text = String.Format("Scanned {0}", ((MediaFile)(e.UserState)).filePath);
				newfiles.Add((MediaFile)e.UserState);
			}
			toolStripProgressBar1.Value = e.ProgressPercentage <= 100 ? e.ProgressPercentage : 100;
			//this.Refresh();
		}

		private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			toolStripProgressBar1.Value = 100;
			listBox1.BeginUpdate();
			foreach (MediaFile mf in newfiles)
				bindingSourceMediaFiles.Add(mf);
			listBox1.EndUpdate();
			newfiles.Clear();
			toolStripStatusLabel1.Text = "Finished loading.";
		}

		private void abortOperationToolStripMenuItem_Click(object sender, EventArgs e) {
			backgroundWorker1.CancelAsync();
		}

		#endregion

	}
}
