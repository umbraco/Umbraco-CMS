import type UmbInputTimeZoneItemElement from './input-time-zone-item.element.js';
import type { UmbInputTimeZonePickerElement } from './input-time-zone-picker.element.js';
import {
	html,
	customElement,
	property,
	css,
	repeat,
	nothing,
	query,
	when,
	state,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { getTimeZoneList, type TimeZone } from '@umbraco-cms/backoffice/utils';
import { DateTime } from '@umbraco-cms/backoffice/external/luxon';

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
	 */
	@property({ type: Number })
	public set min(value) {
		this.#min = value;
	}
	public get min() {
		if (this.required && this.#min < 1) {
			return 1;
		}
		return this.#min;
	}
	#min = 0;

	/**
	 * This is a maximum amount of selected items in this input.
	 * @type {number}
	 * @attr
	 */
	@property({ type: Number })
	max?: number;

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

	@property({ type: Array, reflect: false })
	override set value(value: Array<string>) {
		super.value = value;
		this.#sorter.setModel(this.value);
	}

	override get value(): Array<string> {
		return super.value;
	}

	@state()
	private _disableAddButton = true;

	@query('umb-input-time-zone-picker')
	protected _timeZonePicker?: UmbInputTimeZonePickerElement;

	private _timeZoneList: Array<TimeZone> = [];

	constructor() {
		super();
		this._timeZoneList = getTimeZoneList(undefined, DateTime.now());

		this.addValidator(
			'rangeUnderflow',
			() => this.localize.term('validation_entriesShort', this.min, this.min - this.value.length),
			() => this.value.length < this.min,
		);

		this.addValidator(
			'rangeOverflow',
			() => this.localize.term('validation_entriesExceed', this.max, this.value.length - (this.max || 0)),
			() => !!this.max && this.value.length > this.max,
		);
	}

	protected override getFormElement() {
		return undefined;
	}

	#onAdd() {
		if (this._timeZonePicker) {
			this.value = [...this.value, this._timeZonePicker.value];
			this._timeZonePicker.value = '';
			this._disableAddButton = true;
		}
		this.pristine = false;
		this.dispatchEvent(new UmbChangeEvent());
	}

	#onChange(event: UmbChangeEvent, currentIndex: number) {
		const target = event.currentTarget as UmbInputTimeZoneItemElement;
		const value = target.value as string;
		this.value = this.value.map((item, index) => (index === currentIndex ? value : item));
		this.pristine = false;
		this.dispatchEvent(new UmbChangeEvent());
	}

	#deleteItem(itemIndex: number) {
		this.value = this.value.filter((_item, index) => index !== itemIndex);
		this.pristine = false;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`<div id="sorter-wrapper">${this.#renderSelectedItems()}</div>
			${this.#renderAddTimeZone()}`;
	}

	#renderSelectedItems() {
		return html`
			${repeat(
				this.value,
				(_item, index) => index,
				(item, index) => html`
					<umb-input-time-zone-item
						name="item-${index}"
						data-sort-entry-id=${item}
						value=${item}
						?disabled=${this.disabled}
						?readonly=${this.readonly}
						@delete=${() => this.#deleteItem(index)}
						@change=${(event: UmbChangeEvent) => this.#onChange(event, index)}>
					</umb-input-time-zone-item>
				`,
			)}
		`;
	}

	#renderAddTimeZone() {
		if (this.disabled || this.readonly) return nothing;
		return html`
			<div id="add-time-zone">
				<umb-input-time-zone-picker
					id="time-zone-picker"
					name="picker"
					.options=${this._timeZoneList.filter((tz) => !this.value.includes(tz.value))}
					@change=${(event: UmbChangeEvent) => {
						this._disableAddButton = !(event.target as UmbInputTimeZonePickerElement)?.value;
					}}
					?disabled=${this.disabled}
					?readonly=${this.readonly}>
				</umb-input-time-zone-picker>
				${when(
					!this.readonly,
					() => html`
						<uui-button
							compact
							label="${this.localize.term('general_add')} ${this._timeZonePicker?.value}"
							look="outline"
							color="positive"
							?disabled=${this.disabled || this._disableAddButton}
							@click=${this.#onAdd}>
							<uui-icon name="icon-add"></uui-icon>
						</uui-button>
					`,
				)}
			</div>
		`;
	}

	static override styles = [
		css`
			#add-time-zone {
				display: flex;
				margin-bottom: var(--uui-size-space-3);
				gap: var(--uui-size-space-1);
			}

			#time-zone-picker {
				width: 100%;
				display: inline-flex;
				--uui-input-height: var(--uui-size-12);
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
