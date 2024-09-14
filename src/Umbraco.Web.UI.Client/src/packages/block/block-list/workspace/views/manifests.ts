import { UMB_BLOCK_LIST_TYPE_WORKSPACE_ALIAS } from '../index.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.BlockType.List.Settings',
		name: 'Block List Type Workspace Settings View',
		js: () => import('./block-list-type-workspace-view.element.js'),
		weight: 1000,
		meta: {
			label: '#blockEditor_tabBlockSettings',
			pathname: 'settings',
			icon: 'icon-settings',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_BLOCK_LIST_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
];
