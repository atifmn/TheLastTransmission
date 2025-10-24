// <copyright file="SpreadsheetPage.razor.cs" company="UofU-CS3500">
// Copyright (c) 2024 UofU-CS3500. All rights reserved.
// </copyright>

using System.Diagnostics;
using Formula;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Spreadsheets;

namespace GUI.Components.Pages;

/// <summary>
/// TODO: Fill in
/// </summary>
public partial class SpreadsheetPage
{
    /// <summary>
    /// Based on your computer, you could shrink/grow this value based on performance.
    /// </summary>
    private const int Rows = 50;

    /// <summary>
    /// Number of columns, which will be labeled A-Z.
    /// </summary>
    private const int Cols = 26;

    /// <summary>
    /// Provides an easy way to convert from an index to a letter (0 -> A)
    /// </summary>
    private char[] Alphabet { get; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    /// <summary>
    ///   Gets or sets the name of the file to be saved
    /// </summary>
    private string FileSaveName { get; set; } = "Spreadsheet.sprd";
    
    /// <summary>
    ///   <para> Gets or sets the data for all the cells in the spreadsheet GUI. </para>
    ///   <remarks>Backing Store for HTML</remarks>
    /// </summary>
    private string?[,] CellsBackingStore { get; set; } = new string?[Rows, Cols];

    private string _selectedCell;

    private ElementReference _contentInputBox;
    
    private int currentRow;
    private int currentColumn;

    private Spreadsheet _spreadsheet = new Spreadsheet();

    /// <summary>
    /// Handler for when a cell is clicked
    /// </summary>
    /// <param name="row">The row component of the cell's coordinates</param>
    /// <param name="col">The column component of the cell's coordinates</param>
    private void CellClicked( int row, int col )
    {
        int newRow = row + 1;
        
        _selectedCell = Alphabet[col].ToString() + newRow;

        _contentInputBox.FocusAsync();
        
        currentRow = row;
        currentColumn = col;
    }
    
    /// <summary>
    /// Saves the current spreadsheet, by providing a download of a file
    /// containing the json representation of the spreadsheet.
    /// </summary>
    private async void SaveFile()
    {
        await JsRuntime.InvokeVoidAsync( "downloadFile", FileSaveName, 
            "replace this with the json representation of the current spreadsheet" );
    }

    /// <summary>
    /// This method will run when the file chooser is used, for loading a file.
    /// Uploads a file containing a json representation of a spreadsheet, and 
    /// replaces the current sheet with the loaded one.
    /// </summary>
    /// <param name="args">The event arguments, which contains the selected file name</param>
    private async void HandleFileChooser( EventArgs args )
    {
        try
        {
            string fileContent = string.Empty;

            InputFileChangeEventArgs eventArgs = args as InputFileChangeEventArgs ?? throw new Exception("unable to get file name");
            if ( eventArgs.FileCount == 1 )
            {
                var file = eventArgs.File;
                if ( file is null )
                {
                    return;
                }

                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream);

                // fileContent will contain the contents of the loaded file
                fileContent = await reader.ReadToEndAsync();

                // TODO: Use the loaded fileContent to replace the current spreadsheet

                StateHasChanged();
            }
        }
        catch ( Exception e )
        {
            Debug.WriteLine( "an error occurred while loading the file..." + e );
        }
    }

    private async Task ContentsChanged(ChangeEventArgs obj)
    {
        string? value = obj.Value as string;
    
        IEnumerable<string> changedCells = null;
    
        object cellValue;
        
        if (value != null)
        {
            try
            {
                changedCells = _spreadsheet.SetContentsOfCell(_selectedCell, value);
            }
            catch (Exception ex)
            {
                CellsBackingStore[currentRow, currentColumn] = "ERROR";
                return;
            }
           
        }
        
        try
        {
            cellValue = _spreadsheet.GetCellValue(_selectedCell);
        }
        catch (Exception ex)
        {
            CellsBackingStore[currentRow, currentColumn] = "ERROR";
            return;
        }

        if (changedCells != null)
            foreach (var changedCell in changedCells)
            {
                int[] rowAndCol = ConvertToRowAndCol(changedCell);

                CellsBackingStore[rowAndCol[0], rowAndCol[1]] = _spreadsheet.GetCellValue(changedCell).ToString();
            }

        CellsBackingStore[currentRow, currentColumn] = cellValue.ToString();
    }

    private int[] ConvertToRowAndCol(string cellName)
    {
        int row = int.Parse(cellName[1].ToString());

        int counter = 0;
        
        char cellCol = cellName[0];  

        foreach (char c in Alphabet)
        {
            if (c == cellCol)
            {
                break;
            }
            
            counter++;
        }

        int col = counter;
        
        return [row - 1, col];
    }
}
