import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { UmbPropertyEditorElement } from '@umbraco-cms/backoffice/property-editor';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { DataTypePropertyPresentationModel } from '@umbraco-cms/backoffice/backend-api';

/**
 * @element umb-property-editor-ui-tiny-mce
 */
@customElement('umb-property-editor-ui-tiny-mce')
export class UmbPropertyEditorUITinyMceElement extends UmbLitElement implements UmbPropertyEditorElement {
	static styles = [UUITextStyles];

	@property({ type: String })
	value = '';

	configuration: Array<DataTypePropertyPresentationModel> = [];

	@property({ type: Array, attribute: false })
	public set config(config: Array<DataTypePropertyPresentationModel>) {
		this.configuration = config;
	}

	#onChange(event: InputEvent) {
		this.value = (event.target as HTMLInputElement).value;
		console.log(this.value);
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<umb-input-tiny-mce
			@change=${this.#onChange}
			.configuration=${this.configuration}
			.value=${this.value}></umb-input-tiny-mce>`;
	}
}

export default UmbPropertyEditorUITinyMceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiny-mce': UmbPropertyEditorUITinyMceElement;
	}
}
