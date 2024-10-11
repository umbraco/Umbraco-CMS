import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

export interface UmbPropertyEditorUiElement extends HTMLElement {
	alias?: string;
	value?: unknown;
	config?: UmbPropertyEditorConfigCollection;
	mandatory?: boolean;
	mandatoryMessage?: string;
	destroy?: () => void;
}
