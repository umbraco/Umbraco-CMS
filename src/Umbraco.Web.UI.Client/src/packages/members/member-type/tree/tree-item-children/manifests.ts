import {
	UMB_MEMBER_TYPE_ENTITY_TYPE,
	UMB_MEMBER_TYPE_FOLDER_ENTITY_TYPE,
	UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE,
} from '../../entity.js';
import { manifests as collectionManifests } from './collection/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'reloadTreeItemChildren',
		alias: 'Umb.EntityAction.MemberType.Tree.ReloadChildrenOf',
		name: 'Reload Member Type Tree Item Children Entity Action',
		forEntityTypes: [UMB_MEMBER_TYPE_ENTITY_TYPE, UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE, UMB_MEMBER_TYPE_FOLDER_ENTITY_TYPE],
	},
	...collectionManifests,
];
