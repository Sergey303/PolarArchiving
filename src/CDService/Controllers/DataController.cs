﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CDService.Models;

namespace CDService.Controllers
{
    //[Route("data")]
    //[ApiController]
    public class DataController : ControllerBase
    {
        // GET: data/get
        [HttpGet]
        public IActionResult Get(string id, string dt)
        {
            Record rec = new Record()
            {
                id = "p_ivanov",
                ty = "person",
                arcs = new Arc[] 
                {
                    new ArcField() { alt="field", prop="name", text="Иванов Иван Иванович" },
                    new ArcDirect() { alt="direct", prop="father", rec = new Record()
                        { id="p_ivanov_ip", ty="person", arcs=new Arc[] 
                            { new ArcField() { alt = "field", prop = "name", text = "Иванов Иван Петрович" } } } },
                }
            };
            //id = "syp2001-p-marchuk_e";
            //XElement xresult = Turgunda7.SObjects.GetItemByIdSpecial(id);
            XElement xproba = Turgunda7.SObjects.GetItemById(id, 
                new XElement("record"));
            if (xproba != null)
            {
                string type_id = xproba.Attribute("type").Value;
                XElement format = TurgundaCommon.ModelCommon.formats.Elements()
                    .First(el => el.Attribute("type").Value == type_id);

                XElement xresult = Turgunda7.SObjects.GetItemById(id, format);
                rec = XElement2Record(xresult);
            }
            return new ObjectResult(rec);
        }
        private static Record XElement2Record(XElement xel)
        {
            string id = xel.Attribute("id").Value;
            string ty = xel.Attribute("type")?.Value;
            Arc[] arcs = xel.Elements().Select<XElement, Arc>(xe =>
                {
                    if (xe.Name == "field")
                        return new ArcField
                        {
                            alt = "field",
                            prop = xe.Attribute("prop").Value,
                            text = xe.Value
                        };
                    else if (xe.Name == "direct")
                        return new ArcDirect
                        {
                            alt = "direct",
                            prop = xe.Attribute("prop").Value,
                            rec = XElement2Record(xe.Element("record"))
                        };
                    else if (xe.Name == "inverse")
                        return new ArcInverse
                        {
                            alt = "inverse",
                            prop = xe.Attribute("prop").Value,
                            recs = xe.Elements("record")
                                .Select(r => XElement2Record(r))
                                .ToArray()
                        };
                    else return null;
                }).ToArray();
            return new Record()
            {
                id = id, ty = ty, arcs = arcs
            };
        }
    }
}