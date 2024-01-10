import { UMB_STYLESHEET_ENTITY_TYPE } from '../entity.js';
import { UMB_STYLESHEET_REPOSITORY_ALIAS } from '../repository/index.js';
import { manifests as createManifests } from './create/manifests.js';
import { UmbDeleteEntityAction } from '@umbraco-cms/backoffice/entity-action';
import { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

const stylesheetActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Stylesheet.Delete',
		name: 'Delete Stylesheet Entity Action',
		api: UmbDeleteEntityAction,
		meta: {
			icon: 'icon-trash',
			label: 'Delete',
			repositoryAlias: UMB_STYLESHEET_REPOSITORY_ALIAS,
			entityTypes: [UMB_STYLESHEET_ENTITY_TYPE],
		},
	},
];

export const manifests = [...stylesheetActions, ...createManifests];
