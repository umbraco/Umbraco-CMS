import { UMB_MEMBER_ROOT_ENTITY_TYPE } from '../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		alias: 'Umb.Workspace.MemberRoot',
		name: 'Member Root Workspace View',
		element: () => import('./member-root-workspace.element.js'),
		meta: {
			entityType: UMB_MEMBER_ROOT_ENTITY_TYPE,
		},
	},
];
