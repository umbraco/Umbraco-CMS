import { UMB_MEDIA_DETAIL_REPOSITORY_ALIAS } from '../repository/index.js';
import { manifests as createManifests } from './create/manifests.js';
import { UmbTrashEntityAction } from '@umbraco-cms/backoffice/entity-action';

import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	...createManifests,
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Media.Trash',
		name: 'Trash Media Entity Action ',
		api: UmbTrashEntityAction,
		meta: {
			icon: 'icon-trash',
			label: 'Trash',
			repositoryAlias: UMB_MEDIA_DETAIL_REPOSITORY_ALIAS,
			entityTypes: ['media'],
		},
	},
];

export const manifests = [...entityActions];
