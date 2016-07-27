﻿namespace Trik
open System
open System.Collections.Generic
open System.Threading
open Trik.Sensors
open Trik.Devices
open Trik.Network
open Trik.Helpers

type Model () as model = 

    static do I2C.init("/dev/i2c-2", 0x48, 1)
              IO.File.WriteAllText("/sys/class/gpio/gpio62/value", "1")
              Shell.send (String.Concat(List.map (sprintf "i2cset -y 2 0x48 %d 0x1000 w; ") [0x10 .. 0x13]))
                                                
    static let resources = new ResizeArray<_>()
    
    let mutable isDisposed = false
    do AppDomain.CurrentDomain.ProcessExit.Add(fun _ -> (model :> IDisposable).Dispose())
    let propertyInit config ctor = config |> Array.map (fun key -> (key, ctor key)) |> dict

    let buttons =      lazy new Buttons()
    let gyro =         lazy new Gyroscope(-32767, 32767, "/dev/input/by-path/platform-spi_davinci.1-event")
    let accel =        lazy new Accelerometer(-32767, 32767, "/dev/input/event1")
    let led =          lazy new Led()
    let pad =          lazy new PadServer(model.PadConfig)
    let ledStripe =    lazy new LedStripe(model.LedStripeConfig)
    let lineSensor =   lazy new LineSensor(model.LineSensorConfig)
    let objectSensor = lazy new ObjectSensor(model.ObjectSensorConfig)  
    let mxnSensor =    lazy new MXNSensor(model.MXNSensorConfig)
    let battery =      lazy new Battery()
    let servo =        lazy (model.ServosConfig |> Seq.map (fun x -> (x.Key, new ServoMotor(x.Key.Path, x.Value))) |> dict)
    let motor =        lazy propertyInit model.MotorsConfig        (fun cnum -> new PowerMotor(cnum))
    let encoder =      lazy propertyInit model.EncodersConfig      (fun cnum -> new Encoder(cnum))
    let analogSensor = lazy propertyInit model.AnalogSensorsConfig (fun cnum -> new AnalogSensor(cnum))
    let sonar =        lazy propertyInit model.SonarsConfig        (fun cnum -> new Sonar(0, 400, cnum))
    
    member val PadConfig =          4444 with get, set
    member val LineSensorConfig =   VideoSource.VP2 with get, set
    member val ObjectSensorConfig = VideoSource.VP2 with get, set
    member val MXNSensorConfig =    VideoSource.VP2 with get, set
    member val LedStripeConfig =    Defaults.LedSripe with get, set

    member val ServosConfig: IDictionary<IServoPort,ServoKind> = 
        [| (E1 :> IServoPort, Defaults.Servo3)
           (E2 :> IServoPort, Defaults.Servo3)
           (E3 :> IServoPort, Defaults.Servo3)
           (C1 :> IServoPort, Defaults.Servo4)
           (C2 :> IServoPort, Defaults.Servo4)
           (C3 :> IServoPort, Defaults.Servo4) |] |> Calculations.writableDict with get, set
    member val EncodersConfig: IEncoderPort [] =
        (Defaults.EncoderPorts |> Array.map (fun x -> upcast x)) with get, set
    member val MotorsConfig: IMotorPort [] = 
        (Defaults.MotorPorts |> Array.map (fun x -> upcast x)) with get, set

    member val AnalogSensorsConfig: ISensorPort [] = 
        (Defaults.SensorPorts |> Array.map (fun x -> upcast x)) with get, set
        
    member val SonarsConfig : ISonarPort [] = 
        (Defaults.SonarPorts |> Array.map (fun x -> upcast x)) with get, set


    member self.Accel         with get() = accel.Force()
    member self.AnalogSensors with get() = analogSensor.Force()
    member self.Battery       with get() = battery.Force()
    member self.Buttons       with get() = buttons.Force()
    member self.Encoders      with get() = encoder.Force()
    member self.Gyro          with get() = gyro.Force()
    member self.Led           with get() = led.Force()
    member self.LedStripe     with get() = ledStripe.Force()
    member self.LineSensor    with get() = lineSensor.Force()
    member self.Motors        with get() = motor.Force()
    member self.MXNSensor     with get() = mxnSensor.Force()
    member self.ObjectSensor  with get() = objectSensor.Force()
    member self.Pad           with get() = pad.Force()
    member self.Servos        with get() = servo.Force()
    member self.Sonars        with get() = sonar.Force()
    
    member self.Sleep (ms: int) = Thread.Sleep ms

    static member RegisterResource(resource: #IDisposable) = 
        lock resources <| 
        fun () -> resources.Add(resource :> IDisposable)

    interface IDisposable with
        member self.Dispose() = 
            lock self <| fun () -> 
            if not isDisposed then
                resources.ForEach(fun x -> x.Dispose()) 
                let inline dispose (device: Lazy<'T> when 'T :> IDisposable) = 
                    if device.IsValueCreated then device.Force().Dispose()
                let inline disposeMap (devices: Lazy<IDictionary<'TKey, 'T>> when 'T :> IDisposable) = 
                    if devices.IsValueCreated then 
                        devices.Force().Values |> (Seq.iter (fun x -> x.Dispose()))
                
                dispose lineSensor; dispose objectSensor; dispose mxnSensor; 
                dispose gyro; dispose accel; dispose led; dispose pad;
                dispose ledStripe; disposeMap motor; disposeMap servo; 
                disposeMap analogSensor; dispose buttons; disposeMap sonar
                isDisposed <- true