import { ManifestWorkspaceView } from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspaceView = {
  type: 'workspaceView',
  alias: 'Example.WorkspaceView.EntityContentTypeCondition',
  name: "Example Workspace View With Entity Content Type Condition",
  element : () => import('./workspace-view.element.js'),
  meta: {
    icon : 'icon-bus',
    label : 'Conditional',
    pathname : 'conditional'
  },
  conditions : [
    {
      alias : 'Umb.Condition.EntityContentType',
      oneOf : ['29643452-cff9-47f2-98cd-7de4b6807681','media-type-1-id']
    }
  ]
};

export const manifests = [
  workspace
]