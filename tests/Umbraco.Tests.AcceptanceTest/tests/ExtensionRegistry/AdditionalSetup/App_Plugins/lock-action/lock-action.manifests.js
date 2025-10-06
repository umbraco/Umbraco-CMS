import { UMB_DOCUMENT_ENTITY_TYPE } from '@umbraco-cms/backoffice/document';

export const manifests = [
  {
    type: 'entityAction',
    kind: 'default',
    alias: 'Umb.EntityAction.Document.Lock',
    name: 'Lock Document Entity Action',
    forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
    api: () => import('./lock-action.api.js'),
    weight: 200,
    meta: {
      label: 'Lock it',
      icon: 'icon-lock',
    }
  }
]