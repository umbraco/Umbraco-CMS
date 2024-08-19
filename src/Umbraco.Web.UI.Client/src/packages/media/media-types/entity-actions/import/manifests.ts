import { UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE } from '../../entity.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as modalManifests } from './modal/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.MediaType.Import',
		name: 'Export Media Type Entity Action',
		forEntityTypes: [UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE],
		api: () => import('./media-type-import.action.js'),
		meta: {
			icon: 'icon-page-up',
			label: '#actions_import',
		},
	},
];

export const manifests: Array<ManifestTypes> = [...entityActions, ...repositoryManifests, ...modalManifests];
