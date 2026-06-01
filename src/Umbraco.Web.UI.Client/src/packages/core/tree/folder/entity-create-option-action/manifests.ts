import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UmbFolderCreateOptionAction } from './folder-entity-create-option-action.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.EntityCreateOptionAction.Folder.Create',
		matchKind: 'folder',
		matchType: 'entityCreateOptionAction',
		manifest: {
			type: 'entityCreateOptionAction',
			kind: 'folder',
			api: UmbFolderCreateOptionAction,
			weight: 1,
			forEntityTypes: [],
			meta: {
				icon: 'icon-folder',
				label: '#create_folder',
				additionalOptions: true,
			},
		},
	},
];
