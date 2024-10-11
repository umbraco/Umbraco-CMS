import { UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE, UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '../../../entity.js';
import { UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE } from '../../folder/index.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DocumentType.Tree.ReloadChildrenOf',
		name: 'Reload Document Type Tree Item Children Entity Action',
		kind: 'reloadTreeItemChildren',
		forEntityTypes: [
			UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE,
			UMB_DOCUMENT_TYPE_ENTITY_TYPE,
			UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE,
		],
	},
];
