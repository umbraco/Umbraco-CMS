import { UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE, UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UmbPartialViewCreateOptionsEntityAction } from './create.action.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.PartialView.CreateOptions',
		name: 'Partial View Create Options Entity Action',
		weight: 1000,
		api: UmbPartialViewCreateOptionsEntityAction,
		forEntityTypes: [UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE, UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: '#actions_create',
		},
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.PartialView.CreateOptions',
		name: 'Partial View Create Options Modal',
		js: () => import('./options-modal/partial-view-create-options-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.PartialView.CreateFromSnippet',
		name: 'Create Partial View From Snippet Modal',
		js: () => import('./snippet-modal/create-from-snippet-modal.js'),
	},
];
