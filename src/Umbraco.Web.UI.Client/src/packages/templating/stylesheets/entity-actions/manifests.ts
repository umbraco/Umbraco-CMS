import { UMB_STYLESHEET_ENTITY_TYPE } from '../entity.js';
import { UMB_STYLESHEET_DETAIL_REPOSITORY_ALIAS } from '../repository/index.js';
import { manifests as createManifests } from './create/manifests.js';
import { UmbDeleteEntityAction, UmbRenameEntityAction } from '@umbraco-cms/backoffice/entity-action';
import { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

const stylesheetActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Stylesheet.Rename',
		name: 'Rename Stylesheet Entity Action',
		api: UmbRenameEntityAction,
		weight: 200,
		meta: {
			icon: 'icon-edit',
			label: 'Rename...',
			repositoryAlias: UMB_STYLESHEET_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [UMB_STYLESHEET_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Stylesheet.Delete',
		name: 'Delete Stylesheet Entity Action',
		api: UmbDeleteEntityAction,
		weight: 100,
		meta: {
			icon: 'icon-trash',
			label: 'Delete...',
			repositoryAlias: UMB_STYLESHEET_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [UMB_STYLESHEET_ENTITY_TYPE],
		},
	},
];

export const manifests = [...stylesheetActions, ...createManifests];
