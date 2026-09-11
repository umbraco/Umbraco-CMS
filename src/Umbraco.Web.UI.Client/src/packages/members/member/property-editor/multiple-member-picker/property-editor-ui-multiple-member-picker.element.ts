import type { UmbInputMemberElement } from '../../components/index.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbNumberRangeValueType } from '@umbraco-cms/backoffice/models';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

/**
 * @element umb-property-editor-ui-multiple-member-picker
 */
@customElement('umb-property-editor-ui-multiple-member-picker')
export class UmbPropertyEditorUIMultipleMemberPickerElement
	extends UmbFormControlMixin<Array<string> | undefined, typeof UmbLitElement, undefined>(UmbLitElement)
	implements UmbPropertyEditorUiElement
{
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const minMax = config.getValueByAlias<UmbNumberRangeValueType>('validationLimit');
		this._min = minMax?.min ?? 0;
		this._max = minMax?.max ?? Infinity;

		const filter = config.getValueByAlias<string>('filter');
		this._allowedMemberTypes = filter ? filter.split(',').filter(Boolean) : undefined;
	}

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

	@state()
	private _min = 0;

	@state()
	private _max = Infinity;

	@state()
	private _allowedMemberTypes?: Array<string>;

	protected override firstUpdated() {
		this.addFormControlElement(this.shadowRoot!.querySelector('umb-input-member')!);

		if (this._min && this._max && this._min > this._max) {
			console.warn(
				`Property (Multiple Member Picker) has been misconfigured, 'min' is greater than 'max'. Please correct your data type configuration.`,
				this,
			);
		}
	}

	#onChange(event: CustomEvent & { target: UmbInputMemberElement }) {
		const selection = event.target.selection;
		this.value = selection.length > 0 ? selection : undefined;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`<umb-input-member
			.min=${this._min}
			.max=${this._max}
			.allowedContentTypeIds=${this._allowedMemberTypes}
			.selection=${this.value ?? []}
			@change=${this.#onChange}
			?required=${this.mandatory}
			.requiredMessage=${this.mandatoryMessage}
			?readonly=${this.readonly}></umb-input-member>`;
	}
}

export default UmbPropertyEditorUIMultipleMemberPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-multiple-member-picker': UmbPropertyEditorUIMultipleMemberPickerElement;
	}
}
