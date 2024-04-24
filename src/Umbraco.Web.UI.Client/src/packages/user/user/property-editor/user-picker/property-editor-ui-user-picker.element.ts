import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbUserInputElement } from '@umbraco-cms/backoffice/user';

/**
 * @element umb-property-editor-ui-user-picker
 */
@customElement('umb-property-editor-ui-user-picker')
export class UmbPropertyEditorUIUserPickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	value?: string = '';

	@property({ attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	#onChange(event: CustomEvent & { target: UmbUserInputElement }) {
		this.value = event.target.selection.join(',');
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`<umb-user-input max="1" .value=${this.value ?? ''} @change=${this.#onChange}></umb-user-input>`;
	}
}

export default UmbPropertyEditorUIUserPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-user-picker': UmbPropertyEditorUIUserPickerElement;
	}
}
