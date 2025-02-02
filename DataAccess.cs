//
// Copyright (c) 2022 by Acentus
// Developed by: Luis Cabrera
// gigocabrera@outlook.com
//

using System;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text;

namespace CoreCumulativeReorderReport
{
    class DataAccess
    {
        string conn = ConfigurationManager.ConnectionStrings["arxConnection"].ToString();

        //static DateTime start90days = App.FirstDayMonth.AddMonths(-3);
        //static DateTime end90days = start90days.AddMonths(3).AddTicks(-1);

        public DataTable GetDataTable(string sql)
        {
            using (SqlConnection cn = new SqlConnection(conn))
            {
                cn.Open();
                using (SqlDataAdapter da = new SqlDataAdapter(sql, cn))
                {
                    da.SelectCommand.CommandTimeout = 120;
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    return ds.Tables[0];
                }
            }
        }

        public SqlDataReader GetDataReader(string sql)
        {
            using (SqlConnection cn = new SqlConnection(conn))
            {
                SqlCommand cmd = new SqlCommand(sql, cn);
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                return dr;
            }
        }

        public int GetCount(string sql)
        {
            using (SqlConnection cn = new SqlConnection(conn))
            {
                SqlCommand cmd = new SqlCommand(sql, cn);
                cn.Open();
                int count = (int)cmd.ExecuteScalar();
                return count;
            }
        }

        public DataTable GetSalesReps()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append("    REP.ID, ");
            sb.Append("    CONCAT(REP.FIRSTNAME, ' ', REP.LASTNAME) AS SALESREP ");
            sb.Append("FROM AR1SALES REP ");
            sb.Append("WHERE REP.ISACTIVE = 1; ");
            return GetDataTable(sb.ToString());
        }

        public int GetTotalActivePatients(int repid)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append("    COUNT(PAT.ID) AS TOTAL ");
            sb.Append("FROM AR1PAT PAT ");
            sb.Append("WHERE PAT.PATIENTCATEGORY = 'CGM' ");
            sb.Append("AND PAT.PATIENTSTATUS = 'A' ");
            sb.Append("AND PAT.SALESID = " + repid + "; ");
            return GetCount(sb.ToString());
        }

        public int GetTotalExpVisitNotes(int repid)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append("    COUNT(DISTINCT(WO.PATIENTID)) ");
            sb.Append("FROM AR1ORDW WO ");
            sb.Append("LEFT JOIN AR1PAT PAT ON PAT.ID = WO.PATIENTID ");
            sb.Append("WHERE WO.NOTESEXPIREDATE < '").Append(App.LastDayMonth).Append("' ");
            sb.Append("AND PAT.PATIENTSTATUS = 'A' ");
            sb.Append("AND WO.BILLTYPE IN('M','Q') ");
            sb.Append("AND PAT.PATIENTCATEGORY = 'CGM' ");
            sb.Append("AND WO.NOTESEXPIREDATE < WO.LASTDATEBILLED ");
            sb.Append("AND PAT.SALESID = " + repid + "; ");
            return GetCount(sb.ToString());
        }

        public int GetTotalExpSWO(int repid)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append("    COUNT(DISTINCT(WO.PATIENTID)) ");
            sb.Append("FROM AR1ORDW WO ");
            sb.Append("LEFT JOIN AR1PAT PAT ON PAT.ID = WO.PATIENTID ");
            sb.Append("WHERE WO.CMNEXPIRE < '").Append(App.LastDayMonth).Append("' ");
            sb.Append("AND PAT.PATIENTSTATUS = 'A' ");
            sb.Append("AND WO.BILLTYPE IN('M','Q') ");
            sb.Append("AND PAT.PATIENTCATEGORY = 'CGM' ");
            sb.Append("AND WO.CMNEXPIRE < WO.LASTDATEBILLED ");
            sb.Append("AND PAT.SALESID = " + repid + "; ");
            return GetCount(sb.ToString());
        }

        public int GetCurrentRenewal(int repid)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT SUM(TOTAL) AS TOTAL ");
            sb.Append("FROM( ");
            //             Handle 30-day orders
            sb.Append("    SELECT ");
            sb.Append("        COUNT(DISTINCT(WO.PATIENTID)) AS TOTAL ");
            sb.Append("    FROM AR1ORDW WO ");
            sb.Append("    WHERE WO.LASTDATEBILLED BETWEEN '").Append(App.FirstDayMonth).Append("' AND '").Append(App.LastDayMonth).Append("' ");
            sb.Append("        AND WO.BILLTYPE = 'P' ");
            sb.Append("        AND WO.ITEMID IN(720,1658,1960,1955,1964,1965) ");
            sb.Append("        AND WO.RECORDTYPE = 'M' ");
            sb.Append("        AND WO.PATIENTID IN( ");
            sb.Append("            SELECT ID FROM AR1PAT PAT WHERE PAT.PATIENTCATEGORY = 'CGM' AND PAT.PATIENTSTATUS = 'A' AND PAT.SALESID = ").Append(repid);
            sb.Append("        ) ");
            sb.Append("    UNION ALL ");
            //             Handle 90-day orders
            sb.Append("    SELECT ");
            sb.Append("        COUNT(DISTINCT(WO.PATIENTID)) AS TOTAL ");
            sb.Append("    FROM AR1ORDW WO ");
            sb.Append("    WHERE WO.LASTDATEBILLED BETWEEN '").Append(App.FirstDay3MonthsAgo).Append("' AND '").Append(App.LastDayMonth).Append("' ");
            sb.Append("        AND WO.BILLTYPE = 'P' ");
            sb.Append("        AND WO.ITEMID IN(720,1658,1960,1955,1964,1965) ");
            sb.Append("        AND WO.RECORDTYPE = 'Q' ");
            sb.Append("        AND WO.PATIENTID IN( ");
            sb.Append("            SELECT ID FROM AR1PAT PAT WHERE PAT.PATIENTCATEGORY = 'CGM' AND PAT.PATIENTSTATUS = 'A' AND PAT.SALESID = ").Append(repid);
            sb.Append("        ) ");
            sb.Append(") AS CombinedResults; ");
            return GetCount(sb.ToString());
        }

        public int GetTotalConfirmedOrdersThisMonth()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append("    COUNT(DISTINCT(WO.PATIENTID)) AS TOTAL ");
            sb.Append("FROM AR1ORDW WO ");
            sb.Append("INNER JOIN AR1PAT PAT ON PAT.ID = WO.PATIENTID ");
            sb.Append("WHERE WO.LASTDATEBILLED BETWEEN '").Append(App.FirstDayMonth).Append("' AND '").Append(App.LastDayMonth).Append("' ");
            sb.Append("AND PAT.PATIENTCATEGORY = 'CGM' ");
            sb.Append("AND WO.BILLTYPE = 'P' ");
            sb.Append("AND PAT.PATIENTSTATUS = 'A' ");
            return GetCount(sb.ToString());
        }
    }
}