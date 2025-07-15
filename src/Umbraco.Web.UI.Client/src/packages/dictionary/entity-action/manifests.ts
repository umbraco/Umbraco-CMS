import { UMB_DICTIONARY_ENTITY_TYPE, UMB_DICTIONARY_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_DICTIONARY_DETAIL_REPOSITORY_ALIAS, UMB_DICTIONARY_ITEM_REPOSITORY_ALIAS } from '../repository/index.js';
import { manifests as moveManifests } from './move-to/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Dictionary.Create',
		name: 'Create Dictionary Entity Action',
		weight: 1200,
		api: () => import('./create/create.action.js'),
		forEntityTypes: [UMB_DICTIONARY_ENTITY_TYPE, UMB_DICTIONARY_ROOT_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: '#general_create',
			additionalOptions: true,
		},
	},
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Dictionary.Export',
		name: 'Export Dictionary Entity Action',
		weight: 400,
		api: () => import('./export/export.action.js'),
		forEntityTypes: [UMB_DICTIONARY_ENTITY_TYPE],
		meta: {
			icon: 'icon-download-alt',
			label: '#actions_export',
			additionalOptions: true,
		},
	},
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Dictionary.Import',
		name: 'Import Dictionary Entity Action',
		weight: 300,
		api: () => import('./import/import.action.js'),
		forEntityTypes: [UMB_DICTIONARY_ENTITY_TYPE, UMB_DICTIONARY_ROOT_ENTITY_TYPE],
		meta: {
			icon: 'icon-page-up',
			label: '#actions_import',
			additionalOptions: true,
		},
	},
	{
		type: 'entityAction',
		kind: 'delete',
		alias: 'Umb.EntityAction.Dictionary.Delete',
		name: 'Delete Dictionary Entity Action',
		forEntityTypes: [UMB_DICTIONARY_ENTITY_TYPE],
		meta: {
			itemRepositoryAlias: UMB_DICTIONARY_ITEM_REPOSITORY_ALIAS,
			detailRepositoryAlias: UMB_DICTIONARY_DETAIL_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.Dictionary.Export',
		name: 'Export Dictionary Modal',
		element: () => import('./export/export-dictionary-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.Dictionary.Import',
		name: 'Import Dictionary Modal',
		element: () => import('./import/import-dictionary-modal.element.js'),
	},
	...moveManifests,
];
