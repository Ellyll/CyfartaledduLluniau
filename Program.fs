open System.Drawing
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


[<EntryPoint>]
let main _ =
    let llwybr = "/tmp/"
    let enwauFfeiliau = Directory.EnumerateFileSystemEntries(llwybr, "*.jpg", SearchOption.AllDirectories)    
    let lled = 4272
    let uchder = 2848
    let llwybrBitmapNewydd = "/tmp/cyfartaledd.bmp"

    printfn "Cyfartaleddu bitmaps..."

    let bitmap : Bitmap =
        enwauFfeiliau
        |> Seq.map (fun enw -> Image.FromFile(enw)) // Darllen y delwedd
        |> Seq.filter (fun image -> image.Size.Width = lled && image.Size.Height = uchder) // anwybyddu y rhai sydd ddim y maint cywir
        |> Seq.mapi (fun i image -> (i, bitmapIArray2D(new Bitmap(image)))) // troi i mewn i Bitmap a wedyn Array2D
        |> Seq.take 10
        |> Seq.reduce (fun (_, arr1) (i, arr2) -> (i, Array2D.mapi (fun y x (r1,g1,b1) -> // cyfrifo cyfanswm i bob lliw i bob picsel
                let (r2,g2,b2) = arr2.[y,x]
                (r1+r2, b1+b2, g1+g2)
            ) arr1))
        |> (fun (n, arr) -> arr |> Array2D.map (fun (r,g,b) -> (r/n, b/n, g/n))) // rhannu gyda'r nifer y cyfrifo'r cyfartaledd
        |> array2DIBitmap // troi Array2D yn ôl i Bitmap
    
    bitmap.Save(llwybrBitmapNewydd)

    printfn "bitmap wedi ei arbed i %A" llwybrBitmapNewydd

    0
