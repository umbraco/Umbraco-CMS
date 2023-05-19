import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbDataTypePropertyCollection } from '@umbraco-cms/backoffice/data-type';

/**
 * @element umb-property-editor-ui-tiny-mce
 */
@customElement('umb-property-editor-ui-tiny-mce')
export class UmbPropertyEditorUITinyMceElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {


	@property({ type: String })
	value = '';

	configuration: Array<DataTypePropertyPresentationModel> = [];

	@property({ type: Array, attribute: false })
	public config = new UmbDataTypePropertyCollection();
	public set config(config: Array<DataTypePropertyPresentationModel>) {
		this.configuration = config;
	}

	#onChange(event: InputEvent) {
		this.value = (event.target as HTMLInputElement).value;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<umb-input-tiny-mce
			@change=${this.#onChange}
			.configuration=${this.configuration}
			.value=${this.value}></umb-input-tiny-mce>`;
	}

	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUITinyMceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiny-mce': UmbPropertyEditorUITinyMceElement;
	}
}