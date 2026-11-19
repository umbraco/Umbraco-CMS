import type { ManifestActionGroupBase } from '@umbraco-cms/backoffice/action';

/**
 * A group of workspace action menu items. Workspace action menu items reference it through `group`.
 */
export interface ManifestWorkspaceActionMenuItemGroup extends ManifestActionGroupBase {
	type: 'workspaceActionMenuItemGroup';
}

declare global {
	interface UmbExtensionManifestMap {
		ManifestWorkspaceActionMenuItemGroup: ManifestWorkspaceActionMenuItemGroup;
	}
}
