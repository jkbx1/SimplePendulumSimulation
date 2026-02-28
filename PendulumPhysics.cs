namespace SimplePendulum;

public class PendulumPhysics
{
        // PHYSICAL PARAMETERS
    public double Length { get; set;} = 1.0;
    public double Gravity { get; set;} = 9.81;
    public double Mass { get; set;} = 1.0;
    public double Damping { get; set;} = 0.0;

        // CURRENT STATE
    public double InitialAngle {get; set;} = Math.PI / 4;
    public double Angle {get; set;} = Math.PI / 4;
    public double AngularVelocity {get; set;} = 0.0;

        // STATISTICS
    public double Time {get; private set;} = 0.0;
    public double Period {get; private set;} = 0.0;
    public int SwingCount {get; private set;} = 0;

    private int halfSwingCount = 0;
    private double previousTime = 0.0;

    public void Reset()
    {
        Angle = InitialAngle;
        AngularVelocity = 0;
        Time = 0;
        Period = 0;
        SwingCount = 0;
        halfSwingCount = 0;
        previousTime = 0;
    }

    public void Update(double deltaTime, int iterations = 20)
    {
        double dt = deltaTime / iterations;

        for (int i = 0; i < iterations; i++)
        {
            Time += dt;
            Angle += AngularVelocity * dt;

            double angularAcceleration = -(Gravity / Length * Math.Sin(Angle) + Damping * AngularVelocity);

            // REBOUND LIMIT LOGIC
            if (Math.Abs(Angle) > Math.Abs(InitialAngle))
            {
                Angle = (InitialAngle > 0)
                    ?
                    (Angle > 0 ? InitialAngle : -InitialAngle)
                    :
                    (Angle > 0 ? -InitialAngle : InitialAngle);
                AngularVelocity = 0;
                halfSwingCount++;
            }

            //  PERIOD AND SWING COUNTING LOGIC
            if ((AngularVelocity > 0 && AngularVelocity + angularAcceleration * dt < 0) ||
                (AngularVelocity < 0 && AngularVelocity + angularAcceleration * dt > 0))
            {
                halfSwingCount++;
            }

            if (halfSwingCount % 2 == 0 && halfSwingCount / 2 > SwingCount)
            {
                SwingCount++;
                Period = Time - previousTime;
                previousTime = Time;
            }

            AngularVelocity += angularAcceleration * dt;
        }
    }
}



    