import { UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE } from '../../tree/index.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'create',
		alias: 'Umb.EntityAction.DocumentType.Create',
		name: 'Create Document Type Entity Action',
		weight: 1200,
		forEntityTypes: [UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE, UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: '#actions_create',
			additionalOptions: true,
			headline: '#create_createUnder #treeHeaders_documentTypes',
		},
	},
];
