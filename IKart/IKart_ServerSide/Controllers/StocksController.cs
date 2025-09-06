using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using IKart_ServerSide.Models;
using IKart_Shared.DTOs;

namespace IKart_ServerSide.Controllers
{
    [RoutePrefix("api/stocks")]
    public class StocksController : ApiController
    {
        IKartEntities1 db = new IKartEntities1();

        // GET: api/stocks
        [HttpGet, Route("")]
        public IHttpActionResult GetStocks()
        {
            var stocks = db.Stocks
                .Select(s => new StocksDto
                {
                    StockId = s.Stock_Id,
                    Category = s.CategoryName,
                    SubCategory = s.SubCategoryName,
                    TotalStocks = s.Total_Stocks,
                    AvailableStocks = s.Available_Stocks
                })
                .ToList();

            return Ok(stocks);
        }
    }
}
