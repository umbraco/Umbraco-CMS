import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_USER_PERMISSION_DOCUMENT_DUPLICATE } from '../../user-permissions/constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as modalManifests } from './modal/manifests.js';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		kind: 'duplicateTo',
		alias: 'Umb.EntityAction.Document.DuplicateTo',
		name: 'Duplicate Document To Entity Action',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		api: () => import('./duplicate-document.action.js'),
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_DUPLICATE],
			},
		],
	},
	...repositoryManifests,
	...modalManifests,
];
