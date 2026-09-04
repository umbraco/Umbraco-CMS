import type { UmbInputMemberGroupElement } from '../../components/index.js';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

/**
 * @element umb-property-editor-ui-member-group-picker
 */
@customElement('umb-property-editor-ui-member-group-picker')
export class UmbPropertyEditorUIMemberGroupPickerElement
	extends UmbFormControlMixin<string, typeof UmbLitElement, undefined>(UmbLitElement, undefined)
	implements UmbPropertyEditorUiElement
{
	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;
	@property({ type: Boolean })
	mandatory?: boolean;
	@property({ type: String })
	mandatoryMessage = UMB_VALIDATION_EMPTY_LOCALIZATION_KEY;

	protected override firstUpdated() {
		this.addFormControlElement(this.shadowRoot!.querySelector('umb-input-member-group')!);
	}

	#onChange(event: CustomEvent & { target: UmbInputMemberGroupElement }) {
		this.value = event.target.value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<umb-input-member-group
				.value=${this.value}
				@change=${this.#onChange}
				?required=${this.mandatory}
				.requiredMessage=${this.mandatoryMessage}
				?readonly=${this.readonly}></umb-input-member-group>
		`;
	}
}

export default UmbPropertyEditorUIMemberGroupPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-member-group-picker': UmbPropertyEditorUIMemberGroupPickerElement;
	}
}
