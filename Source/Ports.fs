namespace Trik
    type IEncoderPort =
        abstract member I2CNumber : int
    ///<summary>Type representing Encoders ports</summary>
    type EncoderPort  = B1 | B2 | B3 | B4 | E1 | E2 | E3 | E4 with 
        interface IEncoderPort with
            member self.I2CNumber = 
                match self with 
                    | B1 | E1 -> 0x30
                    | B2 | E2 -> 0x31 
                    | B4 | E3 -> 0x32 
                    | B3 | E4 -> 0x33
    
    type IMotorPort =
        abstract member I2CNumber : int
    ///<summary>Type representing PowerMotor ports</summary>
    type MotorPort = M1 | M2 | M3 | M4 with
        interface IMotorPort with
            member self.I2CNumber = 
                match self with 
                | M1 -> 0x14 
                | M2 -> 0x15
                | M3 -> 0x17
                | M4 -> 0x16
    
    type IServoPort =
        abstract member Path : string
    type ServoPort = E1 | E2 | E3 | C1 | C2 | C3 | S1 | S2 | S3 | S4 | S5 | S6 with
        interface IServoPort with
            member self.Path = 
                match self with 
                | E1 | S6 -> "/sys/class/pwm/ehrpwm.1:1" 
                | E2 | S5 -> "/sys/class/pwm/ehrpwm.1:0" 
                | E3 | S4 -> "/sys/class/pwm/ehrpwm.0:1"             
                | C1 | S3 -> "/sys/class/pwm/ecap.0" 
                | C2 | S2 -> "/sys/class/pwm/ecap.1" 
                | C3 | S1 -> "/sys/class/pwm/ecap.2" 
    
    type ISensorPort =
        abstract member I2CNumber : int
    ///<summary>Type representing Analog and Digital Sensors ports</summary>
    type SensorPort = A1 | A2 | A3 | A4 | A5 | A6 with
        interface ISensorPort with
            member self.I2CNumber = 
                match self with 
                | A1 -> 0x25
                | A2 -> 0x24
                | A3 -> 0x23
                | A4 -> 0x22
                | A5 -> 0x21
                | A6 -> 0x20
                
    type ISonarPort =
        abstract member EventFile : string
        
    type SonarPort = D1 | D2 with
        interface ISonarPort with
            member self.EventFile =
                match self with
                | D1 -> "/dev/input/by-path/platform-trik_jd1-event"
                | D2 -> "/dev/input/by-path/platform-trik_jd2-event"
       

    type VideoSource = USB = 0 | VP1 = 1 | VP2 = 2
