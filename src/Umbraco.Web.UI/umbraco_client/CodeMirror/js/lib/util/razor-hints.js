(function() {

    CodeMirror.razorHints['@'] = [
              'inherits',
              'Library',
              'Model',
              'Parameter',
              'using',
              'Dictionary',
              ['if/else', 'if(SomeCondition){\n\n}else{\n\n}\n'],
              ['if', 'if(SomeCondition){\n\n}\n'],
              ['foreach', 'foreach(var item in collection){\n\n}\n'],
              ['context', 'inherits umbraco.MacroEngines.DynamicNodeContext\n\n'],
              ['helper', 'helperMethod(Model)\n\n@helperMethod(dynamic val){\n\t<p>Hello @val.Name\n}\n\n'],
          ];

    CodeMirror.razorHints['.'] = [
              'Ancestors',
              'AncestorsOrSelf',
              'Children',
              'Descendants',
              'DescendantsOrSelf',
              'Parent',
              'First()',
              'Last()',
              'Up()',
              'Next()',
              'Previous()',
              'AncestorOrSelf()',
              'Where()',
              'OrderBy()',
              'GroupBy()',
              'InGroupsOf()',
              'Pluck()',
              'Take()',
              'Skip()',
              'Count()',
              'XPath()',
              'Search()',

              'Id',
              'Template',
              'SortOrder',
              'Name',
              'Visible',
              'Url',
              'UrlName',
              'NodeTypeAlias',
              'WriterName',
              'CreatorName',
              'WriterId',
              'CreatorId',
              'Path',
              'CreateDate',
              'UpdateDate',
              'NiceUrl',
              'Level',
          ];

    CodeMirror.razorHints['@Library.'] = [
            'Search()',
            'NodeById()',
          ];

    CodeMirror.razorHints['@Model.'] =
          CodeMirror.razorHints['<levelRoot><'] =
          CodeMirror.razorHints['<mainLevel><'] = [
              'second',
              'two'
          ];

    CodeMirror.razorHints['<levelTop><second '] = [
            'secondProperty'
          ];

    CodeMirror.razorHints['<levelTop><second><'] = [
            'three',
            'x-three'
          ];

})();