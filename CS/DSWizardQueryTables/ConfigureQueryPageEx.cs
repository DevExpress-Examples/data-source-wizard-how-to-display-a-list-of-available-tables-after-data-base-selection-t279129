﻿using System.Linq;
using DevExpress.DataAccess.Sql;
using DevExpress.DataAccess.Wizard;
using DevExpress.DataAccess.Wizard.Model;
using DevExpress.DataAccess.Wizard.Presenters;
using DevExpress.DataAccess.Wizard.Services;
using DevExpress.DataAccess.Wizard.Views;

namespace DSWizardCustomizationNoSP
{

    public class ConfigureQueryPageEx<TModel> : ConfigureQueryPage<TModel>
        where TModel : ISqlDataSourceModel
    {

        readonly IDBSchemaProvider schemaProvider;
        public ConfigureQueryPageEx(IConfigureQueryPageView view, 
            IWizardRunnerContext context, 
            SqlWizardOptions options,
            IDBSchemaProvider dbSchemaProvider, 
            IParameterService parameterService, 
            ICustomQueryValidator customQueryValidator)
            : base(view, context, options, dbSchemaProvider, parameterService, customQueryValidator) {
                schemaProvider = dbSchemaProvider;
        }

        public override void Begin()
        {
            base.Begin();
            base.LoadTablesAndViews();
            base.LoadStoredProcedures();

            var configureQueryPageViewEx = (ConfigureQueryPageViewEx)View;
            configureQueryPageViewEx.InitializeTables(DBSchema);
            configureQueryPageViewEx.SelectedTableChanged += configureQueryPageViewEx_SelectedTableChanged;

        }
        void configureQueryPageViewEx_SelectedTableChanged(object sender, SelectedTableChanged e)
        {
            var dbTable = DBSchema.Tables.First(t => string.Equals(t.Name, e.TableName));
            schemaProvider.LoadColumns(Model.DataConnection, dbTable);

            var selectQuery = SelectQueryFluentBuilder
                .AddTable(e.TableName)
                .SelectColumns(dbTable.Columns.Select(c => c.Name).ToArray())
                .Build(e.TableName);

            SetTableOrCustomSqlQuery(selectQuery);

            RaiseChanged();
        }

        public override void Commit()
        {
            ((ConfigureQueryPageViewEx)View).SelectedTableChanged -= configureQueryPageViewEx_SelectedTableChanged;
            base.Commit();
        }
    }

}
