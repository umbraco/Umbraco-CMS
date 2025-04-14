import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbServerFilePathUniqueSerializer } from '@umbraco-cms/backoffice/server-file-system';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbStylesheetInputElement } from '../../global-components/index.js';

@customElement('umb-property-editor-ui-stylesheet-picker')
export class UmbPropertyEditorUIStylesheetPickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	readonly #serverFilePathUniqueSerializer = new UmbServerFilePathUniqueSerializer();

	@property({ type: Array })
	public set value(value: Array<string>) {
		if (!value) return;
		this.#value = value.map((unique) => this.#serverFilePathUniqueSerializer.toUnique(unique));
	}
	public get value(): Array<string> {
		if (!this.#value) return [];
		return this.#value.map((unique) => this.#serverFilePathUniqueSerializer.toServerPath(unique)) as string[];
	}
	#value: Array<string> = [];

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	#onChange(event: CustomEvent) {
		const target = event.target as UmbStylesheetInputElement;
		this.#value = target.selection ?? [];
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`<umb-stylesheet-input @change=${this.#onChange} .selection=${this.#value}></umb-stylesheet-input>`;
	}
}

export default UmbPropertyEditorUIStylesheetPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-stylesheet-picker': UmbPropertyEditorUIStylesheetPickerElement;
	}
}
