using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberSystem.Domain.Entities;
using UberSystem.Domain.Interfaces;
using UberSystem.Domain.Interfaces.Services;
using UberSystem.Domain.Repository;

namespace UberSystem.Service
{
    public interface IGSPService
    {
        Task ImportFile(IFormFile file);
    }
    public class GSPService : IGSPService
    {
        private readonly IGSPRepository gSPRepository;

        public GSPService(IGSPRepository gSPRepository)
        {
            this.gSPRepository = gSPRepository;
        }

        private string ReplaceString(string stringReplace)
        {
            if (stringReplace != null)
            {
                stringReplace = stringReplace.Replace("(","").Replace("'", "").Replace("_", " ").Replace(")", "");
            }
            return stringReplace;
        }
        public async Task ImportFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File không hợp lệ. Vui lòng tải lên file Excel.");
            }

            List<GSP> gspList = new List<GSP>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);

                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; 
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var gsp = new GSP
                        {
                            Id = Convert.ToInt64(worksheet.Cells[row, 1].Value),
                            Index = Convert.ToInt32(worksheet.Cells[row, 2].Value),
                            VehicleID = worksheet.Cells[row, 3].Value?.ToString(),
                            PStart = worksheet.Cells[row, 4].Value?.ToString(),
                            PTerm = worksheet.Cells[row, 5].Value?.ToString(),
                            PEnd = worksheet.Cells[row, 6].Value?.ToString(),
                            PreRouted = ReplaceString(worksheet.Cells[row, 7].Value?.ToString()),
                            Freg = Convert.ToInt32(worksheet.Cells[row, 8].Value),
                            Label = Convert.ToBoolean(worksheet.Cells[row, 9].Value),
                            Regions = ReplaceString(worksheet.Cells[row, 10].Value?.ToString())
                        };

                        gspList.Add(gsp); // Thêm từng đối tượng GSP vào danh sách
                    }
                }
            }
            await gSPRepository.Add(gspList);
        }
    }
}
