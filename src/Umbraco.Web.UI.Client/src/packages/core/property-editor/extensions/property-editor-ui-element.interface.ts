import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

export interface UmbPropertyEditorUiElement extends HTMLElement {
	name?: string;
	value?: unknown;
	config?: UmbPropertyEditorConfigCollection;
	readonly?: boolean;
	mandatory?: boolean;
	mandatoryMessage?: string;
	destroy?: () => void;
}
