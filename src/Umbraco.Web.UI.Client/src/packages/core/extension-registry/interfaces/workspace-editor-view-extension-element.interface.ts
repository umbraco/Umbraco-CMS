import type { ManifestWorkspaceEditorView } from '../models/index.js';

export interface UmbWorkspaceEditorViewExtensionElement extends HTMLElement {
	manifest?: ManifestWorkspaceEditorView;
}
