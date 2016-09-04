# MergeXML Tool
## 1. Description
MergeXML is a tool that can be used to merge multiple XML tools into one file. The tool do the merge in three phases:
 * The Cleanup phase: making sure that the XML definition tag is the first tag in the file.
 * The Transform phase: applying XSLT transformation only if the XSLT path has been provided. The XSLT file provided with the tool will add the indentation and line-breakes to the XML, removes the comments and remove unnecessary whitespaces to the right and left of the entity values. One can replace the XSLT transformation template and customize to suite their needs.
 * The Merge phase: merges the XML files using the provided node path. 

## 2. Parameters
 * **`[files Path]`**: Path to where the files to be merged are located. 
 * **`[Destination File Name]`**: The resulted file name and path.
 * **`[XML node path]`**: The XML node to merge.
 * **`[XSLT file path]`**: Optional, The XSLT file path and name.

## 3. Call Example
* `MergeXML.exe "C:\TEMP\" "C:\TEMP\ResultFile.xml" "OrderList" "XSLT/XSLTIndent.xslt"`
* `MergeXML.exe -help`

---
<span style="color:blue; font-size:12px;">&copy; 2016 [Ghiath Al-Qaisi]</span>


[Ghiath Al-Qaisi]: mailto:ghiath.alqaisi@gmail.com "ghiath.alqaisi@gmail.com"