import { DICTIONARY_REPOSITORY_ALIAS } from '../repository/manifests.js';
import UmbReloadDictionaryEntityAction from './reload.action.js';
import UmbImportDictionaryEntityAction from './import/import.action.js';
import UmbExportDictionaryEntityAction from './export/export.action.js';
import UmbCreateDictionaryEntityAction from './create/create.action.js';
import { UmbDeleteEntityAction, UmbMoveEntityAction } from '@umbraco-cms/backoffice/entity-action';
import type { ManifestEntityAction, ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const entityType = 'dictionary-item';
const repositoryAlias = DICTIONARY_REPOSITORY_ALIAS;

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Dictionary.Create',
		name: 'Create Dictionary Entity Action',
		weight: 600,
		meta: {
			icon: 'umb:add',
			label: 'Create',
			repositoryAlias,
			api: UmbCreateDictionaryEntityAction,
		},
		conditions: {
			entityTypes: [entityType],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Dictionary.Move',
		name: 'Move Dictionary Entity Action',
		weight: 500,
		meta: {
			icon: 'umb:enter',
			label: 'Move',
			repositoryAlias,
			api: UmbMoveEntityAction,
		},
		conditions: {
			entityTypes: [entityType],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Dictionary.Export',
		name: 'Export Dictionary Entity Action',
		weight: 400,
		meta: {
			icon: 'umb:download-alt',
			label: 'Export',
			repositoryAlias,
			api: UmbExportDictionaryEntityAction,
		},
		conditions: {
			entityTypes: [entityType],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Dictionary.Import',
		name: 'Import Dictionary Entity Action',
		weight: 300,
		meta: {
			icon: 'umb:page-up',
			label: 'Import',
			repositoryAlias,
			api: UmbImportDictionaryEntityAction,
		},
		conditions: {
			entityTypes: [entityType],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Dictionary.Reload',
		name: 'Reload Dictionary Entity Action',
		weight: 200,
		meta: {
			icon: 'umb:refresh',
			label: 'Reload',
			repositoryAlias,
			api: UmbReloadDictionaryEntityAction,
		},
		conditions: {
			entityTypes: [entityType],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Dictionary.Delete',
		name: 'Delete Dictionary Entity Action',
		weight: 100,
		meta: {
			icon: 'umb:trash',
			label: 'Delete',
			repositoryAlias,
			api: UmbDeleteEntityAction,
		},
		conditions: {
			entityTypes: [entityType],
		},
	},
];

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.CreateDictionary',
		name: 'Create Dictionary Modal',
		loader: () => import('./create/create-dictionary-modal-layout.element'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.ExportDictionary',
		name: 'Export Dictionary Modal',
		loader: () => import('./export/export-dictionary-modal.element'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.ImportDictionary',
		name: 'Import Dictionary Modal',
		loader: () => import('./import/import-dictionary-modal.element'),
	},
];

export const manifests = [...entityActions, ...modals];
