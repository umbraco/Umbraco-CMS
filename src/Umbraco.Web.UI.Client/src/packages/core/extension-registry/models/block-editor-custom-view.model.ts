import type { UmbPropertyEditorUiElement } from '../interfaces/index.js';
import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestBlockEditorCustomView extends ManifestElement<UmbPropertyEditorUiElement> {
	type: 'bockEditorCustomView';
}
