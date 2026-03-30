import { UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE, UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE } from '../../entity.js';
import { manifests as defaultManifests } from './default/manifests.js';
import { manifests as fromSnippetManifests } from './from-snippet/manifests.js';
import { manifests as folderManifests } from './folder/manifests.js';
import { UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/server';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'entityAction',
		kind: 'create',
		alias: 'Umb.EntityAction.PartialView.Create',
		name: 'Create Partial View Entity Action',
		forEntityTypes: [UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE, UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE],
		conditions: [
			{
				alias: UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS,
				match: false,
			},
		],
	},
	...defaultManifests,
	...fromSnippetManifests,
	...folderManifests,
	{
		type: 'modal',
		alias: 'Umb.Modal.PartialView.CreateFromSnippet',
		name: 'Create Partial View From Snippet Modal',
		js: () => import('./snippet-modal/create-from-snippet-modal.js'),
	},
	// Deprecated: kept for backwards compatibility. Scheduled for removal in Umbraco 19.
	{
		type: 'modal',
		alias: 'Umb.Modal.PartialView.CreateOptions',
		name: 'Partial View Create Options Modal',
		element: () => import('./options-modal/partial-view-create-options-modal.element.js'),
	},
];
