using System;
using System.IO;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;


namespace DeleteDoors
{
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]

    public class DeleteDoorsApp : IExternalCommand
    {


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var app = commandData.Application.Application;
            var doc = commandData.Application.ActiveUIDocument?.Document;

            DeleteAllDoors(app, doc);
            return Result.Succeeded;
        }

        public static void DeleteElement(Document document, Element element)
        {
            // Delete a selected element
            ICollection<Autodesk.Revit.DB.ElementId> deletedIdSet = document.Delete(element.Id);


            if (0 == deletedIdSet.Count)
            {
                throw new Exception("Deleting the selected element in Revit failed.");
            }


            String prompt = "The selected element has been removed and ";
            prompt += deletedIdSet.Count - 1;
            prompt += " more dependent elements have also been removed.";


            // Give the user some information
            TaskDialog.Show("Revit", prompt);


        }

        public static void DeleteAllDoors(Application rvtApp, Document doc)
        {
            if (rvtApp == null) throw new InvalidDataException(nameof(rvtApp));


            if (doc == null) throw new InvalidOperationException("Could not open document.");


            IList<Element> listOfDoors = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Doors).WhereElementIsNotElementType().ToElements();
            int taille = listOfDoors.Count;


            if (taille == 0)
            {


                TaskDialog.Show("Dialog", "There isn't any door in the project");
            }

            else
            {
                using (Transaction tr = new Transaction(doc))
                {


                    tr.Start("do");
                    for (int i = 0; i < taille; i++)
                    {

                        DeleteElement(doc, listOfDoors[i]);
                    }


                    tr.Commit();


                }
            }
        }
    }
}
