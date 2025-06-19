import { UMB_MEDIA_RECYCLE_BIN_ROOT_ENTITY_TYPE } from '../../constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'reloadTreeItemChildren',
		alias: 'Umb.EntityAction.MediaRecycleBin.Tree.ReloadChildrenOf',
		name: 'Reload Media Recycle Bin Tree Item Children Entity Action',
		forEntityTypes: [UMB_MEDIA_RECYCLE_BIN_ROOT_ENTITY_TYPE],
	},
];
