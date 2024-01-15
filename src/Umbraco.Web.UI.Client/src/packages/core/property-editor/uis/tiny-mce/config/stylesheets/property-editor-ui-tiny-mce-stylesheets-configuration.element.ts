import { UmbServerFilePathUniqueSerializer } from '@umbraco-cms/backoffice/server-file-system';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import {
	UmbPropertyEditorConfigCollection,
	UmbPropertyValueChangeEvent,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbStylesheetInputElement } from '@umbraco-cms/backoffice/stylesheet';

/**
 * @element umb-property-editor-ui-tiny-mce-stylesheets-configuration
 */
@customElement('umb-property-editor-ui-tiny-mce-stylesheets-configuration')
export class UmbPropertyEditorUITinyMceStylesheetsConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	private _value: Array<string> = [];
	@property({ type: Array })
	public get value(): Array<string> {
		if (!this._value) return [];
		return this._value.map((unique) => this.#serverFilePathUniqueSerializer.toServerPath(unique)) as string[];
	}
	public set value(value: Array<string>) {
		if (!value) return;
		this._value = value.map((unique) => this.#serverFilePathUniqueSerializer.toUnique(unique));
	}

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	#serverFilePathUniqueSerializer = new UmbServerFilePathUniqueSerializer();

	#onChange(event: CustomEvent) {
		const target = event.target as UmbStylesheetInputElement;
		this._value = target.selectedIds;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`<umb-stylesheet-input @change=${this.#onChange} .selectedIds=${this._value}></umb-stylesheet-input>`;
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbPropertyEditorUITinyMceStylesheetsConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiny-mce-stylesheets-configuration': UmbPropertyEditorUITinyMceStylesheetsConfigurationElement;
	}
}
