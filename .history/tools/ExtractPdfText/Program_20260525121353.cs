using Docnet.Core;
using Docnet.Core.Models;
using System.Text;

if (args.Length < 2)
{
    Console.WriteLine("Usage: ExtractPdfText <input.pdf> <output.txt>");
    Console.WriteLine("Example: ExtractPdfText \"Manhood Device Development.pdf\" \"output.txt\"");
    return 1;
}

string inputPdf = args[0];
string outputTxt = args[1];

if (!File.Exists(inputPdf))
{
    Console.WriteLine($"ERROR: PDF file not found: {inputPdf}");
    return 1;
}

try
{
    Console.WriteLine($"=== PDF Text Extraction ===\n");
    Console.WriteLine($"Input:  {inputPdf}");
    Console.WriteLine($"Output: {outputTxt}");
    Console.WriteLine($"\n[Step 1] Opening PDF...");
    
    using var library = DocLib.Instance;
    using var docReader = library.GetDocReader(inputPdf, new PageDimensions(1080, 1920));
    var pageCount = docReader.GetPageCount();
    
    Console.WriteLine($"[OK] PDF opened: {pageCount} pages");
    Console.WriteLine($"\n[Step 2] Extracting text from {pageCount} pages...");
    
    var extractedText = new StringBuilder();
    var progress = 0;
    
    for (int i = 0; i < pageCount; i++)
    {
        using var pageReader = docReader.GetPageReader(i);
        var pageText = pageReader.GetText();
        
        extractedText.AppendLine($"=== Page {i + 1} ===");
        extractedText.AppendLine(pageText);
        extractedText.AppendLine();
        
        // Show progress every 10 pages
        var pageNum = i + 1;
        if (pageNum % 10 == 0 || pageNum == pageCount)
        {
            var newProgress = (int)((pageNum / (double)pageCount) * 100);
            if (newProgress != progress)
            {
                progress = newProgress;
                Console.WriteLine($"  [{progress}%] Processed {pageNum}/{pageCount} pages...");
            }
        }
    }
    
    Console.WriteLine($"\n[Step 3] Writing extracted text...");
    File.WriteAllText(outputTxt, extractedText.ToString());
    
    var fileInfo = new FileInfo(outputTxt);
    Console.WriteLine($"[OK] Text file created: {fileInfo.Length:N0} bytes");
    
    Console.WriteLine($"\n[SUCCESS] Extraction complete!");
    Console.WriteLine($"\nNext steps:");
    Console.WriteLine($"1. Review extracted text in: {outputTxt}");
    Console.WriteLine($"2. Process into chunks and summaries");
    Console.WriteLine($"3. Store in database (Sources → Chunks → Summaries)");
    
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"\n[ERROR] Failed to extract PDF text:");
    Console.WriteLine($"  {ex.Message}");
    Console.WriteLine($"\nStack trace:");
    Console.WriteLine(ex.StackTrace);
    return 1;
}
