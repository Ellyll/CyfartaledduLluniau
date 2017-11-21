open System.Drawing
open System.Collections.Generic
open System.IO

let bitmapIArray2D (bitmap : Bitmap) : (int*int*int)[,] =
    Array2D.init bitmap.Size.Height bitmap.Size.Width (fun y x ->
        let lliw = bitmap.GetPixel(x, y)
        (int lliw.R, int lliw.G, int lliw.B))

let array2DIBitmap (arr : (int*int*int)[,]) : Bitmap =
    let uchder = Array2D.length1 arr
    let lled = Array2D.length2 arr
    let bitmap = new Bitmap(lled, uchder)
    arr
    |> Array2D.iteri (fun y x (r,g,b) -> bitmap.SetPixel(x, y, Color.FromArgb(r, g, b)))
    bitmap

let cyfartaledduBitmaps enwauFfeiliau =
        enwauFfeiliau
        |> Seq.mapi (fun i enw ->
            use image = Image.FromFile(enw)
            use bm = new Bitmap(image)
            (i, bitmapIArray2D bm)
            )
        |> Seq.reduce (fun (_, arr1) (i, arr2) -> (i, Array2D.mapi (fun y x (r1,g1,b1) ->
                // cyfrifo cyfanswm i bob lliw i bob picsel
                let (r2,g2,b2) = arr2.[y,x]
                (r1+r2, g1+g2, b1+b2)
            ) arr1))
        |> (fun (n, arr) ->
            // rhannu gyda'r nifer y cyfrifo'r cyfartaledd
            arr |> Array2D.map (fun (r,g,b) -> (r/(n+1), g/(n+1), b/(n+1))))
        |> array2DIBitmap // troi Array2D yn ôl i Bitmap


[<EntryPoint>]
let main argv =
    if (Array.length argv) <> 3 then
        let enwRhaglen = "CyfartaledduLluniau"
        eprintfn "Defnydd: %s llwybr patrwm llwybrCadw" enwRhaglen
        eprintfn "e.e. %s /data/Lluniau 'IMG_2017*.jpg' /tmp/" enwRhaglen
        exit 1

    let llwybr = argv.[0]
    let patrwm = argv.[1]
    let llwybrCadw = argv.[2]

    printfn "Nôl enwau ffeiliau..."
    let enwauFfeiliau = Directory.EnumerateFileSystemEntries(llwybr, patrwm, SearchOption.AllDirectories) |> List.ofSeq    
    printfn "Wedi darganfod %d ffeil delwedd" (List.length enwauFfeiliau)

    printfn "Nôl meintiau delweddau..."
    let meintiau =
        enwauFfeiliau
        |> Seq.fold (fun (m : Dictionary<int*int, string list>) enw ->
                        use image = Image.FromFile(enw)
                        let key : int*int = (image.Size.Width,image.Size.Height)                        
                        if (m.ContainsKey(key)) then
                            let value : string list = m.GetValueOrDefault(key, [])
                            m.[key] <- enw::value
                        else
                            m.Add(key, [ enw ])
                        m) (new Dictionary<int*int, string list>())

    meintiau.Keys
    |> Seq.iter (fun key ->
        let nifer = meintiau.[key] |> List.length
        printfn "Maint: %A Nifer: %d" key nifer
        let enwFfeil = sprintf "%scyfartaledd_%dx%d_%d.bmp" llwybrCadw (fst key) (snd key) nifer
        if nifer < 2 then
            printfn "Sgipio, nifer rhy isel"
        elif File.Exists(enwFfeil) then
            printfn "Sgipio, ffeil %s yn bodoli yn barod" enwFfeil
        else
            printfn "Cyfartaleddu..."
            let bitmap = cyfartaledduBitmaps meintiau.[key]        
            bitmap.Save(enwFfeil)
            printfn "Cwblhawyd, arbedwyd y llun i %s" enwFfeil
        )

    0
