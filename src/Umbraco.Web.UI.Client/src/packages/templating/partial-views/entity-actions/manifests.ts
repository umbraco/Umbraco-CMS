import { UMB_PARTIAL_VIEW_ENTITY_TYPE } from '../entity.js';
import { UMB_PARTIAL_VIEW_REPOSITORY_ALIAS } from '../repository/index.js';
import { manifests as createManifests } from './create/manifests.js';
import { manifests as renameManifests } from './rename/manifests.js';
import { UmbDeleteEntityAction } from '@umbraco-cms/backoffice/entity-action';
import { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

const partialViewActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.PartialView.Delete',
		name: 'Delete PartialView Entity Action',
		api: UmbDeleteEntityAction,
		meta: {
			icon: 'icon-trash',
			label: 'Delete...',
			repositoryAlias: UMB_PARTIAL_VIEW_REPOSITORY_ALIAS,
			entityTypes: [UMB_PARTIAL_VIEW_ENTITY_TYPE],
		},
	},
];

export const manifests = [...partialViewActions, ...createManifests, ...renameManifests];
