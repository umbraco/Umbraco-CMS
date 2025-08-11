import type UmbInputTimeZoneItemElement from './input-time-zone-item.element.js';
import type { UmbTimeZoneAddEvent } from './input-time-zone-picker.element.js';
import { html, customElement, property, css, repeat, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { UmbDeleteEvent } from '@umbraco-cms/backoffice/event';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * @element umb-input-time-zone
 */
@customElement('umb-input-time-zone')
export class UmbInputTimeZoneElement extends UmbFormControlMixin<Array<string>, typeof UmbLitElement, Array<string>>(
	UmbLitElement,
	[],
) {
	#sorter = new UmbSorterController(this, {
		getUniqueOfElement: (element) => {
			return element.getAttribute('data-sort-entry-id');
		},
		getUniqueOfModel: (modelEntry: string) => {
			return modelEntry;
		},
		identifier: 'Umb.SorterIdentifier.TimeZone',
		itemSelector: 'umb-input-time-zone-item',
		containerSelector: '#sorter-wrapper',
		onChange: ({ model }) => {
			this.value = model;
			this.dispatchEvent(new UmbChangeEvent());
		},
	});

	/**
	 * This is a minimum amount of selected items in this input.
	 * @type {number}
	 * @attr
	 * @default undefined
	 */
	@property({ type: Number })
	min?: number;

	/**
	 * Min validation message.
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: String, attribute: 'min-message' })
	minMessage = 'This field need more items';

	/**
	 * This is a maximum amount of selected items in this input.
	 * @type {number}
	 * @attr
	 * @default undefined
	 */
	@property({ type: Number })
	max?: number;

	/**
	 * Max validation message.
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: String, attribute: 'min-message' })
	maxMessage = 'This field exceeds the allowed amount of items';

	/**
	 * Disables the input
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: Boolean, reflect: true })
	public set disabled(value) {
		this.#disabled = value;
		if (value) {
			this.#sorter.disable();
		}
	}
	public get disabled() {
		return this.#disabled;
	}
	#disabled = false;

	/**
	 * Makes the input readonly
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: Boolean, reflect: true })
	public set readonly(value) {
		this.#readonly = value;
		if (value) {
			this.#sorter.disable();
		}
	}
	public get readonly() {
		return this.#readonly;
	}
	#readonly = false;

	/**
	 * Sets the input to required, meaning validation will fail if the value is empty.
	 * @type {boolean}
	 */
	@property({ type: Boolean })
	required?: boolean;

	@property({ type: String })
	requiredMessage?: string;

	@property({ type: Array, reflect: false })
	override set value(value: Array<string>) {
		super.value = value;
		this.#sorter.setModel(this.value);
	}

	override get value(): Array<string> {
		return super.value;
	}

	constructor() {
		super();
		console.log(this.value);

		this.addValidator(
			'valueMissing',
			() => this.requiredMessage ?? UMB_VALIDATION_EMPTY_LOCALIZATION_KEY,
			() => !this.readonly && !!this.required && (this.value === undefined || this.value === null || this.value === ''),
		);

		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() => !!this.min && this.value.length < this.min,
		);
		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !!this.max && this.value.length > this.max,
		);
	}

	#onAdd(event: UmbTimeZoneAddEvent) {
		this.value = [...this.value, event.getValue()];
		this.pristine = false;
		this.dispatchEvent(new UmbChangeEvent());
	}

	#onChange(event: UmbChangeEvent, currentIndex: number) {
		event.stopPropagation();
		const target = event.currentTarget as UmbInputTimeZoneItemElement;
		const value = target.value as string;
		this.value = this.value.map((item, index) => (index === currentIndex ? value : item));
		this.dispatchEvent(new UmbChangeEvent());
	}

	#deleteItem(event: UmbDeleteEvent, itemIndex: number) {
		event.stopPropagation();
		this.value = this.value.filter((_item, index) => index !== itemIndex);
		this.pristine = false;
		this.dispatchEvent(new UmbChangeEvent());
	}

	// Prevent valid events from bubbling outside the message element
	#onValid(event: Event) {
		event.stopPropagation();
	}

	// Prevent invalid events from bubbling outside the message element
	#onInvalid(event: Event) {
		event.stopPropagation();
	}

	override getFormElement() {
		return undefined;
	}

	override render() {
		return html` <umb-form-validation-message
			id="validation-message"
			@invalid=${this.#onInvalid}
			@valid=${this.#onValid}>
			<div id="sorter-wrapper">${this.#renderItems()}</div>
			${this.#renderAddButton()}
		</umb-form-validation-message>`;
	}

	#renderItems() {
		return html`
			${repeat(
				this.value,
				(_item, index) => index,
				(item, index) => html`
					<umb-input-time-zone-item
						name="item-${index}"
						data-sort-entry-id=${item}
						required
						required-message="Item ${index + 1} is missing a value"
						value=${item}
						?disabled=${this.disabled}
						?readonly=${this.readonly}
						@enter=${this.#onAdd}
						@delete=${(event: UmbDeleteEvent) => this.#deleteItem(event, index)}
						@change=${(event: UmbChangeEvent) => this.#onChange(event, index)}>
					</umb-input-time-zone-item>
				`,
			)}
		`;
	}

	#renderAddButton() {
		if (this.disabled || this.readonly) return nothing;
		return html`
			<umb-input-time-zone-picker
				name="picker"
				?disabled=${this.disabled}
				?readonly=${this.readonly}
				@added=${this.#onAdd}>
			</umb-input-time-zone-picker>
		`;
	}

	static override styles = [
		css`
			#action {
				display: block;
			}

			.--umb-sorter-placeholder {
				position: relative;
				visibility: hidden;
			}
			.--umb-sorter-placeholder::after {
				content: '';
				position: absolute;
				inset: 0px;
				border-radius: var(--uui-border-radius);
				border: 1px dashed var(--uui-color-divider-emphasis);
			}
		`,
	];
}

export default UmbInputTimeZoneElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-time-zone': UmbInputTimeZoneElement;
	}
}
