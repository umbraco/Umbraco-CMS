export const manifests = [
  {
    type: 'propertyAction',
    kind: 'default',
    alias: 'My.propertyAction.Write',
    name: 'Write Property Action ',
    forPropertyEditorUis: ["Umb.PropertyEditorUi.TextBox"],
    api: () => import('./write-property-action.api.js'),
    weight: 200,
    meta: {
      icon: 'icon-brush',
      label: 'Write text',
    }
  },
  {
    type: 'propertyAction',
    kind: 'default',
    alias: 'My.propertyAction.Read',
    name: 'Read Property Action ',
    forPropertyEditorUis: ["Umb.PropertyEditorUi.TextBox"],
    api: () => import('./read-property-action.api.js'),
    weight: 200,
    meta: {
      icon: 'icon-eye',
      label: 'Read text',
    }
  }
]
