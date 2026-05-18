import { UMB_ELEMENT_FOLDER_WORKSPACE_ALIAS } from '../../folder/workspace/constants.js';
import { UMB_ELEMENT_RECYCLE_BIN_COLLECTION_ALIAS } from '../collection/constants.js';
import { UMB_ENTITY_IS_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import type { ManifestWorkspaceViewCollectionKind } from '@umbraco-cms/backoffice/collection';

const workspaceView: ManifestWorkspaceViewCollectionKind = {
	type: 'workspaceView',
	kind: 'collection',
	alias: 'Umb.WorkspaceView.ElementRecycleBin.Folder',
	name: 'Element Recycle Bin Root Collection Workspace View',
	meta: {
		label: 'Collection',
		pathname: 'collection',
		icon: 'icon-layers',
		collectionAlias: UMB_ELEMENT_RECYCLE_BIN_COLLECTION_ALIAS,
	},
	conditions: [
		{
			alias: UMB_WORKSPACE_CONDITION_ALIAS,
			match: UMB_ELEMENT_FOLDER_WORKSPACE_ALIAS,
		},
		{
			alias: UMB_ENTITY_IS_TRASHED_CONDITION_ALIAS,
		},
	],
};

export const manifests: Array<UmbExtensionManifest> = [workspaceView];
