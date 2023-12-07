import type { UmbPropertyEditorConfigCollection } from 'src/packages/core/property/property-editor';

export interface UmbPropertyEditorUiElement extends HTMLElement {
	value?: unknown;
	config?: UmbPropertyEditorConfigCollection;
}
