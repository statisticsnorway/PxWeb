﻿using PCAxis.Menu;
using PCAxis.Paxiom;

namespace Px.Abstractions.Interfaces
{
    public interface IDataSource
    {
        /// <summary>
        /// Get Menu
        /// </summary>
        /// <param name="id"></param>
        /// <param name="language"></param>
        /// <param name="selectionExists"></param>
        /// <returns></returns>
        Item? CreateMenu(string id, string language, out bool selectionExists);

        /// <summary>
        /// Create builder
        /// </summary>
        /// <param name="id">Table id</param>
        /// <param name="language">Language</param>
        /// <returns></returns>
        IPXModelBuilder? CreateBuilder(string id, string language);

        //string GetSource(IDatabaseInfo dbi, PCAxis.Paxiom.PXModel model, string language);
    }
}