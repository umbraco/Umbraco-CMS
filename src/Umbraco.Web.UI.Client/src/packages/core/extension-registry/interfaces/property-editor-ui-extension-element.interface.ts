import type { UmbPropertyEditorConfigCollection } from "@umbraco-cms/backoffice/property-editor";

export interface UmbPropertyEditorExtensionElement extends HTMLElement {
	value: unknown;
	config?: UmbPropertyEditorConfigCollection;
}
