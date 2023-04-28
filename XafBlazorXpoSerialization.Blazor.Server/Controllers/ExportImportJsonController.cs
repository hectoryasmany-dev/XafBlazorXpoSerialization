using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Blazor;
using DevExpress.Persistent.Base;
using DevExpress.Xpo.Helpers;
using Microsoft.JSInterop;
using System.Text.Json;
using System.Text;
using XafBlazorXpoSerialization.Module.BusinessObjects;
using DevExpress.Xpo.Metadata;

namespace XafBlazorXpoSerialization.Blazor.Server.Controllers
{
    public class ExportImportJsonController : ViewController
    {
        public SimpleAction ExportSimpleAction { get; }
        public ExportImportJsonController()
        {
            TargetViewType = ViewType.ListView;
            TargetObjectType = typeof(Client);
            ExportSimpleAction = new SimpleAction(this, "acBlazorExportToJson", PredefinedCategory.Edit)
            {
                Caption = $"Export JSON all",
                TargetViewNesting = Nesting.Root,
                PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.CaptionAndImage,
                SelectionDependencyType = SelectionDependencyType.Independent,
                ImageName = "Open2"


            };
            ExportSimpleAction.Execute += ExportSimpleAction_Execute;
        }
        private async void ExportSimpleAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            try
            {
                XPDictionary dictionary = new ReflectionDictionary();
                dictionary.GetDataStoreSchema(typeof(Invoice), typeof(Client));
                var listViewCollection = (View as ListView).CollectionSource;
                if (listViewCollection.List.Count > 0)
                {
                    var JSRuntime = (Application as BlazorApplication).ServiceProvider.GetService<IJSRuntime>();
                    
                        var serializerOptions = new JsonSerializerOptions
                        {
                            //WriteIndented = true,
                            //DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                            Converters = { new PersistentBaseConverterFactory(((BlazorApplication)Application).ServiceProvider), new XpoModelJsonConverter<Client>(dictionary),
                                new ChangesSetJsonConverterFactory(null) },
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        };


                        var json = System.Text.Json.JsonSerializer.Serialize(listViewCollection.List, serializerOptions);
                        byte[] data = Encoding.UTF8.GetBytes(json);
                        MemoryStream stream = new MemoryStream(data);
                        string fileName = $"{View.Caption} List.json";
                        await JSRuntime.InvokeAsync<object>("saveAsFileJson", fileName, Convert.ToBase64String(stream.ToArray()));

                   




                }
                else
                {
                    throw new UserFriendlyException("No records to export");

                }

            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }
    }
}
