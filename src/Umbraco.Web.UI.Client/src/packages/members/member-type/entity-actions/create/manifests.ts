import { UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE } from '../../entity.js';
import { manifests as defaultManifests } from './default/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'entityAction',
		kind: 'create',
		alias: 'Umb.EntityAction.MemberType.Create',
		name: 'Create Member Type Entity Action',
		forEntityTypes: [UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE],
	},
	...defaultManifests,
];
