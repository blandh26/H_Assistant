using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using H_Assistant.Framework.PhysicalDataModel;
using H_Assistant.Framework.Util;
using SqlSugar;

namespace H_Assistant.Framework.Exporter
{
    public class OracleExporter : Exporter, IExporter
    {
        private readonly IDbMaintenance _dbMaintenance;
        private readonly IDbFirst _dbMaintenanceDbFirst;
        public OracleExporter(string connectionString) : base(connectionString)
        {
            _dbMaintenance = SugarFactory.GetDbMaintenance(DbType.Oracle, DbConnectString);
            _dbMaintenanceDbFirst = SugarFactory.GetDbMaintenanceDbFirst(DbType.Oracle, DbConnectString);
        }
        public OracleExporter(string connectionString, string dbName) : base(connectionString, dbName)
        {
            _dbMaintenance = SugarFactory.GetDbMaintenance(DbType.Oracle, DbConnectString);
            _dbMaintenanceDbFirst = SugarFactory.GetDbMaintenanceDbFirst(DbType.Oracle, DbConnectString);
        }

        public OracleExporter(string tableName, List<Column> columns) : base(tableName, columns)
        {

        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public override Model Init()
        {
            var model = new Model { Database = "Oracle" };
            try
            {
                model.Tables = this.GetTables();
                model.Views = this.GetViews();
                model.Procedures = new Procedures();//暂时不支持存储过程 this.GetProcedures();
                return model;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Private
        private Tables GetTables()
        {
            #region MyRegion
            var tables = new Tables();
            var tableList = _dbMaintenance.GetTableInfoList(false);
            tableList.ForEach(tb =>
            {
                if (tables.ContainsKey(tb.Name))
                {
                    return;
                }
                var table = new Table
                {
                    Id = tb.Name,
                    Name = tb.Name,
                    DisplayName = tb.Name,
                    Comment = tb.Description,
                    CreateDate = tb.CreateDate,
                    ModifyDate = tb.ModifyDate
                };
                tables.Add(tb.Name, table);
            });
            return tables;
            #endregion
        }

        private Views GetViews()
        {
            #region MyRegion
            var views = new Views();
            var viewList = _dbMaintenance.GetViewInfoList(false);
            viewList.ForEach(v =>
            {
                if (views.ContainsKey(v.Name))
                {
                    return;
                }
                var view = new View()
                {
                    Id = v.Name,
                    Name = v.Name,
                    DisplayName = v.Name,
                    Comment = v.Description,
                    CreateDate = v.CreateDate,
                    ModifyDate = v.ModifyDate
                };
                views.Add(v.Name, view);
            });
            return views;
            #endregion
        }

        private Procedures GetProcedures()
        {
            #region MyRegion
            var procDic = new Procedures();
            var procInfoList = _dbMaintenance.GetProcInfoList(false);
            var dbName = _dbMaintenance.Context.Ado.Connection.Database;
            var procList = procInfoList.Where(x => x.Schema == dbName).ToList();
            procList.ForEach(p =>
            {
                if (procDic.ContainsKey(p.Name))
                {
                    return;
                }
                var proc = new Procedure()
                {
                    Id = p.Name,
                    Name = p.Name,
                    DisplayName = p.Name,
                    Comment = p.Description,
                    CreateDate = p.CreateDate,
                    ModifyDate = p.ModifyDate
                };
                procDic.Add(p.Name, proc);
            });
            return procDic;
            #endregion
        }
        #endregion

        public override List<DataBase> GetDatabases(string defaultDatabase = "")
        {
            #region MyRegion
            var dbClient = SugarFactory.GetInstance(DbType.Oracle, DbConnectString);
            var dataBaseList = dbClient.Ado.SqlQuery<string>("SELECT USERNAME FROM ALL_USERS ORDER BY USERNAME");
            return new List<DataBase>
            {
                new DataBase
                {
                    DbName = defaultDatabase,
                    IsSelected = true
                }
            };
            #endregion
        }

        /// <summary>
        /// 根据对象ID获取列信息
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public override Columns GetColumnInfoById(string objectId)
        {
            #region MyRegion
            var columns = new Columns(500);
            var schema = DbName.Contains(":") ? DbName.Split(':')[1] : DbName;
            var dbClient = SugarFactory.GetInstance(DbType.Oracle, DbConnectString);
            var sql = $@"
select
    COLUMN_NAME AS DbColumnName
    , DATA_TYPE AS DataType
    , DATA_DEFAULT AS DefaultValue
    , data_length AS Length
    , DATA_PRECISION AS DecimalDigits
    , DATA_SCALE AS Scale 
from
    user_tab_columns 
where
    upper(table_name) = upper('{objectId}') "; 
            var colList = _dbMaintenance.GetColumnInfosByTableName(objectId, false);
            
            List<DbColumnInfo> colList_temp = dbClient.SqlQueryable<DbColumnInfo>(sql).ToList();
            // var viewList = _dbMaintenance.GetColumnInfosByTableName(objectId, false);
            colList.ForEach(v =>
            {
                if (columns.ContainsKey(v.DbColumnName==null?"": v.DbColumnName.ToString()))
                {
                    return;
                }
                var column = new Column(v.DbColumnName, v.DbColumnName, v.DbColumnName, v.DataType, v.ColumnDescription);
                column.LengthName = "";
                var temp = colList_temp.FirstOrDefault(x => x.DbColumnName == v.DbColumnName);
                if (temp != null)
                {
                    v.DataType = temp.DataType.ToUpper();
                }
                var dataType = v.DataType.ToLower();
                switch (dataType)
                {
                    case "char":
                    case "nchar":
                    case "time":
                    case "text":
                    case "string":
                    case "binary":
                    case "varchar":
                    case "varchar2":
                    case "nvarchar":
                    case "nvarchar2":
                    case "varbinary":
                    case "datetime2":
                    case "datetimeoffset":
                        {
                            var temp1 = colList_temp.FirstOrDefault(x => x.DbColumnName == v.DbColumnName);
                            if (temp1!= null) {
                                column.LengthName = $"({temp1.Length})";
                            }
                        }
                        break;
                    case "number":
                    case "numeric":
                    case "decimal":
                        {
                            var temp2 = colList_temp.FirstOrDefault(x => x.DbColumnName == v.DbColumnName);
                            if (temp2 != null)
                            {
                                column.LengthName = $"({temp2.DecimalDigits},{temp2.Scale})";
                            }
                        }
                        break;
                }

                column.ObjectId = objectId.ToString();
                column.ObjectName = v.DbColumnName;
                column.IsIdentity = v.IsIdentity;
                column.IsNullable = v.IsNullable;
                column.DefaultValue = !string.IsNullOrEmpty(v.DefaultValue) && v.DefaultValue.Contains("((") ? v.DefaultValue.Replace("((", "").Replace("))", "") : v.DefaultValue;
                column.DataType = v.DataType.ToUpper();
                temp = colList_temp.FirstOrDefault(x => x.DbColumnName == v.DbColumnName);
                if (temp != null)
                {
                    column.DefaultValue = !string.IsNullOrEmpty(temp.DefaultValue) && temp.DefaultValue.Contains("((") ? 
                    temp.DefaultValue.Replace("((", "").Replace("))", "") : temp.DefaultValue;
                }
                column.OriginalName = v.DbColumnName;
                column.Comment = v.ColumnDescription;
                column.IsPrimaryKey = v.IsPrimarykey;
                column.CSharpType = OracleDbTypeMapHelper.MapCsharpType(v.DataType, v.IsNullable);
                columns.Add(v.DbColumnName==null ? "" : v.DbColumnName.ToString(), column);
            });
            return columns;
            #endregion
        }

        public override string GetScriptInfoById(string objectId, DbObjectType objectType)
        {
            #region MyRegion
            var scriptInfo = _dbMaintenance.GetScriptInfo(objectId, objectType);
            return scriptInfo.Definition;
            #endregion
        }

        /// <summary>
        /// 更新表/视图/存储过程对象注释
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public override bool UpdateObjectRemark(string objectName, string remark, DbObjectType objectType = DbObjectType.Table)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 更新列注释
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public override bool UpdateColumnRemark(Column columnInfo, string remark, DbObjectType objectType = DbObjectType.Table)
        {
                if (objectType == DbObjectType.Table)
                {
                    var columnName = columnInfo.Name;
                    var tableName = columnInfo.ObjectId;
                    if (_dbMaintenance.IsAnyColumnRemark(columnName, tableName))
                    {
                        _dbMaintenance.DeleteColumnRemark(columnName, tableName);
                    }
                    return _dbMaintenance.AddColumnRemark(columnName, tableName, remark);
                }
            throw new NotSupportedException();
        }

        public override string CreateTableSql()
        {
            return "";
        }

        public override string SelectSql()
        {
            #region MyRegion
            var strSql = new StringBuilder("SELECT ");
            var tempCol = new StringBuilder();
            Columns.ForEach(col =>
            {
                tempCol.Append($"{col.Name},");
            });
            var tempSql = tempCol.ToString().TrimEnd(',');
            strSql.Append($"{tempSql} FROM {TableName}");
            return strSql.ToString();
            #endregion
        }

        public override string InsertSql()
        {
            #region MyRegion
            var tempCols = Columns.Where(x => x.IsIdentity == false).ToList();
            var strSql = new StringBuilder($"INSERT INTO {TableName} (");
            var tempCol = new StringBuilder();
            tempCols.ForEach(col =>
            {
                tempCol.Append($"{col.Name},");
            });
            strSql.Append($"{tempCol.ToString().TrimEnd(',')}) VALUES (");
            tempCol.Clear();
            tempCols.ForEach(col =>
            {
                if (col.DataType != "int" || col.DataType != "bigint")
                {
                    tempCol.Append("''");
                }
                tempCol.Append($",");
            });
            strSql.Append($"{tempCol.ToString().TrimEnd(',')})");
            return strSql.ToString();
            #endregion
        }

        public override string UpdateSql()
        {
            #region MyRegion
            var tempCols = Columns.Where(x => x.IsIdentity == false).ToList();
            var strSql = new StringBuilder($"UPDATE {TableName} SET ");
            var tempCol = new StringBuilder();
            tempCols.ForEach(col =>
            {
                tempCol.Append($"{col.Name}=");
                if (col.DataType == "int" || col.DataType == "bigint")
                {
                    tempCol.Append("0");
                }
                else
                {
                    tempCol.Append("''");
                }
                tempCol.Append($",");
            });
            strSql.Append($"{tempCol.ToString().TrimEnd(',')} WHERE ");
            tempCol.Clear();
            var j = 0;
            Columns.ForEach(col =>
            {
                if (j == 0)
                {
                    tempCol.Append($"{col.Name}=");
                    if (col.DataType == "int" || col.DataType == "bigint")
                    {
                        tempCol.Append("0");
                    }
                    else
                    {
                        tempCol.Append("''");
                    }
                }
                j++;
            });
            strSql.Append(tempCol);
            return strSql.ToString();
            #endregion
        }

        public override string DeleteSql()
        {
            #region MyRegion
            var strSql = new StringBuilder($"DELETE FROM {TableName} WHERE ");
            var tempCol = new StringBuilder();
            var j = 0;
            Columns.ForEach(col =>
            {
                if (j == 0)
                {
                    tempCol.Append($"{col.Name}=");
                    if (col.DataType == "int" || col.DataType == "bigint")
                    {
                        tempCol.Append("0");
                    }
                    else
                    {
                        tempCol.Append("''");
                    }
                }
                j++;
            });
            strSql.Append(tempCol);
            return strSql.ToString();
            #endregion
        }

        public override string AddColumnSql()
        {
            return "";
        }

        public override string AlterColumnSql()
        {
            return "";
        }

        public override string DropColumnSql()
        {
            return "";
        }

        /// <summary>
        /// 获取所有表信息
        /// </summary>
        /// <returns></returns>
        public override List<DbTableInfo> GetTableInfoList()
        {
            return _dbMaintenance.GetTableInfoList();
        }

        /// <summary>
        /// DbFirst
        /// </summary>
        /// <returns></returns>
        public IDbFirst DbFirst
        {
            get { return _dbMaintenanceDbFirst; }
        }
    }
}
