import type { UmbPropertyEditorConfigCollection } from "@umbraco-cms/backoffice/property-editor";

export interface UmbPropertyEditorUiElement extends HTMLElement {
	value: unknown;
	config?: UmbPropertyEditorConfigCollection;
}
