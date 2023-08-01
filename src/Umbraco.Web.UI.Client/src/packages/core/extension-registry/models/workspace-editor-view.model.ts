import { ConditionTypes } from '../conditions/types.js';
import type { UmbWorkspaceEditorViewExtensionElement } from '../interfaces/workspace-editor-view-extension-element.interface.js';
import type { ManifestWithDynamicConditions, ManifestWithView } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestWorkspaceEditorView
	extends ManifestWithView<UmbWorkspaceEditorViewExtensionElement>,
		ManifestWithDynamicConditions<ConditionTypes> {
	type: 'workspaceEditorView';
}
