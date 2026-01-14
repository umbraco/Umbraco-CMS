import { customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
//import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-property-editor-ui-element-picker')
export class UmbPropertyEditorUIElementPicker
	extends UmbFormControlMixin<Array<string> | undefined, typeof UmbLitElement>(UmbLitElement, undefined)
	implements UmbPropertyEditorUiElement
{
	@property({ attribute: false })
	public config?: UmbPropertyEditorUiElement['config'];

	@property({ type: Boolean })
	mandatory?: boolean;

	@property({ type: String })
	mandatoryMessage = UMB_VALIDATION_EMPTY_LOCALIZATION_KEY;

	@property({ type: String })
	name?: string;

	@property({ type: Boolean, reflect: true })
	readonly = false;

	protected override firstUpdated() {
		this.addFormControlElement(this.shadowRoot!.querySelector('umb-element-input')!);
	}

	// #onChange(event: CustomEvent & { target: UmbElementInputElement }) {
	// 	this.value = event.target.value;
	// 	this.dispatchEvent(new UmbChangeEvent());
	// }

	// override render() {
	// 	return html`
	// 		<umb-element-input
	// 			.requiredMessage=${this.mandatoryMessage}
	// 			.value=${this.value}
	// 			?readonly=${this.readonly}
	// 			?required=${this.mandatory}
	// 			@change=${this.#onChange}>
	// 		</umb-element-input>
	// 	`;
	// }
}

export default UmbPropertyEditorUIElementPicker;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-element-picker': UmbPropertyEditorUIElementPicker;
	}
}
