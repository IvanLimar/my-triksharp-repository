namespace Trik.Sensors
open System
open System.IO
open Trik
open Trik.Internals
open Trik.Helpers

type Sonar (min, max, devicePath) =
    inherit BinaryFifoSensor<int>(devicePath, 16, 1024)
    [<Literal>]
    let evAbs = 3us
    let mutable distance = 0
    override self.Parse (bytes, offset) =
        if bytes.Length < 16 then None
        else
            let evType = BitConverter.ToUInt16(bytes, offset + 8)
            let evCode = BitConverter.ToUInt16(bytes, offset + 10)
            let evValue = BitConverter.ToInt32(bytes, offset + 12)
            if evType = evAbs
            then match evCode with
                 | 25us -> distance <- Calculations.limit min max evValue
                           None
                 | _ -> Some(distance)
            else None
    new (min : int, max : int, port : ISonarPort) = new Sonar(min, max, port.EventFile)