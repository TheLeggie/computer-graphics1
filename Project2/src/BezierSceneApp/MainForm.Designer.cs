namespace BezierSceneApp
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            drawingPanel = new DrawingPanel();
            controlPanel = new Panel();
            showFilledCheckBox = new CheckBox();
            showMeshCheckBox = new CheckBox();
            showPolygonsCheckBox = new CheckBox();
            divisionsTrackBar = new TrackBar();
            divisionsLabel = new Label();
            kdLabel = new Label();
            kdTrackBar = new TrackBar();
            ksLabel = new Label();
            ksTrackBar = new TrackBar();
            mLabel = new Label();
            mTrackBar = new TrackBar();
            lightZLabel = new Label();
            lightZTrackBar = new TrackBar();
            lightColorButton = new Button();
            lightColorDialog = new ColorDialog();
            surfaceComboBox = new ComboBox();
            objectColorGroupBox = new GroupBox();
            solidColorRadioButton = new RadioButton();
            textureRadioButton = new RadioButton();
            objectColorButton = new Button();
            loadTextureButton = new Button();
            useNormalMapCheckBox = new CheckBox();
            loadNormalMapButton = new Button();
            objectColorDialog = new ColorDialog();
            imageOpenFileDialog = new OpenFileDialog();
            controlPanel.SuspendLayout();
            objectColorGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)divisionsTrackBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)kdTrackBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ksTrackBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)mTrackBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lightZTrackBar).BeginInit();
            SuspendLayout();
            // 
            // drawingPanel
            // 
            drawingPanel.BackColor = Color.White;
            drawingPanel.Dock = DockStyle.Fill;
            drawingPanel.Location = new Point(0, 0);
            drawingPanel.Name = "drawingPanel";
            drawingPanel.Size = new Size(800, 750);
            drawingPanel.TabIndex = 0;
            // 
            // controlPanel
            // 
            controlPanel.AutoScroll = true;
            controlPanel.Controls.Add(loadNormalMapButton);
            controlPanel.Controls.Add(useNormalMapCheckBox);
            controlPanel.Controls.Add(objectColorGroupBox);
            controlPanel.Controls.Add(surfaceComboBox);
            controlPanel.Controls.Add(lightColorButton);
            controlPanel.Controls.Add(lightZTrackBar);
            controlPanel.Controls.Add(lightZLabel);
            controlPanel.Controls.Add(mTrackBar);
            controlPanel.Controls.Add(mLabel);
            controlPanel.Controls.Add(ksTrackBar);
            controlPanel.Controls.Add(ksLabel);
            controlPanel.Controls.Add(kdTrackBar);
            controlPanel.Controls.Add(kdLabel);
            controlPanel.Controls.Add(showFilledCheckBox);
            controlPanel.Controls.Add(showMeshCheckBox);
            controlPanel.Controls.Add(showPolygonsCheckBox);
            controlPanel.Controls.Add(divisionsTrackBar);
            controlPanel.Controls.Add(divisionsLabel);
            controlPanel.Dock = DockStyle.Right;
            controlPanel.Location = new Point(800, 0);
            controlPanel.Name = "controlPanel";
            controlPanel.Padding = new Padding(8);
            controlPanel.Size = new Size(200, 750);
            controlPanel.TabIndex = 1;
            // 
            // showFilledCheckBox
            // 
            showFilledCheckBox.AutoSize = true;
            showFilledCheckBox.Checked = true;
            showFilledCheckBox.CheckState = CheckState.Checked;
            showFilledCheckBox.Location = new Point(11, 140);
            showFilledCheckBox.Name = "showFilledCheckBox";
            showFilledCheckBox.Size = new Size(138, 19);
            showFilledCheckBox.TabIndex = 4;
            showFilledCheckBox.Text = "Wypełnione trójkąty";
            showFilledCheckBox.UseVisualStyleBackColor = true;
            showFilledCheckBox.CheckedChanged += showFilledCheckBox_CheckedChanged;
            // 
            // showMeshCheckBox
            // 
            showMeshCheckBox.AutoSize = true;
            showMeshCheckBox.Checked = true;
            showMeshCheckBox.CheckState = CheckState.Checked;
            showMeshCheckBox.Location = new Point(11, 115);
            showMeshCheckBox.Name = "showMeshCheckBox";
            showMeshCheckBox.Size = new Size(117, 19);
            showMeshCheckBox.TabIndex = 3;
            showMeshCheckBox.Text = "Siatka trójkątów";
            showMeshCheckBox.UseVisualStyleBackColor = true;
            showMeshCheckBox.CheckedChanged += showMeshCheckBox_CheckedChanged;
            // 
            // showPolygonsCheckBox
            // 
            showPolygonsCheckBox.AutoSize = true;
            showPolygonsCheckBox.Checked = true;
            showPolygonsCheckBox.CheckState = CheckState.Checked;
            showPolygonsCheckBox.Location = new Point(11, 90);
            showPolygonsCheckBox.Name = "showPolygonsCheckBox";
            showPolygonsCheckBox.Size = new Size(126, 19);
            showPolygonsCheckBox.TabIndex = 2;
            showPolygonsCheckBox.Text = "Wieloboki Béziera";
            showPolygonsCheckBox.UseVisualStyleBackColor = true;
            showPolygonsCheckBox.CheckedChanged += showPolygonsCheckBox_CheckedChanged;
            // 
            // divisionsTrackBar
            // 
            divisionsTrackBar.Location = new Point(11, 30);
            divisionsTrackBar.Maximum = 40;
            divisionsTrackBar.Minimum = 4;
            divisionsTrackBar.Name = "divisionsTrackBar";
            divisionsTrackBar.Size = new Size(178, 45);
            divisionsTrackBar.TabIndex = 1;
            divisionsTrackBar.TickFrequency = 4;
            divisionsTrackBar.Value = 16;
            divisionsTrackBar.ValueChanged += divisionsTrackBar_ValueChanged;
            // 
            // divisionsLabel
            // 
            divisionsLabel.AutoSize = true;
            divisionsLabel.Location = new Point(11, 10);
            divisionsLabel.Name = "divisionsLabel";
            divisionsLabel.Size = new Size(140, 15);
            divisionsLabel.TabIndex = 0;
            divisionsLabel.Text = "Dokładność triangulacji: 16";
            // 
            // kdLabel
            // 
            kdLabel.AutoSize = true;
            kdLabel.Location = new Point(11, 175);
            kdLabel.Name = "kdLabel";
            kdLabel.Size = new Size(50, 15);
            kdLabel.TabIndex = 5;
            kdLabel.Text = "kd: 0.70";
            // 
            // kdTrackBar
            // 
            kdTrackBar.Location = new Point(11, 195);
            kdTrackBar.Maximum = 100;
            kdTrackBar.Name = "kdTrackBar";
            kdTrackBar.Size = new Size(178, 45);
            kdTrackBar.TabIndex = 6;
            kdTrackBar.TickFrequency = 10;
            kdTrackBar.Value = 70;
            kdTrackBar.ValueChanged += lightingTrackBar_ValueChanged;
            // 
            // ksLabel
            // 
            ksLabel.AutoSize = true;
            ksLabel.Location = new Point(11, 245);
            ksLabel.Name = "ksLabel";
            ksLabel.Size = new Size(50, 15);
            ksLabel.TabIndex = 7;
            ksLabel.Text = "ks: 0.50";
            // 
            // ksTrackBar
            // 
            ksTrackBar.Location = new Point(11, 265);
            ksTrackBar.Maximum = 100;
            ksTrackBar.Name = "ksTrackBar";
            ksTrackBar.Size = new Size(178, 45);
            ksTrackBar.TabIndex = 8;
            ksTrackBar.TickFrequency = 10;
            ksTrackBar.Value = 50;
            ksTrackBar.ValueChanged += lightingTrackBar_ValueChanged;
            // 
            // mLabel
            // 
            mLabel.AutoSize = true;
            mLabel.Location = new Point(11, 315);
            mLabel.Name = "mLabel";
            mLabel.Size = new Size(40, 15);
            mLabel.TabIndex = 9;
            mLabel.Text = "m: 20";
            // 
            // mTrackBar
            // 
            mTrackBar.Location = new Point(11, 335);
            mTrackBar.Maximum = 100;
            mTrackBar.Minimum = 1;
            mTrackBar.Name = "mTrackBar";
            mTrackBar.Size = new Size(178, 45);
            mTrackBar.TabIndex = 10;
            mTrackBar.TickFrequency = 10;
            mTrackBar.Value = 20;
            mTrackBar.ValueChanged += lightingTrackBar_ValueChanged;
            // 
            // lightZLabel
            // 
            lightZLabel.AutoSize = true;
            lightZLabel.Location = new Point(11, 385);
            lightZLabel.Name = "lightZLabel";
            lightZLabel.Size = new Size(80, 15);
            lightZLabel.TabIndex = 11;
            lightZLabel.Text = "z światła: 2.0";
            // 
            // lightZTrackBar
            // 
            lightZTrackBar.Location = new Point(11, 405);
            lightZTrackBar.Maximum = 50;
            lightZTrackBar.Minimum = 5;
            lightZTrackBar.Name = "lightZTrackBar";
            lightZTrackBar.Size = new Size(178, 45);
            lightZTrackBar.TabIndex = 12;
            lightZTrackBar.TickFrequency = 5;
            lightZTrackBar.Value = 20;
            lightZTrackBar.ValueChanged += lightingTrackBar_ValueChanged;
            // 
            // lightColorButton
            // 
            lightColorButton.BackColor = Color.White;
            lightColorButton.Location = new Point(11, 455);
            lightColorButton.Name = "lightColorButton";
            lightColorButton.Size = new Size(178, 27);
            lightColorButton.TabIndex = 13;
            lightColorButton.Text = "Kolor światła";
            lightColorButton.UseVisualStyleBackColor = false;
            lightColorButton.Click += lightColorButton_Click;
            // 
            // lightColorDialog
            // 
            lightColorDialog.Color = Color.White;
            lightColorDialog.FullOpen = true;
            // 
            // surfaceComboBox
            // 
            surfaceComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            surfaceComboBox.Location = new Point(11, 495);
            surfaceComboBox.Name = "surfaceComboBox";
            surfaceComboBox.Size = new Size(178, 23);
            surfaceComboBox.TabIndex = 14;
            surfaceComboBox.SelectedIndexChanged += surfaceComboBox_SelectedIndexChanged;
            // 
            // objectColorGroupBox
            // 
            objectColorGroupBox.Controls.Add(loadTextureButton);
            objectColorGroupBox.Controls.Add(objectColorButton);
            objectColorGroupBox.Controls.Add(textureRadioButton);
            objectColorGroupBox.Controls.Add(solidColorRadioButton);
            objectColorGroupBox.Location = new Point(11, 525);
            objectColorGroupBox.Name = "objectColorGroupBox";
            objectColorGroupBox.Size = new Size(178, 135);
            objectColorGroupBox.TabIndex = 15;
            objectColorGroupBox.TabStop = false;
            objectColorGroupBox.Text = "Kolor obiektu";
            // 
            // solidColorRadioButton
            // 
            solidColorRadioButton.AutoSize = true;
            solidColorRadioButton.Checked = true;
            solidColorRadioButton.Location = new Point(8, 22);
            solidColorRadioButton.Name = "solidColorRadioButton";
            solidColorRadioButton.Size = new Size(110, 19);
            solidColorRadioButton.TabIndex = 0;
            solidColorRadioButton.TabStop = true;
            solidColorRadioButton.Text = "Jednolity kolor";
            solidColorRadioButton.UseVisualStyleBackColor = true;
            // 
            // textureRadioButton
            // 
            textureRadioButton.AutoSize = true;
            textureRadioButton.Location = new Point(8, 45);
            textureRadioButton.Name = "textureRadioButton";
            textureRadioButton.Size = new Size(71, 19);
            textureRadioButton.TabIndex = 1;
            textureRadioButton.Text = "Tekstura";
            textureRadioButton.UseVisualStyleBackColor = true;
            textureRadioButton.CheckedChanged += textureRadioButton_CheckedChanged;
            // 
            // objectColorButton
            // 
            objectColorButton.Location = new Point(8, 70);
            objectColorButton.Name = "objectColorButton";
            objectColorButton.Size = new Size(162, 27);
            objectColorButton.TabIndex = 2;
            objectColorButton.Text = "Kolor obiektu";
            objectColorButton.UseVisualStyleBackColor = false;
            objectColorButton.Click += objectColorButton_Click;
            // 
            // loadTextureButton
            // 
            loadTextureButton.Location = new Point(8, 100);
            loadTextureButton.Name = "loadTextureButton";
            loadTextureButton.Size = new Size(162, 27);
            loadTextureButton.TabIndex = 3;
            loadTextureButton.Text = "Wczytaj teksturę...";
            loadTextureButton.UseVisualStyleBackColor = true;
            loadTextureButton.Click += loadTextureButton_Click;
            // 
            // useNormalMapCheckBox
            // 
            useNormalMapCheckBox.AutoSize = true;
            useNormalMapCheckBox.Location = new Point(11, 670);
            useNormalMapCheckBox.Name = "useNormalMapCheckBox";
            useNormalMapCheckBox.Size = new Size(118, 19);
            useNormalMapCheckBox.TabIndex = 16;
            useNormalMapCheckBox.Text = "Mapa normalnych";
            useNormalMapCheckBox.UseVisualStyleBackColor = true;
            useNormalMapCheckBox.CheckedChanged += useNormalMapCheckBox_CheckedChanged;
            // 
            // loadNormalMapButton
            // 
            loadNormalMapButton.Location = new Point(11, 695);
            loadNormalMapButton.Name = "loadNormalMapButton";
            loadNormalMapButton.Size = new Size(178, 27);
            loadNormalMapButton.TabIndex = 17;
            loadNormalMapButton.Text = "Wczytaj NormalMap...";
            loadNormalMapButton.UseVisualStyleBackColor = true;
            loadNormalMapButton.Click += loadNormalMapButton_Click;
            // 
            // objectColorDialog
            // 
            objectColorDialog.FullOpen = true;
            // 
            // imageOpenFileDialog
            // 
            imageOpenFileDialog.Filter = "Obrazy (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 750);
            Controls.Add(drawingPanel);
            Controls.Add(controlPanel);
            Name = "MainForm";
            Text = "Bezier Scene";
            objectColorGroupBox.ResumeLayout(false);
            objectColorGroupBox.PerformLayout();
            controlPanel.ResumeLayout(false);
            controlPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)divisionsTrackBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)kdTrackBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)ksTrackBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)mTrackBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)lightZTrackBar).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DrawingPanel drawingPanel;
        private Panel controlPanel;
        private Label divisionsLabel;
        private TrackBar divisionsTrackBar;
        private CheckBox showPolygonsCheckBox;
        private CheckBox showMeshCheckBox;
        private CheckBox showFilledCheckBox;
        private Label kdLabel;
        private TrackBar kdTrackBar;
        private Label ksLabel;
        private TrackBar ksTrackBar;
        private Label mLabel;
        private TrackBar mTrackBar;
        private Label lightZLabel;
        private TrackBar lightZTrackBar;
        private Button lightColorButton;
        private ColorDialog lightColorDialog;
        private ComboBox surfaceComboBox;
        private GroupBox objectColorGroupBox;
        private RadioButton solidColorRadioButton;
        private RadioButton textureRadioButton;
        private Button objectColorButton;
        private Button loadTextureButton;
        private CheckBox useNormalMapCheckBox;
        private Button loadNormalMapButton;
        private ColorDialog objectColorDialog;
        private OpenFileDialog imageOpenFileDialog;
    }
}
