import { css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-property-editor-ui-element-picker')
export class UmbPropertyEditorUIElementPicker
	extends UmbFormControlMixin<Array<string> | undefined, typeof UmbLitElement>(UmbLitElement, undefined)
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

	/**
	 * Sets the input to mandatory, meaning validation will fail if the value is empty.
	 * @type {boolean}
	 */
	@property({ type: Boolean })
	mandatory?: boolean;
	@property({ type: String })
	mandatoryMessage = UMB_VALIDATION_EMPTY_LOCALIZATION_KEY;

	/**
	 * The name of this field.
	 * @type {string}
	 */
	@property({ type: String })
	name?: string;

	#onInput(event: InputEvent) {
		this.value = JSON.parse((event.target as HTMLTextAreaElement).value);
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<uui-textarea
				rows="10"
				.label=${this.localize.term('general_fieldFor', [this.name])}
				.value=${JSON.stringify((this.value ?? []), null, '  ')}
				@input=${this.#onInput}
				?required=${this.mandatory}
				.requiredMessage=${this.mandatoryMessage}
				?readonly=${this.readonly}></uui-textarea>
		`;
	}

	static override readonly styles = [
		UmbTextStyles,
		css`
			uui-textarea {
				width: 100%;
			}
		`,
	];
}

export default UmbPropertyEditorUIElementPicker;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-element-picker': UmbPropertyEditorUIElementPicker;
	}
}
