import { UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE, UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_PARTIAL_VIEW_DETAIL_REPOSITORY_ALIAS } from '../../repository/manifests.js';
import { UmbPartialViewCreateOptionsEntityAction } from './create.action.js';
import { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.PartialView.CreateOptions',
		name: 'Partial View Create Options Entity Action',
		weight: 1000,
		api: UmbPartialViewCreateOptionsEntityAction,
		meta: {
			icon: 'icon-add',
			label: 'Create...',
			repositoryAlias: UMB_PARTIAL_VIEW_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [UMB_PARTIAL_VIEW_ROOT_ENTITY_TYPE, UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE],
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
