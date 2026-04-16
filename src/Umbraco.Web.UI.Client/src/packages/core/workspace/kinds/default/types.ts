import type { ManifestWorkspace, MetaWorkspace } from '../../extensions/types.js';
import type { UmbDefaultWorkspaceContext } from './default-workspace.context.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export interface ManifestWorkspaceDefaultKind
	extends ManifestWorkspace<MetaWorkspaceDefaultKind, UmbControllerHostElement, UmbDefaultWorkspaceContext> {
	type: 'workspace';
	kind: 'default';
}

export interface MetaWorkspaceDefaultKind extends MetaWorkspace {
	headline: string;
	/**
	 * Optional icon for this workspace (e.g. `icon-globe`).
	 * Surfaced by consumers like the user history list.
	 */
	icon?: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestWorkspaceDefaultKind: ManifestWorkspaceDefaultKind;
	}
}
