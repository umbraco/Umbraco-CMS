import type { UmbPropertyEditorConfigCollection } from '../config/index.js';
import type { ManifestPropertyEditorUi } from './property-editor.extension.js';

export interface UmbPropertyEditorUiElement extends HTMLElement {
	manifest?: ManifestPropertyEditorUi;
	name?: string;
	value?: unknown;
	dataSourceAlias?: string;
	config?: UmbPropertyEditorConfigCollection;
	readonly?: boolean;
	mandatory?: boolean;
	mandatoryMessage?: string;
	destroy?: () => void;
}
