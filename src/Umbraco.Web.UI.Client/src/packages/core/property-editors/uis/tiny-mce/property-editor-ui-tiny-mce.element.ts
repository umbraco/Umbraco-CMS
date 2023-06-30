import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbDataTypeConfigCollection } from '@umbraco-cms/backoffice/components';

/**
 * @element umb-property-editor-ui-tiny-mce
 */
@customElement('umb-property-editor-ui-tiny-mce')
export class UmbPropertyEditorUITinyMceElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
	#configuration?: UmbDataTypeConfigCollection;

	@property({ type: String })
	value = '';

	@property({ attribute: false })
	public set config(config: UmbDataTypeConfigCollection | undefined) {
		this.#configuration = config;
	}

	#onChange(event: InputEvent) {
		this.value = (event.target as HTMLInputElement).value;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<umb-input-tiny-mce
			@change=${this.#onChange}
			.configuration=${this.#configuration}
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
