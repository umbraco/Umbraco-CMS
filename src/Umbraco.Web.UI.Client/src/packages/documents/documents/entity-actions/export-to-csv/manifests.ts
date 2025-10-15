import { UMB_USER_PERMISSION_DOCUMENT_EXPORT_TO_CSV } from '../../constants.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: 'entityAction',
    kind: 'default',
    alias: 'Umb.EntityAction.Document.ExportToCsv',
    name: 'Export to CSV Entity Action',
    weight: 750, 
	// Position between Publish (600) and other actions
    api: () => import('./export-to-csv.action.js'),
    forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
    meta: {
      icon: 'icon-download',
      label: '#actions_exportToCsv',
      additionalOptions: true,
    },
    conditions: [
      {
        alias: 'Umb.Condition.UserPermission.Document',
        allOf: [UMB_USER_PERMISSION_DOCUMENT_EXPORT_TO_CSV],
      },
      {
        alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
      },
    ],
  },
];