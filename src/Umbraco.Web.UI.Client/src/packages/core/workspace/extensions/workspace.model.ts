import type { UmbRoutableWorkspaceContext } from '../contexts/tokens/routable-workspace-context.interface.js';
import type { UmbWorkspaceContext } from '../workspace-context.interface.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestElementAndApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestWorkspace<
	MetaType extends MetaWorkspace = MetaWorkspace,
	ElementType extends UmbControllerHostElement = UmbControllerHostElement,
	ApiType extends UmbWorkspaceContext = UmbWorkspaceContext,
> extends ManifestElementAndApi<ElementType, ApiType> {
	type: 'workspace';
	meta: MetaType;
}

export interface MetaWorkspace {
	entityType: string;
}

export interface ManifestWorkspaceRoutableKind
	extends ManifestWorkspace<MetaWorkspaceRoutableKind, UmbControllerHostElement, UmbRoutableWorkspaceContext> {
	type: 'workspace';
	kind: 'routable';
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaWorkspaceRoutableKind extends MetaWorkspace {}

declare global {
	interface UmbExtensionManifestMap {
		ManifestWorkspace: ManifestWorkspace;
		ManifestWorkspaceRoutableKind: ManifestWorkspaceRoutableKind;
	}
}
