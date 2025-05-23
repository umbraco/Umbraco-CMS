import type { UmbPropertyEditorConfigCollection } from '../config/index.js';

export interface UmbPropertyEditorUiElement extends HTMLElement {
	name?: string;
	value?: unknown;
	config?: UmbPropertyEditorConfigCollection;
	readonly?: boolean;
	mandatory?: boolean;
	mandatoryMessage?: string;
	destroy?: () => void;
}
