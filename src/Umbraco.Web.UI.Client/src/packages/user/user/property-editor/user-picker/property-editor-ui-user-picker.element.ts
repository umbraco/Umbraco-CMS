import type { UmbUserInputElement } from '../../components/index.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

/**
 * @element umb-property-editor-ui-user-picker
 */
@customElement('umb-property-editor-ui-user-picker')
export class UmbPropertyEditorUIUserPickerElement
	extends UmbFormControlMixin<string, typeof UmbLitElement, undefined>(UmbLitElement, undefined)
	implements UmbPropertyEditorUiElement
{
	@property({ type: Boolean, reflect: true })
	readonly = false;
	@property({ type: Boolean })
	mandatory?: boolean;
	@property({ type: String })
	mandatoryMessage = UMB_VALIDATION_EMPTY_LOCALIZATION_KEY;

	@property({ attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	protected override firstUpdated() {
		this.addFormControlElement(this.shadowRoot!.querySelector('umb-user-input')!);
	}

	#onChange(event: CustomEvent & { target: UmbUserInputElement }) {
		this.value = event.target.value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<umb-user-input
				min="0"
				max="1"
				.value=${this.value}
				@change=${this.#onChange}
				?readonly=${this.readonly}
				?required=${this.mandatory}
				.requiredMessage=${this.mandatoryMessage}></umb-user-input>
		`;
	}
}

export default UmbPropertyEditorUIUserPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-user-picker': UmbPropertyEditorUIUserPickerElement;
	}
}
