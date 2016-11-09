using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace LBDCinator
{
    [Transaction(TransactionMode.Manual)]
    class Command : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Methods methods = new Methods();

            //MainWindow mWindow = new MainWindow(doc);
            //mWindow.ShowDialog();

            //pick a detail line
            Reference lineRef = uidoc.Selection.PickObject(
                ObjectType.Element, new LineSelectionFilter(), "Pick a detail line.");
            DetailLine pickedLine = (DetailLine)uidoc.Document.GetElement(lineRef.ElementId);

            FilteredElementCollector lines = new FilteredElementCollector(doc);

            //manual filters
            List<DetailLine> detailLines = new List<DetailLine>();
            lines.OfClass(typeof(CurveElement));
            foreach (Element element in lines)
            {
                if (element is DetailLine)
                {
                    detailLines.Add(element as DetailLine);
                }
                else
                {
                    //do nothing
                }

            }
            List<DetailLine> filteredLines = new List<DetailLine>();
            foreach (DetailLine line in detailLines)
            {
                if (line.LineStyle.Name == pickedLine.LineStyle.Name &&
                    line.GroupId.IntegerValue == -1)
                {
                    filteredLines.Add(line);
                }
                else
                {
                    //do nothing
                }
            }

            //Pick a line based detail component =============(fill in element type)
            Reference componentRef = uidoc.Selection.PickObject(
                ObjectType.Element, new ComponentSelectionFilter(), "Pick a line-based detail component.");
            FamilyInstance pickedComponent = (FamilyInstance)uidoc.Document.GetElement(componentRef.ElementId);

            var componentType = pickedComponent.Symbol;
            var replacementCount = 0;


            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Replace Lines");

                foreach (DetailLine line in filteredLines)
                {
                    var view = doc.GetElement(line.OwnerViewId) as View;

                    LocationCurve curve = line.Location as LocationCurve;
                    //XYZ start = curve.Curve.GetEndPoint(0);
                    //XYZ end = curve.Curve.GetEndPoint(1);
                    doc.Create.NewFamilyInstance(line.GeometryCurve as Line, componentType, view);
                    doc.Delete(line.Id);

                    replacementCount++;
                }


                tx.Commit();
            }

            TaskDialog.Show("Revit Task", "Replaced " + replacementCount + " detail lines.");

            return Result.Succeeded;
        }
    }

    public class LineSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem is DetailLine;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }

    //filters for line based detail components
    //================figure out what that is called in this setting
    public class ComponentSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem is FamilyInstance;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }


}
