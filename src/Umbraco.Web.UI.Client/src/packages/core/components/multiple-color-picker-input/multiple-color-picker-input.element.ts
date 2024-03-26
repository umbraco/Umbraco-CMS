import type { UmbMultipleColorPickerItemInputElement } from './multiple-color-picker-item-input.element.js';
import {
	css,
	customElement,
	html,
	ifDefined,
	nothing,
	repeat,
	property,
	state,
} from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { UmbInputEvent, UmbDeleteEvent } from '@umbraco-cms/backoffice/event';
import type { UmbSwatchDetails } from '@umbraco-cms/backoffice/models';

/**
 * @element umb-multiple-color-picker-input
 */
@customElement('umb-multiple-color-picker-input')
export class UmbMultipleColorPickerInputElement extends FormControlMixin(UmbLitElement) {
	#sorter = new UmbSorterController(this, {
		getUniqueOfElement: (element: UmbMultipleColorPickerItemInputElement) => {
			return element.value.toString();
		},
		getUniqueOfModel: (modelEntry: UmbSwatchDetails) => {
			return modelEntry.value;
		},
		identifier: 'Umb.SorterIdentifier.ColorEditor',
		itemSelector: 'umb-multiple-color-picker-item-input',
		containerSelector: '#sorter-wrapper',
		onChange: ({ model }) => {
			const oldValue = this._items;
			this._items = model;
			this.requestUpdate('_items', oldValue);
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
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	disabled = false;

	/**
	 * Makes the input readonly
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	@property({ type: Boolean })
	showLabels = true;

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, async (instance) => {
			const workspace = instance;
			this.observe(
				await workspace.propertyValueByAlias<boolean>('useLabel'),
				(value) => {
					// Only set a true/false value. If value is undefined, keep the default value of True, if value is defined, set the value but remove the undefined type from the Type Union.
					this.showLabels = value === undefined ? true : (value as Exclude<typeof value, undefined>);
				},
				'observeUseLabel',
			);
		});

		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() => !!this.min && this._items.length < this.min,
		);
		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !!this.max && this._items.length > this.max,
		);
	}

	@state()
	private _items: Array<UmbSwatchDetails> = [];

	@property({ type: Array })
	public get items(): Array<UmbSwatchDetails> {
		return this._items;
	}
	public set items(items: Array<UmbSwatchDetails>) {
		this._items = items ?? [];
		this.#sorter.setModel(this.items);
	}

	#onAdd() {
		this.items = [...this._items, { value: '', label: '' }];
		this.pristine = false;
		this.dispatchEvent(new UmbChangeEvent());
		this.#focusNewItem();
	}

	#onChange(event: UmbInputEvent, currentIndex: number) {
		event.stopPropagation();
		const target = event.currentTarget as UmbMultipleColorPickerItemInputElement;
		const value = target.value as string;
		const label = target.label as string;

		this.items = this._items.map((item, index) => (index === currentIndex ? { value, label } : item));

		this.dispatchEvent(new UmbChangeEvent());
	}

	async #focusNewItem() {
		await this.updateComplete;
		const items = this.shadowRoot?.querySelectorAll<UmbMultipleColorPickerItemInputElement>(
			'umb-multiple-color-picker-item-input',
		);
		if (items) {
			const newItem = items[items.length - 1];
			newItem.focus();
		}
	}

	#deleteItem(event: UmbDeleteEvent, itemIndex: number) {
		event.stopPropagation();
		this.items = this._items.filter((item, index) => index !== itemIndex);
		this.pristine = false;
		this.dispatchEvent(new UmbChangeEvent());
	}

	protected getFormElement() {
		return undefined;
	}

	render() {
		return html`<div id="sorter-wrapper">${this.#renderItems()}</div>
			${this.#renderAddButton()} `;
	}

	#renderItems() {
		return html`
			${repeat(
				this._items,
				(item) => item.value,
				(item, index) =>
					html` <umb-multiple-color-picker-item-input
						?showLabels=${this.showLabels}
						value=${item.value}
						label=${ifDefined(item.label)}
						@change=${(event: UmbChangeEvent) => this.#onChange(event, index)}
						@delete="${(event: UmbDeleteEvent) => this.#deleteItem(event, index)}"
						?disabled=${this.disabled}
						?readonly=${this.readonly}
						required
						required-message="Item ${index + 1} is missing a value"></umb-multiple-color-picker-item-input>`,
			)}
		`;
	}

	#renderAddButton() {
		return html`
			${this.disabled || this.readonly
				? nothing
				: html`<uui-button
						id="action"
						label=${this.localize.term('general_add')}
						look="placeholder"
						color="default"
						@click="${this.#onAdd}"
						?disabled=${this.disabled}></uui-button>`}
		`;
	}

	static styles = [
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

export default UmbMultipleColorPickerInputElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-multiple-color-picker-input': UmbMultipleColorPickerInputElement;
	}
}
