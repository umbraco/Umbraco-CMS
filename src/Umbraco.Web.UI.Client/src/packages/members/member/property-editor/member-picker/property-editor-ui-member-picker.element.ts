import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import type { UmbInputMemberElement } from '@umbraco-cms/backoffice/member';

/**
 * @element umb-property-editor-ui-member-picker
 */
@customElement('umb-property-editor-ui-member-picker')
export class UmbPropertyEditorUIMemberPickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	public value?: string;

	@property({ attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	#onChange(event: CustomEvent & { target: UmbInputMemberElement }) {
		this.value = event.target.value;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		return html`<umb-input-member
			min="0"
			max="1"
			.value=${this.value}
			@change=${this.#onChange}
			?readonly=${this.readonly}></umb-input-member>`;
	}
}

export default UmbPropertyEditorUIMemberPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-member-picker': UmbPropertyEditorUIMemberPickerElement;
	}
}
