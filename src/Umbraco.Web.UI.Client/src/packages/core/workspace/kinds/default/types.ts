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
}

declare global {
	interface UmbExtensionManifestMap {
		umbManifestWorkspaceDefaultKind: ManifestWorkspaceDefaultKind;
	}
}
