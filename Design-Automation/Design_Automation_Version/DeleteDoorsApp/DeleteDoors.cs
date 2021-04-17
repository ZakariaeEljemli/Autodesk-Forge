using System;
using System.IO;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using DesignAutomationFramework;

namespace DeleteDoors
{
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class DeleteDoorsApp : IExternalDBApplication
    {
        public ExternalDBApplicationResult OnStartup(Autodesk.Revit.ApplicationServices.ControlledApplication app)
        {
            DesignAutomationBridge.DesignAutomationReadyEvent += HandleDesignAutomationReadyEvent;
            return ExternalDBApplicationResult.Succeeded;
        }
        public void HandleDesignAutomationReadyEvent(object sender, DesignAutomationReadyEventArgs e)
        {
            e.Succeeded = true;
            DeleteAllDoors(e.DesignAutomationData);
        }
        public ExternalDBApplicationResult OnShutdown(Autodesk.Revit.ApplicationServices.ControlledApplication app)
        {
            return ExternalDBApplicationResult.Succeeded;
        }
        public static void DeleteElement(Document document, Element element)
        {
            // Delete a selected element.
            ICollection<Autodesk.Revit.DB.ElementId> deletedIdSet = document.Delete(element.Id);

            if (0 == deletedIdSet.Count)
            {
                throw new Exception("Deleting the selected element in Revit failed.");
            }

            String prompt = "The selected element has been removed and ";
            prompt += deletedIdSet.Count - 1;
            prompt += " more dependent elements have also been removed.";

            // Give the user some information
            Console.WriteLine(prompt);

        }
        public static void DeleteAllDoors(DesignAutomationData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            Application rvtApp = data.RevitApp;
            if (rvtApp == null) throw new InvalidDataException(nameof(rvtApp));

            string modelPath = data.FilePath;
            if (String.IsNullOrWhiteSpace(modelPath)) throw new InvalidDataException(nameof(modelPath));

            Document doc = data.RevitDoc;
            if (doc == null) throw new InvalidOperationException("Could not open document.");

            IList<Element> listOfDoors = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Doors).WhereElementIsNotElementType().ToElements();
            int taille = listOfDoors.Count;

            using (Transaction tr = new Transaction(doc))
                {

                    tr.Start("do");
                    for (int i = 0; i < taille; i++)
                    {
                        DeleteElement(doc, listOfDoors[i]);
                    }

                    tr.Commit();

                }
            ModelPath path = ModelPathUtils.ConvertUserVisiblePathToModelPath("DeleteDoorsProjectResult.rvt");
            SaveAsOptions SAO = new SaveAsOptions();
            SAO.OverwriteExistingFile = false;

            //Save the project file with updated window's parameters
            LogTrace("Saving file...");
            doc.SaveAs(path, SAO);

        }
        private static void LogTrace(string format, params object[] args) { System.Console.WriteLine(format, args); }
    }
}
