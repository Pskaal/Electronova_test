﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExcelDataReader;

public class ExcelImporter
{
    public static List<List<string>> ImportExcel(string filePath)
    {
        List<List<string>> data = new List<List<string>>();

        try
        {
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read)) //"..\\..\\..\\Vedlegg\\Trekkekum.xlsx"
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    do
                    {
                        while (reader.Read())
                        {
                            List<string> row = new List<string>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row.Add(reader.GetValue(i).ToString());
                            }
                            data.Add(row);
                        }
                    } while (reader.NextResult());
                }
            }
        }
        catch (Exception ex)
        {
            return null;
        }

        return data;
    }
}