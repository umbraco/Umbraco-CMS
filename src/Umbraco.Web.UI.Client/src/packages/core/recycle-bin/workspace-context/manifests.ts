import { UmbTrashableEntityWorkspaceController } from './trashable-entity-workspace.controller.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.WorkspaceContext.Trashable',
		matchKind: 'trashable',
		matchType: 'workspaceContext',
		manifest: {
			type: 'workspaceContext',
			kind: 'trashable',
			api: UmbTrashableEntityWorkspaceController,
		},
	},
];
