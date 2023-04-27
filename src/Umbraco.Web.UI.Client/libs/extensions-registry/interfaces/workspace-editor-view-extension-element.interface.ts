import type { ManifestWorkspaceView } from '../models';

export interface UmbWorkspaceEditorViewExtensionElement extends HTMLElement {
	manifest?: ManifestWorkspaceView;
}
