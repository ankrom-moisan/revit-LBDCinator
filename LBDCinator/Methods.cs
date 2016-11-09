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
    class Methods
    {
        // Useful methods can go here.

        ////////////////   Pick elements by rectangle with filter   /////////////////////////////////////
        public IList<Element> PickRectangle(UIDocument uiDoc, ISelectionFilter selFilter, string message)
        {
            // Change PickElementsByRectangle to PickObject for single select
            IList<Element> list = uiDoc.Selection.PickElementsByRectangle(selFilter, message);
            return list;
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////

        //////   Custom seeking container for any object. Uses FilteredElementCollector   //////////
        public IEnumerable<T> GetElements<T>(Document document) where T : Element
        {
            FilteredElementCollector collector = new FilteredElementCollector(document);
            collector.OfClass(typeof(T));
            return collector.Cast<T>();

            // In code ex:     <T> name = GetElements<T>(doc)
            //                      .Where (i => i.Name == "whatever");
        }
        ///////////////////////////////////////////////////////////////////////////////////////////

        ////////////////   Place a named element in the document   ////////////////////////////////
        public void PlaceNamedElement(UIDocument uiDoc, string name)
        {
            FamilySymbol symbol = GetElements<FamilySymbol>(uiDoc.Document)
                          .Where(i => i.Name == name)
                          .First();
            uiDoc.PromptForFamilyInstancePlacement(symbol);
        }
        ///////////////////////////////////////////////////////////////////////////////////////////

        ///////////////   Turn list of ElementIds to list of Elements   ///////////////////////////
        public IList<Element> ids2Elements(Document doc, IList<ElementId> idList)
        {
            IList<Element> elementList = new List<Element>();
            foreach (ElementId eId in idList)
            {
                Element fromId = doc.GetElement(eId);
                elementList.Add(fromId);
            }
            return elementList;
        }
        //////////////////////////////////////////////////////////////////////////////////////////

        ///////////////////   Get collection of elements by name   ///////////////////////////////
        public FilteredElementCollector GetElementofNameType(Document doc, string name)
        {
            FilteredElementCollector a
                = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance));

            BuiltInParameter bip
                = BuiltInParameter.ALL_MODEL_TYPE_NAME;

            ParameterValueProvider provider
                = new ParameterValueProvider(
                    new ElementId(bip));

            FilterStringRuleEvaluator evaluator
                = new FilterStringEquals();

            FilterRule rule = new FilterStringRule(
              provider, evaluator, name, true);

            ElementParameterFilter filter
              = new ElementParameterFilter(rule);

            return a.WherePasses(filter);
        }
        /////////////////////////////////////////////////////////////////////////////////////////

        //////////////////   Fire a reference intersector with filters   ////////////////////////
        public IList<ElementId> Fire(Document doc, LocationPoint pXYZ)
        {
            IList<ElementId> returnId = new List<ElementId>();

            //limit depth of ray cast
            double limit = 10;

            //setup parameters for raycast
            View3D view = Get3DView(doc);
            XYZ direction = new XYZ(0, 0, -1);
            XYZ origin = pXYZ.Point;

            ElementCategoryFilter roofFilter = new ElementCategoryFilter(BuiltInCategory.OST_Roofs);
            ElementCategoryFilter floorFilter = new ElementCategoryFilter(BuiltInCategory.OST_Floors);
            LogicalOrFilter orFilter = new LogicalOrFilter(roofFilter, floorFilter);

            //cast ray, found elements are stored in array as References with proximity data attached
            ReferenceIntersector refIntersector = new ReferenceIntersector(orFilter, FindReferenceTarget.Element, view);
            IList<ReferenceWithContext> referenceWithContext = refIntersector.Find(origin, direction).Where(w => w.Proximity < limit).OrderBy(o => o.Proximity).ToList();

            //iterate through array, store Elements with proximity < limit to ElementId array
            foreach (ReferenceWithContext rC in referenceWithContext)
            {
                returnId.Add(doc.GetElement(rC.GetReference()).GetTypeId());
            }
            if (returnId.Count < 1)
            {
                return null;
            }
            return returnId;
        }
        /////////////////////////////////////////////////////////////////////////////////////////

        /////////////////   Get the canonical 3D View   /////////////////////////////////////////
        public View3D Get3DView(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(View3D));

            foreach (View3D v in collector)
            {
                //skip templates here because
                //they are invisible in browser:

                if (v != null && !v.IsTemplate && v.Name == "{3D}")
                {
                    return v;
                }
            }
            return null;
        }
        /////////////////////////////////////////////////////////////////////////////////////////
    }

    //////////////////   Selection Filter for IList<>   /////////////////////////////////////////
    public class WallFilter : ISelectionFilter
    {
        public bool AllowElement(Element wall)
        {
            if (wall is Wall) return true;
            return false;
        }
        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
    }
    /////////////////////////////////////////////////////////////////////////////////////////////
}
