import { UMB_MEDIA_DETAIL_REPOSITORY_ALIAS } from '../repository/index.js';
import { manifests as createManifests } from './create/manifests.js';
import { UmbDeleteEntityAction } from '@umbraco-cms/backoffice/entity-action';

import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	...createManifests,
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Media.Delete',
		name: 'Delete Media Entity Action ',
		api: UmbDeleteEntityAction,
		meta: {
			icon: 'icon-delete',
			label: 'Delete',
			repositoryAlias: UMB_MEDIA_DETAIL_REPOSITORY_ALIAS,
			entityTypes: ['media'],
		},
	},
];

export const manifests = [...entityActions];
