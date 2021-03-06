namespace Trik.Internals
open System
open Trik
open Trik.Helpers


type Sensor3d (min, max, devicePath) = 
    inherit BinaryFifoSensor<Point>(devicePath, 16, 1024)
    [<Literal>]
    let evAbs = 3us
    let last = Array.zeroCreate 3
    override self.Parse(bytes, offset) = 
        if bytes.Length < 16 then None
        else 
            let evType = BitConverter.ToUInt16(bytes, offset + 8)
            let evCode = BitConverter.ToUInt16(bytes, offset + 10)
            let evValue = BitConverter.ToInt32(bytes, offset + 12)
            if evType = evAbs && evCode < 3us then 
                last.[int evCode] <- Calculations.limit min max evValue 
                None
            else
                Some <| new Point(last.[0], last.[1], last.[2])