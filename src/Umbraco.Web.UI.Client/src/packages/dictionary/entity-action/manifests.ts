import { UMB_DICTIONARY_ENTITY_TYPE, UMB_DICTIONARY_ROOT_ENTITY_TYPE } from '../entity.js';
import {
	UMB_DICTIONARY_DETAIL_REPOSITORY_ALIAS,
	UMB_DICTIONARY_EXPORT_REPOSITORY_ALIAS,
	UMB_DICTIONARY_IMPORT_REPOSITORY_ALIAS,
	UMB_DICTIONARY_ITEM_REPOSITORY_ALIAS,
} from '../repository/index.js';
import UmbImportDictionaryEntityAction from './import/import.action.js';
import UmbExportDictionaryEntityAction from './export/export.action.js';
import UmbCreateDictionaryEntityAction from './create/create.action.js';
import { UmbMoveEntityAction } from '@umbraco-cms/backoffice/entity-action';
import type { ManifestModal, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Dictionary.Create',
		name: 'Create Dictionary Entity Action',
		weight: 600,
		api: UmbCreateDictionaryEntityAction,
		meta: {
			icon: 'icon-add',
			label: 'Create',
			repositoryAlias: UMB_DICTIONARY_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [UMB_DICTIONARY_ENTITY_TYPE, UMB_DICTIONARY_ROOT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Dictionary.Move',
		name: 'Move Dictionary Entity Action',
		weight: 500,
		api: UmbMoveEntityAction,
		meta: {
			icon: 'icon-enter',
			label: 'Move',
			repositoryAlias: UMB_DICTIONARY_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [UMB_DICTIONARY_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Dictionary.Export',
		name: 'Export Dictionary Entity Action',
		weight: 400,
		api: UmbExportDictionaryEntityAction,
		meta: {
			icon: 'icon-download-alt',
			label: 'Export',
			repositoryAlias: UMB_DICTIONARY_EXPORT_REPOSITORY_ALIAS,
			entityTypes: [UMB_DICTIONARY_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Dictionary.Import',
		name: 'Import Dictionary Entity Action',
		weight: 300,
		api: UmbImportDictionaryEntityAction,
		meta: {
			icon: 'icon-page-up',
			label: 'Import',
			repositoryAlias: UMB_DICTIONARY_IMPORT_REPOSITORY_ALIAS,
			entityTypes: [UMB_DICTIONARY_ENTITY_TYPE, UMB_DICTIONARY_ROOT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Dictionary.Delete',
		name: 'Delete Dictionary Entity Action',
		kind: 'delete',
		meta: {
			itemRepositoryAlias: UMB_DICTIONARY_ITEM_REPOSITORY_ALIAS,
			detailRepositoryAlias: UMB_DICTIONARY_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [UMB_DICTIONARY_ENTITY_TYPE],
		},
	},
];

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Dictionary.Create',
		name: 'Create Dictionary Modal',
		js: () => import('./create/create-dictionary-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.Dictionary.Export',
		name: 'Export Dictionary Modal',
		js: () => import('./export/export-dictionary-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.Dictionary.Import',
		name: 'Import Dictionary Modal',
		js: () => import('./import/import-dictionary-modal.element.js'),
	},
];

export const manifests = [...entityActions, ...modals];
