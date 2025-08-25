using Api.Models;
using System.IO.Compression;
using System.Text;

namespace Api.Services.Export;

public class PowerPointExporter
{
    private readonly ILogger<PowerPointExporter> _logger;

    public PowerPointExporter(ILogger<PowerPointExporter> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> GeneratePowerPointAsync(PaperResponse paper)
    {
        try
        {
            using var memoryStream = new MemoryStream();
            
            // Create a ZIP file (PPTX is essentially a ZIP with XML files)
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                // Add [Content_Types].xml
                var contentTypesEntry = archive.CreateEntry("[Content_Types].xml");
                using (var writer = new StreamWriter(contentTypesEntry.Open()))
                {
                    writer.Write(CreateContentTypesXml());
                }
                
                // Add _rels/.rels
                var relsEntry = archive.CreateEntry("_rels/.rels");
                using (var writer = new StreamWriter(relsEntry.Open()))
                {
                    writer.Write(CreateRelsXml());
                }
                
                // Add ppt/_rels/presentation.xml.rels
                var presentationRelsEntry = archive.CreateEntry("ppt/_rels/presentation.xml.rels");
                using (var writer = new StreamWriter(presentationRelsEntry.Open()))
                {
                    writer.Write(CreatePresentationRelsXml());
                }
                
                // Add ppt/presentation.xml
                var presentationEntry = archive.CreateEntry("ppt/presentation.xml");
                using (var writer = new StreamWriter(presentationEntry.Open()))
                {
                    writer.Write(CreatePresentationXml(paper));
                }
                
                // Add ppt/slideMasters/slideMaster1.xml
                var slideMasterEntry = archive.CreateEntry("ppt/slideMasters/slideMaster1.xml");
                using (var writer = new StreamWriter(slideMasterEntry.Open()))
                {
                    writer.Write(CreateSlideMasterXml());
                }
                
                // Add ppt/slideLayouts/slideLayout1.xml
                var slideLayoutEntry = archive.CreateEntry("ppt/slideLayouts/slideLayout1.xml");
                using (var writer = new StreamWriter(slideLayoutEntry.Open()))
                {
                    writer.Write(CreateSlideLayoutXml());
                }
                
                // Create all slides
                var slides = CreateAllSlides(paper);
                int slideNumber = 1;
                
                foreach (var slide in slides)
                {
                    var slideEntry = archive.CreateEntry($"ppt/slides/slide{slideNumber}.xml");
                    using (var writer = new StreamWriter(slideEntry.Open()))
                    {
                        writer.Write(slide);
                    }
                    slideNumber++;
                }
            }
            
            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PowerPoint for paper {PaperId}", paper.Id);
            throw;
        }
    }
    
    private List<string> CreateAllSlides(PaperResponse paper)
    {
        var slides = new List<string>();
        
        // Title Slide
        slides.Add(CreateTitleSlideXml(paper));
        
        // Executive Summary Slide (only if has content)
        if (!string.IsNullOrEmpty(paper.Summary?.ExecutiveSummary))
        {
            slides.Add(CreateContentSlideXml("Executive Summary", TruncateContent(paper.Summary.ExecutiveSummary, 300)));
        }
        
        // Abstract Slide (only if has content)
        if (!string.IsNullOrEmpty(paper.Summary?.Abstract))
        {
            slides.Add(CreateContentSlideXml("Abstract", TruncateContent(paper.Summary.Abstract, 400)));
        }
        
        // Introduction Slide (only if has content)
        if (!string.IsNullOrEmpty(paper.Summary?.Introduction))
        {
            slides.Add(CreateContentSlideXml("Introduction", TruncateContent(paper.Summary.Introduction, 350)));
        }
        
        // Methodology Slide (only if has content)
        if (!string.IsNullOrEmpty(paper.Summary?.Methodology))
        {
            slides.Add(CreateContentSlideXml("Methodology", TruncateContent(paper.Summary.Methodology, 350)));
        }
        
        // Results Slide (only if has content)
        if (!string.IsNullOrEmpty(paper.Summary?.Results))
        {
            slides.Add(CreateContentSlideXml("Results & Findings", TruncateContent(paper.Summary.Results, 350)));
        }
        
        // Discussion Slide (only if has content)
        if (!string.IsNullOrEmpty(paper.Summary?.Discussion))
        {
            slides.Add(CreateContentSlideXml("Discussion", TruncateContent(paper.Summary.Discussion, 350)));
        }
        
        // Technical Details Slide (only if has content)
        if (!string.IsNullOrEmpty(paper.Summary?.TechnicalDetails))
        {
            slides.Add(CreateContentSlideXml("Technical Details", TruncateContent(paper.Summary.TechnicalDetails, 400)));
        }
        
        // Limitations Slide (only if has content)
        if (!string.IsNullOrEmpty(paper.Summary?.Limitations))
        {
            slides.Add(CreateContentSlideXml("Limitations", TruncateContent(paper.Summary.Limitations, 300)));
        }
        
        // Impact Slide (only if has content)
        if (!string.IsNullOrEmpty(paper.Summary?.Impact))
        {
            slides.Add(CreateContentSlideXml("Impact & Significance", TruncateContent(paper.Summary.Impact, 300)));
        }
        
        // Key Contributions Overview Slide (only if has content)
        if (paper.Contributions?.Bullets != null && paper.Contributions.Bullets.Length > 0)
        {
            slides.Add(CreateBulletListSlideXml("Key Contributions", paper.Contributions.Bullets));
            
            // Individual contribution slides (only first 3 to keep it concise)
            var maxContributions = Math.Min(3, paper.Contributions.Bullets.Length);
            for (int i = 0; i < maxContributions; i++)
            {
                var bullet = paper.Contributions.Bullets[i];
                var detail = paper.Contributions.Details != null && i < paper.Contributions.Details.Length 
                    ? TruncateContent(paper.Contributions.Details[i], 250)
                    : "This contribution represents a significant advancement in the field.";
                
                var content = $"{bullet}\n\n{detail}";
                slides.Add(CreateContentSlideXml($"Contribution {i + 1}", content));
            }
        }
        
        // Related Work Slide (only if has content, limit to 3 items)
        if (paper.Related?.Items != null && paper.Related.Items.Length > 0)
        {
            var relatedWorkContent = new StringBuilder();
            foreach (var item in paper.Related.Items.Take(3))
            {
                relatedWorkContent.AppendLine($"• {item.Title}");
                relatedWorkContent.AppendLine($"  {TruncateContent(item.Reason, 100)}");
                relatedWorkContent.AppendLine();
            }
            slides.Add(CreateContentSlideXml("Related Work", relatedWorkContent.ToString()));
        }
        
        // Thank You Slide
        slides.Add(CreateThankYouSlideXml());
        
        return slides;
    }
    
    private string TruncateContent(string content, int maxLength)
    {
        if (string.IsNullOrEmpty(content)) return "";
        if (content.Length <= maxLength) return content;
        
        // Find the last complete sentence within the limit
        var truncated = content.Substring(0, maxLength);
        var lastPeriod = truncated.LastIndexOf('.');
        var lastExclamation = truncated.LastIndexOf('!');
        var lastQuestion = truncated.LastIndexOf('?');
        
        var lastSentenceEnd = Math.Max(Math.Max(lastPeriod, lastExclamation), lastQuestion);
        
        if (lastSentenceEnd > maxLength * 0.7) // If we found a sentence end in the last 30%
        {
            return truncated.Substring(0, lastSentenceEnd + 1);
        }
        
        return truncated + "...";
    }
    
    private string CreateContentTypesXml()
    {
        return @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
  <Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/>
  <Default Extension=""xml"" ContentType=""application/xml""/>
  <Override PartName=""/ppt/presentation.xml"" ContentType=""application/vnd.openxmlformats-officedocument.presentationml.presentation.main+xml""/>
  <Override PartName=""/ppt/slideMasters/slideMaster1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.presentationml.slideMaster+xml""/>
  <Override PartName=""/ppt/slideLayouts/slideLayout1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.presentationml.slideLayout+xml""/>
  <Override PartName=""/ppt/slides/slide1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.presentationml.slide+xml""/>
  <Override PartName=""/ppt/slides/slide2.xml"" ContentType=""application/vnd.openxmlformats-officedocument.presentationml.slide+xml""/>
  <Override PartName=""/ppt/slides/slide3.xml"" ContentType=""application/vnd.openxmlformats-officedocument.presentationml.slide+xml""/>
  <Override PartName=""/ppt/slides/slide4.xml"" ContentType=""application/vnd.openxmlformats-officedocument.presentationml.slide+xml""/>
  <Override PartName=""/ppt/slides/slide5.xml"" ContentType=""application/vnd.openxmlformats-officedocument.presentationml.slide+xml""/>
  <Override PartName=""/ppt/slides/slide6.xml"" ContentType=""application/vnd.openxmlformats-officedocument.presentationml.slide+xml""/>
  <Override PartName=""/ppt/slides/slide7.xml"" ContentType=""application/vnd.openxmlformats-officedocument.presentationml.slide+xml""/>
  <Override PartName=""/ppt/slides/slide8.xml"" ContentType=""application/vnd.openxmlformats-officedocument.presentationml.slide+xml""/>
  <Override PartName=""/ppt/slides/slide9.xml"" ContentType=""application/vnd.openxmlformats-officedocument.presentationml.slide+xml""/>
  <Override PartName=""/ppt/slides/slide10.xml"" ContentType=""application/vnd.openxmlformats-officedocument.presentationml.slide+xml""/>
  <Override PartName=""/ppt/slides/slide11.xml"" ContentType=""application/vnd.openxmlformats-officedocument.presentationml.slide+xml""/>
  <Override PartName=""/ppt/slides/slide12.xml"" ContentType=""application/vnd.openxmlformats-officedocument.presentationml.slide+xml""/>
  <Override PartName=""/ppt/slides/slide13.xml"" ContentType=""application/vnd.openxmlformats-officedocument.presentationml.slide+xml""/>
  <Override PartName=""/ppt/slides/slide14.xml"" ContentType=""application/vnd.openxmlformats-officedocument.presentationml.slide+xml""/>
  <Override PartName=""/ppt/slides/slide15.xml"" ContentType=""application/vnd.openxmlformats-officedocument.presentationml.slide+xml""/>
</Types>";
    }
    
    private string CreateRelsXml()
    {
        return @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""ppt/presentation.xml""/>
</Relationships>";
    }
    
    private string CreatePresentationRelsXml()
    {
        return @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/slideMaster"" Target=""slideMasters/slideMaster1.xml""/>
  <Relationship Id=""rId2"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide"" Target=""slides/slide1.xml""/>
  <Relationship Id=""rId3"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide"" Target=""slides/slide2.xml""/>
  <Relationship Id=""rId4"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide"" Target=""slides/slide3.xml""/>
  <Relationship Id=""rId5"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide"" Target=""slides/slide4.xml""/>
  <Relationship Id=""rId6"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide"" Target=""slides/slide5.xml""/>
  <Relationship Id=""rId7"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide"" Target=""slides/slide6.xml""/>
  <Relationship Id=""rId8"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide"" Target=""slides/slide7.xml""/>
  <Relationship Id=""rId9"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide"" Target=""slides/slide8.xml""/>
  <Relationship Id=""rId10"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide"" Target=""slides/slide9.xml""/>
  <Relationship Id=""rId11"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide"" Target=""slides/slide10.xml""/>
  <Relationship Id=""rId12"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide"" Target=""slides/slide11.xml""/>
  <Relationship Id=""rId13"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide"" Target=""slides/slide12.xml""/>
  <Relationship Id=""rId14"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide"" Target=""slides/slide13.xml""/>
  <Relationship Id=""rId15"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide"" Target=""slides/slide14.xml""/>
  <Relationship Id=""rId16"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide"" Target=""slides/slide15.xml""/>
</Relationships>";
    }
    
    private string CreatePresentationXml(PaperResponse paper)
    {
        return $@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<p:presentation xmlns:a=""http://schemas.openxmlformats.org/drawingml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"" xmlns:p=""http://schemas.openxmlformats.org/presentationml/2006/main"">
  <p:sldIdLst>
    <p:sldId id=""256"" r:id=""rId2""/>
    <p:sldId id=""257"" r:id=""rId3""/>
    <p:sldId id=""258"" r:id=""rId4""/>
    <p:sldId id=""259"" r:id=""rId5""/>
    <p:sldId id=""260"" r:id=""rId6""/>
    <p:sldId id=""261"" r:id=""rId7""/>
    <p:sldId id=""262"" r:id=""rId8""/>
    <p:sldId id=""263"" r:id=""rId9""/>
    <p:sldId id=""264"" r:id=""rId10""/>
    <p:sldId id=""265"" r:id=""rId11""/>
    <p:sldId id=""266"" r:id=""rId12""/>
    <p:sldId id=""267"" r:id=""rId13""/>
    <p:sldId id=""268"" r:id=""rId14""/>
    <p:sldId id=""269"" r:id=""rId15""/>
    <p:sldId id=""270"" r:id=""rId16""/>
  </p:sldIdLst>
</p:presentation>";
    }
    
    private string CreateSlideMasterXml()
    {
        return @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<p:sldMaster xmlns:a=""http://schemas.openxmlformats.org/drawingml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"" xmlns:p=""http://schemas.openxmlformats.org/presentationml/2006/main"">
  <p:cSld>
    <p:bg>
      <p:bgPr>
        <a:solidFill>
          <a:srgbClr val=""FFFFFF""/>
        </a:solidFill>
      </p:bgPr>
    </p:bg>
  </p:cSld>
</p:sldMaster>";
    }
    
    private string CreateSlideLayoutXml()
    {
        return @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<p:sldLayout xmlns:a=""http://schemas.openxmlformats.org/drawingml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"" xmlns:p=""http://schemas.openxmlformats.org/presentationml/2006/main"">
  <p:cSld>
    <p:spTree>
      <p:nvGrpSpPr>
        <p:cNvPr id=""1"" name=""""/>
        <p:cNvGrpSpPr/>
        <p:nvPr/>
      </p:nvGrpSpPr>
      <p:grpSpPr>
        <a:xfrm>
          <a:off x=""0"" y=""0""/>
          <a:ext cx=""0"" cy=""0""/>
          <a:chOff x=""0"" y=""0""/>
          <a:chExt cx=""0"" cy=""0""/>
        </a:xfrm>
      </p:grpSpPr>
    </p:spTree>
  </p:cSld>
</p:sldLayout>";
    }
    
    private string CreateTitleSlideXml(PaperResponse paper)
    {
        var title = paper.Title ?? "Research Paper Summary";
        var authors = paper.Authors ?? "Unknown Authors";
        
        return $@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<p:sld xmlns:a=""http://schemas.openxmlformats.org/drawingml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"" xmlns:p=""http://schemas.openxmlformats.org/presentationml/2006/main"">
  <p:cSld>
    <p:bg>
      <p:bgPr>
        <a:gradFill>
          <a:gsLst>
            <a:gs pos=""0"">
              <a:srgbClr val=""1F4E79""/>
            </a:gs>
            <a:gs pos=""100000"">
              <a:srgbClr val=""2E86AB""/>
            </a:gs>
          </a:gsLst>
          <a:lin ang=""5400000"" scaled=""1""/>
        </a:gradFill>
      </p:bgPr>
    </p:bg>
    <p:spTree>
      <p:nvGrpSpPr>
        <p:cNvPr id=""1"" name=""""/>
        <p:cNvGrpSpPr/>
        <p:nvPr/>
      </p:nvGrpSpPr>
      <p:grpSpPr>
        <a:xfrm>
          <a:off x=""0"" y=""0""/>
          <a:ext cx=""0"" cy=""0""/>
          <a:chOff x=""0"" y=""0""/>
          <a:chExt cx=""0"" cy=""0""/>
        </a:xfrm>
      </p:grpSpPr>
      <p:sp>
        <p:nvSpPr>
          <p:cNvPr id=""2"" name=""Title""/>
          <p:cNvSpPr/>
          <p:nvPr>
            <p:ph type=""title""/>
          </p:nvPr>
        </p:nvSpPr>
        <p:spPr>
          <a:xfrm>
            <a:off x=""914400"" y=""457200""/>
            <a:ext cx=""7315200"" cy=""1828800""/>
          </a:xfrm>
        </p:spPr>
        <p:txBody>
          <a:bodyPr anchor=""ctr"" anchorCtr=""1""/>
          <a:lstStyle/>
          <a:p>
            <a:pPr algn=""ctr""/>
            <a:r>
              <a:rPr lang=""en-US"" sz=""4000"" b=""1"">
                <a:solidFill>
                  <a:srgbClr val=""FFFFFF""/>
                </a:solidFill>
                <a:latin typeface=""Calibri""/>
              </a:rPr>
              <a:t>{title}</a:t>
            </a:r>
          </a:p>
          <a:p>
            <a:pPr algn=""ctr""/>
            <a:r>
              <a:rPr lang=""en-US"" sz=""2400"">
                <a:solidFill>
                  <a:srgbClr val=""E0E0E0""/>
                </a:solidFill>
                <a:latin typeface=""Calibri""/>
              </a:rPr>
              <a:t>{authors}</a:t>
            </a:r>
          </a:p>
        </p:txBody>
      </p:sp>
    </p:spTree>
  </p:cSld>
</p:sld>";
    }
    
    private string CreateContentSlideXml(string title, string content)
    {
        return $@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<p:sld xmlns:a=""http://schemas.openxmlformats.org/drawingml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"" xmlns:p=""http://schemas.openxmlformats.org/presentationml/2006/main"">
  <p:cSld>
    <p:bg>
      <p:bgPr>
        <a:solidFill>
          <a:srgbClr val=""FFFFFF""/>
        </a:solidFill>
      </p:bgPr>
    </p:bg>
    <p:spTree>
      <p:nvGrpSpPr>
        <p:cNvPr id=""1"" name=""""/>
        <p:cNvGrpSpPr/>
        <p:nvPr/>
      </p:nvGrpSpPr>
      <p:grpSpPr>
        <a:xfrm>
          <a:off x=""0"" y=""0""/>
          <a:ext cx=""0"" cy=""0""/>
          <a:chOff x=""0"" y=""0""/>
          <a:chExt cx=""0"" cy=""0""/>
        </a:xfrm>
      </p:grpSpPr>
      <p:sp>
        <p:nvSpPr>
          <p:cNvPr id=""2"" name=""Title""/>
          <p:cNvSpPr/>
          <p:nvPr>
            <p:ph type=""title""/>
          </p:nvPr>
        </p:nvSpPr>
        <p:spPr>
          <a:xfrm>
            <a:off x=""914400"" y=""457200""/>
            <a:ext cx=""7315200"" cy=""914400""/>
          </a:xfrm>
        </p:spPr>
        <p:txBody>
          <a:bodyPr/>
          <a:lstStyle/>
          <a:p>
            <a:r>
              <a:rPr lang=""en-US"" sz=""3200"" b=""1"">
                <a:solidFill>
                  <a:srgbClr val=""2E86AB""/>
                </a:solidFill>
                <a:latin typeface=""Calibri""/>
              </a:rPr>
              <a:t>{title}</a:t>
            </a:r>
          </a:p>
        </p:txBody>
      </p:sp>
      <p:sp>
        <p:nvSpPr>
          <p:cNvPr id=""3"" name=""Content""/>
          <p:cNvSpPr/>
          <p:nvPr>
            <p:ph type=""body""/>
          </p:nvPr>
        </p:nvSpPr>
        <p:spPr>
          <a:xfrm>
            <a:off x=""914400"" y=""1371600""/>
            <a:ext cx=""7315200"" cy=""4572000""/>
          </a:xfrm>
        </p:spPr>
        <p:txBody>
          <a:bodyPr wrap=""square"" anchor=""t""/>
          <a:lstStyle/>
          <a:p>
            <a:r>
              <a:rPr lang=""en-US"" sz=""1800"">
                <a:solidFill>
                  <a:srgbClr val=""000000""/>
                </a:solidFill>
                <a:latin typeface=""Calibri""/>
              </a:rPr>
              <a:t>{content}</a:t>
            </a:r>
          </a:p>
        </p:txBody>
      </p:sp>
    </p:spTree>
  </p:cSld>
</p:sld>";
    }
    
    private string CreateBulletListSlideXml(string title, string[] bullets)
    {
        var bulletContent = new StringBuilder();
        foreach (var bullet in bullets)
        {
            bulletContent.AppendLine($"• {bullet}");
        }
        
        return $@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<p:sld xmlns:a=""http://schemas.openxmlformats.org/drawingml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"" xmlns:p=""http://schemas.openxmlformats.org/presentationml/2006/main"">
  <p:cSld>
    <p:bg>
      <p:bgPr>
        <a:solidFill>
          <a:srgbClr val=""FFFFFF""/>
        </a:solidFill>
      </p:bgPr>
    </p:bg>
    <p:spTree>
      <p:nvGrpSpPr>
        <p:cNvPr id=""1"" name=""""/>
        <p:cNvGrpSpPr/>
        <p:nvPr/>
      </p:nvGrpSpPr>
      <p:grpSpPr>
        <a:xfrm>
          <a:off x=""0"" y=""0""/>
          <a:ext cx=""0"" cy=""0""/>
          <a:chOff x=""0"" y=""0""/>
          <a:chExt cx=""0"" cy=""0""/>
        </a:xfrm>
      </p:grpSpPr>
      <p:sp>
        <p:nvSpPr>
          <p:cNvPr id=""2"" name=""Title""/>
          <p:cNvSpPr/>
          <p:nvPr>
            <p:ph type=""title""/>
          </p:nvPr>
        </p:nvSpPr>
        <p:spPr>
          <a:xfrm>
            <a:off x=""914400"" y=""457200""/>
            <a:ext cx=""7315200"" cy=""914400""/>
          </a:xfrm>
        </p:spPr>
        <p:txBody>
          <a:bodyPr/>
          <a:lstStyle/>
          <a:p>
            <a:r>
              <a:rPr lang=""en-US"" sz=""3200"" b=""1"">
                <a:solidFill>
                  <a:srgbClr val=""2E86AB""/>
                </a:solidFill>
                <a:latin typeface=""Calibri""/>
              </a:rPr>
              <a:t>{title}</a:t>
            </a:r>
          </a:p>
        </p:txBody>
      </p:sp>
      <p:sp>
        <p:nvSpPr>
          <p:cNvPr id=""3"" name=""Content""/>
          <p:cNvSpPr/>
          <p:nvPr>
            <p:ph type=""body""/>
          </p:nvPr>
        </p:nvSpPr>
        <p:spPr>
          <a:xfrm>
            <a:off x=""914400"" y=""1371600""/>
            <a:ext cx=""7315200"" cy=""4572000""/>
          </a:xfrm>
        </p:spPr>
        <p:txBody>
          <a:bodyPr wrap=""square"" anchor=""t""/>
          <a:lstStyle>
            <a:lvl1pPr algn=""l"" defTabSz=""914400"" rtl=""0"" eaLnBrk=""1"" latinLnBrk=""0"" hangingPunct=""1"">
              <a:lnSpc>
                <a:spcPts val=""0""/>
              </a:lnSpc>
              <a:spcBef>
                <a:spcPts val=""0""/>
              </a:spcBef>
              <a:spcAft>
                <a:spcPts val=""0""/>
              </a:spcAft>
            </a:lvl1pPr>
          </a:lstStyle>
          <a:p>
            <a:pPr lvl=""0""/>
            <a:r>
              <a:rPr lang=""en-US"" sz=""1800"">
                <a:solidFill>
                  <a:srgbClr val=""000000""/>
                </a:solidFill>
                <a:latin typeface=""Calibri""/>
              </a:rPr>
              <a:t>{bulletContent}</a:t>
            </a:r>
          </a:p>
        </p:txBody>
      </p:sp>
    </p:spTree>
  </p:cSld>
</p:sld>";
    }
    
    private string CreateThankYouSlideXml()
    {
        return @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<p:sld xmlns:a=""http://schemas.openxmlformats.org/drawingml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"" xmlns:p=""http://schemas.openxmlformats.org/presentationml/2006/main"">
  <p:cSld>
    <p:bg>
      <p:bgPr>
        <a:gradFill>
          <a:gsLst>
            <a:gs pos=""0"">
              <a:srgbClr val=""1F4E79""/>
            </a:gs>
            <a:gs pos=""100000"">
              <a:srgbClr val=""2E86AB""/>
            </a:gs>
          </a:gsLst>
          <a:lin ang=""5400000"" scaled=""1""/>
        </a:gradFill>
      </p:bgPr>
    </p:bg>
    <p:spTree>
      <p:nvGrpSpPr>
        <p:cNvPr id=""1"" name=""""/>
        <p:cNvGrpSpPr/>
        <p:nvPr/>
      </p:nvGrpSpPr>
      <p:grpSpPr>
        <a:xfrm>
          <a:off x=""0"" y=""0""/>
          <a:ext cx=""0"" cy=""0""/>
          <a:chOff x=""0"" y=""0""/>
          <a:chExt cx=""0"" cy=""0""/>
        </a:xfrm>
      </p:grpSpPr>
      <p:sp>
        <p:nvSpPr>
          <p:cNvPr id=""2"" name=""Title""/>
          <p:cNvSpPr/>
          <p:nvPr>
            <p:ph type=""title""/>
          </p:nvPr>
        </p:nvSpPr>
        <p:spPr>
          <a:xfrm>
            <a:off x=""914400"" y=""457200""/>
            <a:ext cx=""7315200"" cy=""1828800""/>
          </a:xfrm>
        </p:spPr>
        <p:txBody>
          <a:bodyPr anchor=""ctr"" anchorCtr=""1""/>
          <a:lstStyle/>
          <a:p>
            <a:pPr algn=""ctr""/>
            <a:r>
              <a:rPr lang=""en-US"" sz=""4000"" b=""1"">
                <a:solidFill>
                  <a:srgbClr val=""FFFFFF""/>
                </a:solidFill>
                <a:latin typeface=""Calibri""/>
              </a:rPr>
              <a:t>Thank You</a:t>
            </a:r>
          </a:p>
          <a:p>
            <a:pPr algn=""ctr""/>
            <a:r>
              <a:rPr lang=""en-US"" sz=""2000"">
                <a:solidFill>
                  <a:srgbClr val=""E0E0E0""/>
                </a:solidFill>
                <a:latin typeface=""Calibri""/>
              </a:rPr>
              <a:t>SciDigest AI</a:t>
            </a:r>
          </a:p>
        </p:txBody>
      </p:sp>
    </p:spTree>
  </p:cSld>
</p:sld>";
    }
}
