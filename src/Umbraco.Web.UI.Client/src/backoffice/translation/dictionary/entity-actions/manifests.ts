import { DICTIONARY_REPOSITORY_ALIAS } from '../repository/manifests';
import UmbReloadDictionaryEntityAction from './reload.action';
import UmbImportDictionaryEntityAction from './import/import.action';
import UmbExportDictionaryEntityAction from './export/export.action';
import UmbCreateDictionaryEntityAction from './create/create.action';
import { UmbDeleteEntityAction, UmbMoveEntityAction } from '@umbraco-cms/entity-action';
import type { ManifestEntityAction, ManifestModal } from '@umbraco-cms/models';

const entityType = 'dictionary-item';
const repositoryAlias = DICTIONARY_REPOSITORY_ALIAS;

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Dictionary.Create',
		name: 'Create Dictionary Entity Action',
		weight: 600,
		meta: {
			entityType,
			icon: 'umb:add',
			label: 'Create',
			repositoryAlias,
			api: UmbCreateDictionaryEntityAction,
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Dictionary.Move',
		name: 'Move Dictionary Entity Action',
		weight: 500,
		meta: {
			entityType,
			icon: 'umb:enter',
			label: 'Move',
			repositoryAlias,
			api: UmbMoveEntityAction,
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Dictionary.Export',
		name: 'Export Dictionary Entity Action',
		weight: 400,
		meta: {
			entityType,
			icon: 'umb:download-alt',
			label: 'Export',
			repositoryAlias,
			api: UmbExportDictionaryEntityAction,
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Dictionary.Import',
		name: 'Import Dictionary Entity Action',
		weight: 300,
		meta: {
			entityType,
			icon: 'umb:page-up',
			label: 'Import',
			repositoryAlias,
			api: UmbImportDictionaryEntityAction,
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Dictionary.Reload',
		name: 'Reload Dictionary Entity Action',
		weight: 200,
		meta: {
			entityType,
			icon: 'umb:refresh',
			label: 'Reload',
			repositoryAlias,
			api: UmbReloadDictionaryEntityAction,
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Dictionary.Delete',
		name: 'Delete Dictionary Entity Action',
		weight: 100,
		meta: {
			entityType,
			icon: 'umb:trash',
			label: 'Delete',
			repositoryAlias,
			api: UmbDeleteEntityAction,
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
		loader: () => import('./export/export-dictionary-modal-layout.element'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.ImportDictionary',
		name: 'Import Dictionary Modal',
		loader: () => import('./import/import-dictionary-modal-layout.element'),
	},
];

export const manifests = [...entityActions, ...modals];
