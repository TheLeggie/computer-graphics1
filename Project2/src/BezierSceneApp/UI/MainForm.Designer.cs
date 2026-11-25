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
            components = new System.ComponentModel.Container();
            menuStrip = new MenuStrip();
            helpMenuItem = new ToolStripMenuItem();
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
            animateLightCheckBox = new CheckBox();
            showLightCurveCheckBox = new CheckBox();
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
            lightAnimationTimer = new System.Windows.Forms.Timer(components);
            menuStrip.SuspendLayout();
            controlPanel.SuspendLayout();
            objectColorGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)divisionsTrackBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)kdTrackBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ksTrackBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)mTrackBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lightZTrackBar).BeginInit();
            SuspendLayout();
            // 
            // menuStrip
            // 
            menuStrip.Items.AddRange(new ToolStripItem[] { helpMenuItem });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(1000, 24);
            menuStrip.TabIndex = 2;
            // 
            // helpMenuItem
            // 
            helpMenuItem.Name = "helpMenuItem";
            helpMenuItem.Size = new Size(44, 20);
            helpMenuItem.Text = "Help";
            helpMenuItem.Click += helpMenuItem_Click;
            // 
            // drawingPanel
            // 
            drawingPanel.BackColor = Color.White;
            drawingPanel.Dock = DockStyle.Fill;
            drawingPanel.Location = new Point(0, 24);
            drawingPanel.Name = "drawingPanel";
            drawingPanel.Size = new Size(800, 626);
            drawingPanel.TabIndex = 0;
            // 
            // controlPanel
            // 
            controlPanel.AutoScroll = true;
            controlPanel.Controls.Add(loadNormalMapButton);
            controlPanel.Controls.Add(useNormalMapCheckBox);
            controlPanel.Controls.Add(objectColorGroupBox);
            controlPanel.Controls.Add(surfaceComboBox);
            controlPanel.Controls.Add(showLightCurveCheckBox);
            controlPanel.Controls.Add(animateLightCheckBox);
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
            controlPanel.Location = new Point(800, 24);
            controlPanel.Name = "controlPanel";
            controlPanel.Padding = new Padding(6);
            controlPanel.Size = new Size(200, 626);
            controlPanel.TabIndex = 1;
            // 
            // showFilledCheckBox
            // 
            showFilledCheckBox.AutoSize = true;
            showFilledCheckBox.Checked = true;
            showFilledCheckBox.CheckState = CheckState.Checked;
            showFilledCheckBox.Location = new Point(11, 94);
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
            showMeshCheckBox.Location = new Point(11, 74);
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
            showPolygonsCheckBox.Location = new Point(11, 54);
            showPolygonsCheckBox.Name = "showPolygonsCheckBox";
            showPolygonsCheckBox.Size = new Size(126, 19);
            showPolygonsCheckBox.TabIndex = 2;
            showPolygonsCheckBox.Text = "Wieloboki Béziera";
            showPolygonsCheckBox.UseVisualStyleBackColor = true;
            showPolygonsCheckBox.CheckedChanged += showPolygonsCheckBox_CheckedChanged;
            // 
            // divisionsTrackBar
            // 
            divisionsTrackBar.AutoSize = false;
            divisionsTrackBar.Location = new Point(11, 22);
            divisionsTrackBar.Maximum = 40;
            divisionsTrackBar.Minimum = 4;
            divisionsTrackBar.Name = "divisionsTrackBar";
            divisionsTrackBar.Size = new Size(178, 28);
            divisionsTrackBar.TabIndex = 1;
            divisionsTrackBar.TickFrequency = 4;
            divisionsTrackBar.Value = 16;
            divisionsTrackBar.ValueChanged += divisionsTrackBar_ValueChanged;
            // 
            // divisionsLabel
            // 
            divisionsLabel.AutoSize = true;
            divisionsLabel.Location = new Point(11, 6);
            divisionsLabel.Name = "divisionsLabel";
            divisionsLabel.Size = new Size(140, 15);
            divisionsLabel.TabIndex = 0;
            divisionsLabel.Text = "Dokładność triangulacji: 16";
            // 
            // kdLabel
            // 
            kdLabel.AutoSize = true;
            kdLabel.Location = new Point(11, 119);
            kdLabel.Name = "kdLabel";
            kdLabel.Size = new Size(50, 15);
            kdLabel.TabIndex = 5;
            kdLabel.Text = "kd: 0.70";
            // 
            // kdTrackBar
            // 
            kdTrackBar.AutoSize = false;
            kdTrackBar.Location = new Point(11, 135);
            kdTrackBar.Maximum = 100;
            kdTrackBar.Name = "kdTrackBar";
            kdTrackBar.Size = new Size(178, 28);
            kdTrackBar.TabIndex = 6;
            kdTrackBar.TickFrequency = 10;
            kdTrackBar.Value = 70;
            kdTrackBar.ValueChanged += lightingTrackBar_ValueChanged;
            // 
            // ksLabel
            // 
            ksLabel.AutoSize = true;
            ksLabel.Location = new Point(11, 167);
            ksLabel.Name = "ksLabel";
            ksLabel.Size = new Size(50, 15);
            ksLabel.TabIndex = 7;
            ksLabel.Text = "ks: 0.50";
            // 
            // ksTrackBar
            // 
            ksTrackBar.AutoSize = false;
            ksTrackBar.Location = new Point(11, 183);
            ksTrackBar.Maximum = 100;
            ksTrackBar.Name = "ksTrackBar";
            ksTrackBar.Size = new Size(178, 28);
            ksTrackBar.TabIndex = 8;
            ksTrackBar.TickFrequency = 10;
            ksTrackBar.Value = 50;
            ksTrackBar.ValueChanged += lightingTrackBar_ValueChanged;
            // 
            // mLabel
            // 
            mLabel.AutoSize = true;
            mLabel.Location = new Point(11, 215);
            mLabel.Name = "mLabel";
            mLabel.Size = new Size(40, 15);
            mLabel.TabIndex = 9;
            mLabel.Text = "m: 20";
            // 
            // mTrackBar
            // 
            mTrackBar.AutoSize = false;
            mTrackBar.Location = new Point(11, 231);
            mTrackBar.Maximum = 100;
            mTrackBar.Minimum = 1;
            mTrackBar.Name = "mTrackBar";
            mTrackBar.Size = new Size(178, 28);
            mTrackBar.TabIndex = 10;
            mTrackBar.TickFrequency = 10;
            mTrackBar.Value = 20;
            mTrackBar.ValueChanged += lightingTrackBar_ValueChanged;
            // 
            // lightZLabel
            // 
            lightZLabel.AutoSize = true;
            lightZLabel.Location = new Point(11, 263);
            lightZLabel.Name = "lightZLabel";
            lightZLabel.Size = new Size(80, 15);
            lightZLabel.TabIndex = 11;
            lightZLabel.Text = "z światła: 2.0";
            // 
            // lightZTrackBar
            // 
            lightZTrackBar.AutoSize = false;
            lightZTrackBar.Location = new Point(11, 279);
            lightZTrackBar.Maximum = 50;
            lightZTrackBar.Minimum = 5;
            lightZTrackBar.Name = "lightZTrackBar";
            lightZTrackBar.Size = new Size(178, 28);
            lightZTrackBar.TabIndex = 12;
            lightZTrackBar.TickFrequency = 5;
            lightZTrackBar.Value = 20;
            lightZTrackBar.ValueChanged += lightingTrackBar_ValueChanged;
            // 
            // lightColorButton
            // 
            lightColorButton.BackColor = Color.White;
            lightColorButton.Location = new Point(11, 313);
            lightColorButton.Name = "lightColorButton";
            lightColorButton.Size = new Size(178, 25);
            lightColorButton.TabIndex = 13;
            lightColorButton.Text = "Kolor światła";
            lightColorButton.UseVisualStyleBackColor = false;
            lightColorButton.Click += lightColorButton_Click;
            // 
            // animateLightCheckBox
            // 
            animateLightCheckBox.AutoSize = true;
            animateLightCheckBox.Checked = true;
            animateLightCheckBox.CheckState = CheckState.Checked;
            animateLightCheckBox.Location = new Point(11, 343);
            animateLightCheckBox.Name = "animateLightCheckBox";
            animateLightCheckBox.Size = new Size(115, 19);
            animateLightCheckBox.TabIndex = 14;
            animateLightCheckBox.Text = "Animacja światła";
            animateLightCheckBox.UseVisualStyleBackColor = true;
            animateLightCheckBox.CheckedChanged += animateLightCheckBox_CheckedChanged;
            // 
            // showLightCurveCheckBox
            // 
            showLightCurveCheckBox.AutoSize = true;
            showLightCurveCheckBox.Location = new Point(11, 363);
            showLightCurveCheckBox.Name = "showLightCurveCheckBox";
            showLightCurveCheckBox.Size = new Size(138, 19);
            showLightCurveCheckBox.TabIndex = 15;
            showLightCurveCheckBox.Text = "Pokaż krzywą światła";
            showLightCurveCheckBox.UseVisualStyleBackColor = true;
            showLightCurveCheckBox.CheckedChanged += showLightCurveCheckBox_CheckedChanged;
            // 
            // lightColorDialog
            // 
            lightColorDialog.Color = Color.White;
            lightColorDialog.FullOpen = true;
            // 
            // surfaceComboBox
            // 
            surfaceComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            surfaceComboBox.Location = new Point(11, 388);
            surfaceComboBox.Name = "surfaceComboBox";
            surfaceComboBox.Size = new Size(178, 23);
            surfaceComboBox.TabIndex = 16;
            surfaceComboBox.SelectedIndexChanged += surfaceComboBox_SelectedIndexChanged;
            // 
            // objectColorGroupBox
            // 
            objectColorGroupBox.Controls.Add(loadTextureButton);
            objectColorGroupBox.Controls.Add(objectColorButton);
            objectColorGroupBox.Controls.Add(textureRadioButton);
            objectColorGroupBox.Controls.Add(solidColorRadioButton);
            objectColorGroupBox.Location = new Point(11, 416);
            objectColorGroupBox.Name = "objectColorGroupBox";
            objectColorGroupBox.Size = new Size(178, 120);
            objectColorGroupBox.TabIndex = 17;
            objectColorGroupBox.TabStop = false;
            objectColorGroupBox.Text = "Kolor obiektu";
            // 
            // solidColorRadioButton
            // 
            solidColorRadioButton.AutoSize = true;
            solidColorRadioButton.Checked = true;
            solidColorRadioButton.Location = new Point(8, 18);
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
            textureRadioButton.Location = new Point(8, 38);
            textureRadioButton.Name = "textureRadioButton";
            textureRadioButton.Size = new Size(71, 19);
            textureRadioButton.TabIndex = 1;
            textureRadioButton.Text = "Tekstura";
            textureRadioButton.UseVisualStyleBackColor = true;
            textureRadioButton.CheckedChanged += textureRadioButton_CheckedChanged;
            // 
            // objectColorButton
            // 
            objectColorButton.Location = new Point(8, 60);
            objectColorButton.Name = "objectColorButton";
            objectColorButton.Size = new Size(162, 25);
            objectColorButton.TabIndex = 2;
            objectColorButton.Text = "Kolor obiektu";
            objectColorButton.UseVisualStyleBackColor = false;
            objectColorButton.Click += objectColorButton_Click;
            // 
            // loadTextureButton
            // 
            loadTextureButton.Location = new Point(8, 89);
            loadTextureButton.Name = "loadTextureButton";
            loadTextureButton.Size = new Size(162, 25);
            loadTextureButton.TabIndex = 3;
            loadTextureButton.Text = "Wczytaj teksturę...";
            loadTextureButton.UseVisualStyleBackColor = true;
            loadTextureButton.Click += loadTextureButton_Click;
            // 
            // useNormalMapCheckBox
            // 
            useNormalMapCheckBox.AutoSize = true;
            useNormalMapCheckBox.Location = new Point(11, 542);
            useNormalMapCheckBox.Name = "useNormalMapCheckBox";
            useNormalMapCheckBox.Size = new Size(118, 19);
            useNormalMapCheckBox.TabIndex = 18;
            useNormalMapCheckBox.Text = "Mapa normalnych";
            useNormalMapCheckBox.UseVisualStyleBackColor = true;
            useNormalMapCheckBox.CheckedChanged += useNormalMapCheckBox_CheckedChanged;
            // 
            // loadNormalMapButton
            // 
            loadNormalMapButton.Location = new Point(11, 564);
            loadNormalMapButton.Name = "loadNormalMapButton";
            loadNormalMapButton.Size = new Size(178, 25);
            loadNormalMapButton.TabIndex = 19;
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
            // lightAnimationTimer
            // 
            lightAnimationTimer.Interval = 33;
            lightAnimationTimer.Tick += lightAnimationTimer_Tick;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 650);
            Controls.Add(drawingPanel);
            Controls.Add(controlPanel);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
            Name = "MainForm";
            Text = "Bezier Scene";
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
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
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip;
        private ToolStripMenuItem helpMenuItem;
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
        private CheckBox animateLightCheckBox;
        private CheckBox showLightCurveCheckBox;
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
        private System.Windows.Forms.Timer lightAnimationTimer;
    }
}
