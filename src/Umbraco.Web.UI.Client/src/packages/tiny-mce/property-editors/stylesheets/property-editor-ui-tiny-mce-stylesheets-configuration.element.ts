import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import { UmbServerFilePathUniqueSerializer } from '@umbraco-cms/backoffice/server-file-system';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import type { UmbStylesheetInputElement } from '@umbraco-cms/backoffice/stylesheet';

/**
 * @element umb-property-editor-ui-tiny-mce-stylesheets-configuration
 */
@customElement('umb-property-editor-ui-tiny-mce-stylesheets-configuration')
export class UmbPropertyEditorUITinyMceStylesheetsConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
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
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		return html`<umb-stylesheet-input @change=${this.#onChange} .selection=${this.#value}></umb-stylesheet-input>`;
	}
}

export default UmbPropertyEditorUITinyMceStylesheetsConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiny-mce-stylesheets-configuration': UmbPropertyEditorUITinyMceStylesheetsConfigurationElement;
	}
}
