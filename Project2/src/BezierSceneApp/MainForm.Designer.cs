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
            showMeshCheckBox = new CheckBox();
            showPolygonsCheckBox = new CheckBox();
            divisionsTrackBar = new TrackBar();
            divisionsLabel = new Label();
            controlPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)divisionsTrackBar).BeginInit();
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
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 750);
            Controls.Add(drawingPanel);
            Controls.Add(controlPanel);
            Name = "MainForm";
            Text = "Bezier Scene";
            controlPanel.ResumeLayout(false);
            controlPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)divisionsTrackBar).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DrawingPanel drawingPanel;
        private Panel controlPanel;
        private Label divisionsLabel;
        private TrackBar divisionsTrackBar;
        private CheckBox showPolygonsCheckBox;
        private CheckBox showMeshCheckBox;
    }
}
