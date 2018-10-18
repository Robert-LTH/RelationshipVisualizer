//-----------------------------------------------------------------------------
//<filedescription file="PrototypeControl.cs" company="Microsoft">
//  <copyright>
//     Copyright © Microsoft Corporation.  All rights reserved.
//      THIS CODE SAMPLE IS MADE AVAILABLE TO YOU WITHOUT WARRANTY OF ANY KIND
//      AND CONFERS NO RIGHTS ON YOU WHATSOEVER EXCEPT THE RIGHT TO REVIEW IT
//      SOLELY AS A REFERENCE.  THE ENTIRE RISK OF USE OR RESULTS FROM USE OF
//      THIS CODE SAMPLE REMAINS WITH YOU.
//  </copyright>
//  <purpose>
//     Sample dialog
//  </purpose>
//  <notes>
//  </notes>
//</filedescription>
//-----------------------------------------------------------------------------

#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Microsoft.ConfigurationManagement.ManagementProvider;
using System.Globalization;

#endregion

namespace Microsoft.ConfigurationManagement.AdminConsole.RelationshipVisualizerControl
{
    /// <summary>
    /// Page control representing SMS security General management object properties.
    /// </summary>
    public partial class RelationshipVisualizerControl : SmsPageControl
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="pageData"></param>
        public RelationshipVisualizerControl(SmsPageData pageData)
            : base(pageData)
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes the page control when it is created.
        /// </summary>
        public override void InitializePageControl()
        {
            base.InitializePageControl();
            treeView1.Nodes.Add(PropertyManager["CollectionID"].StringValue,PropertyManager["Name"].StringValue + " (" + PropertyManager["CollectionID"].StringValue + ")");
            treeView1.Nodes[0].ExpandAll();
            GetNestedCollection(PropertyManager["CollectionID"].StringValue);
        }
        /// <summary>
        /// Dig into the relationships of collections
        /// </summary>
        /// <param name="CollectionID"></param>
        private void GetNestedCollection(string CollectionID)
        {
            SmsBackgroundWorker worker = new SmsBackgroundWorker();
            worker.QueryProcessorObjectReady += new EventHandler<QueryProcessorObjectEventArgs>(worker_QueryProcessorObjectReady);
            worker.QueryProcessorCompleted += new EventHandler<RunWorkerCompletedEventArgs>(worker_QueryProcessorCompleted);
            string queryToRun = string.Format(CultureInfo.InvariantCulture, "SELECT SMS_CollectionDependencies.*,SMS_Collection.Name FROM SMS_CollectionDependencies INNER JOIN SMS_Collection ON SMS_CollectionDependencies.DependentCollectionID = SMS_Collection.CollectionID WHERE RelationshipType = '2' AND SourceCollectionID='{0}'", PropertyManager.ConnectionManager.EscapeQueryString(CollectionID, ConnectionManagerBase.EscapeQuoteType.SingleQuote));
            QueryProcessor.ProcessQuery(worker,queryToRun);
        }
        
        /// <summary>
        /// This event is fired for each item received from the above query.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void worker_QueryProcessorObjectReady(object sender, QueryProcessorObjectEventArgs e)
        {
            foreach (IResultObject queryResult in (IResultObject)e.ResultObject)
            {
                IResultObject SMS_CollectionDependencies = queryResult.GenericsArray[1];
                IResultObject SMS_Collection = queryResult.GenericsArray[0];
                string DependantCollectionID = "";
                string DependantCollectionName = "";
                string SourceCollectionID = "";
                foreach (IResultObject Resource1 in SMS_CollectionDependencies)
                {
                    Console.WriteLine(Resource1["DependentCollectionID"].StringValue);
                    DependantCollectionID = Resource1["DependentCollectionID"].StringValue;
                    SourceCollectionID = Resource1["SourceCollectionID"].StringValue;
                }
                foreach (IResultObject Resource1 in SMS_Collection)
                {
                    DependantCollectionName = Resource1["Name"].StringValue;
                }
                TreeNode[] tn = treeView1.Nodes.Find(SourceCollectionID, true);
                tn[0].Nodes.Add(DependantCollectionID, DependantCollectionName + " (" + DependantCollectionID + ")");
                treeView1.Nodes[0].ExpandAll();
                GetNestedCollection(DependantCollectionID);
            }
            e.ResultObject.Dispose();
        }

        /// <summary>
        /// This event is fired when all results have been received from the above query.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void worker_QueryProcessorCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SmsBackgroundWorker worker = sender as SmsBackgroundWorker;
            System.Diagnostics.Debug.Assert(worker != null, "Completion event send from unexpected sender");
            worker.Dispose();
        }
    }
}
