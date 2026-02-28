using System.Drawing.Drawing2D;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.ApplicationServices;

namespace SimplePendulum;

public class MainForm : Form
{
    private PendulumPhysics physics;

    //  RENDERING VARIABLES
    private int anchorX, anchorY, bobX, bobY;
    private int bobRadius = 15;
    private bool isDragging = false;
    private DateTime lastFrametime;

    //  UI CONTROLS
    private System.Windows.Forms.Timer simulationTimer;
    private PictureBox canvas = null!;
    private Button btnStartStop = null!;
    private TrackBar tbDamping = null!;
    private NumericUpDown nudInitialAngle = null!;
    private NumericUpDown nudLength = null!;
    private CheckBox cbShowVectors = null!;
    private Label lblTime = null!, lblPeriod = null!, lblSwings = null!, lblDamping = null!, lblDragInfo = null!;

    public MainForm()
    {
        physics = new PendulumPhysics();

        Text = "Simple Pendulum Simulation";
        Size = new Size(1200, 900);
        MinimumSize = new Size(800, 600);
        DoubleBuffered = true;

        InitializeUI();

        simulationTimer = new System.Windows.Forms.Timer { Interval = 16 };
        simulationTimer.Tick += SimulationLoop;

        ApplyInitialConditions();
    }

    private void InitializeUI()
    {
        Panel sidePanel = new Panel { Dock = DockStyle.Right, Width = 220 };
        Controls.Add(sidePanel);

        btnStartStop = new Button { Text = "Start", Top = 20, Left = 20, Width = 180, Height = 40, Font = new Font("Arial", 12, FontStyle.Bold) };
        btnStartStop.Click += BtnStartStop_Click;
        sidePanel.Controls.Add(btnStartStop);

        sidePanel.Controls.Add(new Label { Text = "Damping (Beta):", Top = 80, Left = 20, Width = 180 });
        tbDamping = new TrackBar { Minimum = 0, Maximum = 20, Value = 0, Top = 100, Left = 15, Width = 180 };
        tbDamping.ValueChanged += (s, e) => { ApplyInitialConditions(); };
        sidePanel.Controls.Add(tbDamping);

        lblDamping = new Label { Text = "0.00", Top = 150, Left = 20, Width = 180 };
        sidePanel.Controls.Add(lblDamping);

        sidePanel.Controls.Add(new Label { Text = "Initial Angle (Degrees):", Top = 180, Left = 20, Width = 180 });
        nudInitialAngle = new NumericUpDown { Minimum = -179, Maximum = 179, Value = 45, Top = 200, Left = 20, Width = 180 };
        nudInitialAngle.ValueChanged += (s, e) => { ApplyInitialConditions(); };
        sidePanel.Controls.Add(nudInitialAngle);

        cbShowVectors = new CheckBox { Text = "Show Force Vectors", Top = 240, Left = 20, Width = 180, Checked = true };
        cbShowVectors.CheckedChanged += (s, e) => { canvas.Invalidate(); };
        sidePanel.Controls.Add(cbShowVectors);

        sidePanel.Controls.Add(new Label { Text = "Length (Meters):", Top = 270, Left = 20, Width = 180 });
        nudLength = new NumericUpDown { Minimum = 0.1m, Maximum = 100m, Value = 1.0m, DecimalPlaces = 2, Increment = 0.1m, Top = 290, Left = 20, Width = 180 };
        nudLength.ValueChanged += (s, e) => { ApplyInitialConditions(); };
        sidePanel.Controls.Add(nudLength);

        lblTime = new Label { Text = "Time: 0.00 s", Top = 340, Left = 20, Width = 180, Font = new Font("Arial", 10) };
        lblPeriod = new Label { Text = "Period T: 0.00 s", Top = 370, Left = 20, Width = 180, Font = new Font("Arial", 10) };
        lblSwings = new Label { Text = "Swings: 0", Top = 400, Left = 20, Width = 180, Font = new Font("Arial", 10) };
        sidePanel.Controls.Add(lblTime);
        sidePanel.Controls.Add(lblPeriod);
        sidePanel.Controls.Add(lblSwings);

        lblDragInfo = new Label { 
            Text = "Tip: You can click and drag the pendulum bob to set its angle manually.", 
            Top = this.ClientSize.Height - 80, 
            Left = 20, 
            Width = 180, 
            Height = 60, 
            Font = new Font("Arial", 9, FontStyle.Italic), 
            ForeColor = Color.Gray,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left
        };
        sidePanel.Controls.Add(lblDragInfo);

        canvas = new PictureBox { Dock = DockStyle.Fill };
        canvas.Paint += RenderPendulum;
        canvas.MouseDown += Canvas_MouseDown;
        canvas.MouseMove += Canvas_MouseMove;
        canvas.MouseUp += Canvas_MouseUp;
        Controls.Add(canvas);
        canvas.BringToFront();
    }
    private void ApplyInitialConditions()
    {
        physics.InitialAngle = (double)nudInitialAngle.Value * Math.PI / 180.0;
        physics.Damping = tbDamping.Value / 10.0;
        physics.Length = (double)nudLength.Value;
        lblDamping.Text = physics.Damping.ToString("0.00");

        physics.Reset();
        UpdateStatsUi();
        canvas.Invalidate();
    }

    private void BtnStartStop_Click(object? sender, EventArgs e)
    {
        if (btnStartStop.Text == "Start")
        {
            btnStartStop.Text = "Stop";
            if (physics.Time == 0) ApplyInitialConditions();
            lastFrametime = DateTime.Now;
            simulationTimer.Start();
        }
        else
        {
            btnStartStop.Text = "Start";
            simulationTimer.Stop();
        }
    }
    private void SimulationLoop(object? sender, EventArgs e)
    {
        DateTime currentTime = DateTime.Now;
        double deltaTime = (currentTime - lastFrametime).TotalSeconds;
        lastFrametime = currentTime;

        physics.Update(deltaTime);
        UpdateStatsUi();
        canvas.Invalidate();
    }

    private void UpdateStatsUi()
    {
        lblTime.Text = $"Time: {physics.Time:0.00} s";
        lblPeriod.Text = $"Period T: {physics.Period:0.00} s";
        lblSwings.Text = $"Swings: {physics.SwingCount}";
    }

    private void RenderPendulum(object? sender, PaintEventArgs e)
    {
        Graphics gFx = e.Graphics;
        gFx.SmoothingMode = SmoothingMode.AntiAlias;

        double viewSize = 2.8 * physics.Length;
        double pixelsPerMeter = Math.Min(canvas.Width, canvas.Height) / viewSize;

        anchorX = canvas.Width / 2;
        anchorY = canvas.Height / 2;

        double xPos = physics.Length * Math.Sin(physics.Angle);
        double yPos = -physics.Length * Math.Cos(physics.Angle);

        bobX = anchorX + (int)(xPos * pixelsPerMeter);
        bobY = anchorY - (int)(yPos * pixelsPerMeter);

        // DRAW STRING AND BOB
        gFx.DrawLine(new Pen(Color.DarkGreen, 3), anchorX, anchorY, bobX, bobY);
        gFx.FillEllipse(Brushes.Green, bobX - bobRadius, bobY - bobRadius, bobRadius * 2, bobRadius * 2);

        // DRAW FORCES
        if (cbShowVectors.Checked)
        {
            double v = physics.AngularVelocity * physics.Length;
            double normalForce = physics.Mass * physics.Gravity * Math.Cos(physics.Angle) + physics.Mass * v * v / physics.Length;
            
            // Visually scale drawn vectors so the max theoretical force (5*m*g) takes up ~1.5x the pendulum length,
            // scaling proportional to pixelsPerMeter so they never go out of the window.
            double vectorScale = (pixelsPerMeter * physics.Length * 1.5) / (5.0 * physics.Mass * physics.Gravity);

            // GRAVITY (GREEN)
            VectorRenderer.DrawForceVector(gFx, Color.Green, bobX, bobY, 0, -physics.Mass * physics.Gravity * vectorScale);

            // TANGENTIAL FORCE (BLUE)
            VectorRenderer.DrawForceVector(gFx, Color.Blue, bobX, bobY,
                    -Math.Cos(physics.Angle) * physics.Mass * physics.Gravity * Math.Sin(physics.Angle) * vectorScale,
                    -Math.Sin(physics.Angle) * physics.Mass * physics.Gravity * Math.Sin(physics.Angle) * vectorScale);

            // NORMAL COMPONENT OF GRAVITY (PURPLE)
            VectorRenderer.DrawForceVector(gFx, Color.Purple, bobX, bobY,
                Math.Sin(physics.Angle) * physics.Mass * physics.Gravity * Math.Cos(physics.Angle) * vectorScale,
                -Math.Cos(physics.Angle) * physics.Mass * physics.Gravity * Math.Cos(physics.Angle) * vectorScale);

            // TENSION (RED)
            VectorRenderer.DrawForceVector(gFx, Color.Red, bobX, bobY,
                -normalForce * Math.Sin(physics.Angle) * vectorScale,
                normalForce * Math.Cos(physics.Angle) * vectorScale);
        }

    }

    private void Canvas_MouseDown(object? sender, MouseEventArgs e)
    {
        if (Math.Sqrt(Math.Pow(e.X - bobX, 2) + Math.Pow(e.Y - bobY, 2)) <= bobRadius * 2)
        {
            isDragging = true;
            simulationTimer.Stop();
            btnStartStop.Text = "Start";
        }
    }
    private void Canvas_MouseMove(object? sender, MouseEventArgs e)
    {
        if (isDragging)
        { 
            double dx = e.X - anchorX;
            double dy = e.Y - anchorY;

            double newAngle = Math.Atan2(dx, dy);

            if (newAngle > Math.PI * 179 / 180) newAngle = Math.PI * 179 / 180;
            if (newAngle < -Math.PI * 179 / 180) newAngle = -Math.PI * 179 / 180;

                physics.InitialAngle = physics.Angle = newAngle;
                physics.AngularVelocity = 0;
                nudInitialAngle.Value = (decimal)(physics.Angle * 180 / Math.PI);
                
                canvas.Invalidate();
        }
        else
        {
            canvas.Cursor = (Math.Sqrt(Math.Pow(e.X - bobX, 2) + Math.Pow(e.Y - bobY, 2)) <= bobRadius * 2) 
                            ? Cursors.Hand : Cursors.Default;
        }
    }
    private void Canvas_MouseUp(object? sender, MouseEventArgs e)
    {
        if (isDragging)
        {
            isDragging = false;
            ApplyInitialConditions();
        }
    }
}
