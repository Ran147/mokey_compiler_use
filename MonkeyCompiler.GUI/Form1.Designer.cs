namespace MonkeyCompiler.GUI;

partial class Form1
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
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        txtCode = new System.Windows.Forms.TextBox();
        btnRun = new System.Windows.Forms.Button();
        txtOutput = new System.Windows.Forms.TextBox();
        SuspendLayout();
        // 
        // txtCode
        // 
        txtCode.Location = new System.Drawing.Point(34, 29);
        txtCode.Multiline = true;
        txtCode.Name = "txtCode";
        txtCode.ScrollBars = System.Windows.Forms.ScrollBars.Both;
        txtCode.Size = new System.Drawing.Size(728, 187);
        txtCode.TabIndex = 0;
        // 
        // btnRun
        // 
        btnRun.Location = new System.Drawing.Point(642, 236);
        btnRun.Name = "btnRun";
        btnRun.Size = new System.Drawing.Size(119, 30);
        btnRun.TabIndex = 1;
        btnRun.Text = "Compilar y Correr";
        btnRun.UseVisualStyleBackColor = true;
        btnRun.Click += btnRun_Click;
        // 
        // txtOutput
        // 
        txtOutput.Location = new System.Drawing.Point(37, 275);
        txtOutput.Multiline = true;
        txtOutput.Name = "txtOutput";
        txtOutput.Size = new System.Drawing.Size(724, 153);
        txtOutput.TabIndex = 2;
        // 
        // Form1
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(800, 450);
        Controls.Add(txtOutput);
        Controls.Add(btnRun);
        Controls.Add(txtCode);
        Text = "Form1";
        Load += Form1_Load;
        ResumeLayout(false);
        PerformLayout();
    }

    private System.Windows.Forms.TextBox txtOutput;

    private System.Windows.Forms.Button btnRun;

    private System.Windows.Forms.TextBox txtCode;

    #endregion
}