using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.cms.businesslogic.skinning.utils
{
    //will be used to parse a skin manifest during installation
    //in case the skin is using css variables
    public class SkinManifestParser
    {
        public string SkinFilePath;

        public SkinManifestParser()
        {

        }

        public void ProcessSkinFile(string path)
        {
            SkinFilePath = path;

            List<CssVariable> cssVariables = new List<CssVariable>();
            //get all variables


            CssParser parser = new CssParser();
            //add stylesheet contents



            //build collection of properties and selectors for each variable
            // - Var 1  - Property  - Selector
            //                      - Selector
            //                      - Selector
            //          - Property  - Selector
            //                      - Selector
            foreach (var ruleSet in parser.Styles)
            {
                string Selectors = ruleSet.Key;

                foreach (var declaration in ruleSet.Value.Attributes)
                {
                    var property = declaration.Key;
                    var value = declaration.Value;

                    foreach (CssVariable cssvar in cssVariables)
                    {
                        if (value.Contains(cssvar.Name))
                        {
                            //add property 
                            CssVariableProperty prop =
                                cssvar.Properties.Find(delegate(CssVariableProperty p) { return p.Name == property; });

                            if (prop == null)
                                prop = new CssVariableProperty(property);

                            //add selector
                            //maybe split up the Selectors, since they can contain multiple
                            //also 
                            String sel = prop.Selectors.Find(delegate(string s) { return s == Selectors; });

                            if (string.IsNullOrEmpty(sel))
                                prop.Selectors.Add(Selectors);


                            if (!cssvar.Properties.Contains(prop))
                                cssvar.Properties.Add(prop);
                        }
                    }

                }

            }

            //modify tasks that are using a variable

            //create default version of css (replace variables with default value)

            //save skin manifest
        }
    }
}
