﻿namespace Trik.Sensors
open System
open System.IO
open System.Threading
open Trik
open Trik.Internals

[<Sealed>]
type MXNSensor(scriptPath, commandPath: string, sensorPath) = 
    inherit StringFifoSensor<int[]>(scriptPath)

    let mutable stream = null
    let mutable commandFifo: StreamWriter = null
    let mutable isDisposed = false
    let mutable sizeX = 3
    let mutable sizeY = 3

    let script cmd = Helpers.SendToShell <| scriptPath + " " + cmd

    member self.Size 
        with get () = (sizeX, sizeY)
        and set (v1, v2) = 
            sizeX <- v1
            sizeY <- v2
            if commandFifo = null then invalidOp "missing Start() before call"
            commandFifo.WriteLine("mxn " + (string v1) + " " + (string v2))

    member self.Start() = 
        script "start"; base.Start()

        stream <- new FileStream(commandPath, FileMode.Open, FileAccess.Write)
        commandFifo <- new StreamWriter(stream, Text.Encoding.UTF8, AutoFlush = true)

    member self.Stop() = base.Stop(); commandFifo.Close(); script "stop"

    new (videoSource) = 
        let script = 
            match videoSource with
            | Ports.VideoSource.USB -> "/etc/init.d/mxn-sensor-webcam.sh"
            | _                     -> "/etc/init.d/mxn-sensor-ov7670.sh"

        new MXNSensor(script, "/run/mxn-sensor.in.fifo", "/run/mxn-sensor.out.fifo")

    override self.Parse text =
        let parse x = Trik.Helpers.fastInt32Parse x
        let parsedLines = text.Split([|' '|], StringSplitOptions.RemoveEmptyEntries)

        parsedLines.[0].Remove |> ignore
        Array.map parse parsedLines |> Some

    override self.Dispose() = 
        if not isDisposed then
            commandFifo.Dispose()
            stream.Dispose()
            base.Dispose()
            self.Stop()
            isDisposed <- true
