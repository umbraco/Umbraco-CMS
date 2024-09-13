import { UMB_MEMBER_ROOT_ENTITY_TYPE } from '../../entity.js';
import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [
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
