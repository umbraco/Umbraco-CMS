import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
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
		this.value = event.target.value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<umb-user-input min="0" max="1" .value=${this.value ?? ''} @change=${this.#onChange}></umb-user-input>
		`;
	}
}

export default UmbPropertyEditorUIUserPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-user-picker': UmbPropertyEditorUIUserPickerElement;
	}
}
