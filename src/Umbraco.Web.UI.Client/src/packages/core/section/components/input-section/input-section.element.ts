import type { UmbSectionItemModel } from '../../types.js';
import { UmbSectionPickerInputContext } from './input-section.context.js';
import { css, html, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { isAboveZeroAndBelowMinimum, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

@customElement('umb-input-section')
export class UmbInputSectionElement extends UmbFormControlMixin<string | undefined, typeof UmbLitElement>(
	UmbLitElement,
) {
	/**
	 * This is a minimum amount of selected items in this input.
	 * @type {number}
	 * @attr
	 * @default
	 */
	@property({ type: Number })
	public set min(value: number) {
		this.#pickerContext.min = value;
	}
	public get min(): number {
		return this.#pickerContext.min;
	}

	/**
	 * Min validation message. When not set, a localized message stating the minimum and how many more items are needed is used.
	 * @type {string}
	 * @attr
	 * @default undefined
	 */
	@property({ type: String, attribute: 'min-message' })
	minMessage?: string;

	/**
	 * This is a maximum amount of selected items in this input.
	 * @type {number}
	 * @attr
	 * @default
	 */
	@property({ type: Number })
	public set max(value: number) {
		this.#pickerContext.max = value;
	}
	public get max(): number {
		return this.#pickerContext.max;
	}

	/**
	 * Max validation message. When not set, a localized message stating the maximum and how many items are over it is used.
	 * @type {string}
	 * @attr
	 * @default undefined
	 */
	@property({ type: String, attribute: 'max-message' })
	maxMessage?: string;

	public set selection(uniques: Array<string>) {
		this.#pickerContext.setSelection(uniques);
	}
	public get selection(): Array<string> {
		return this.#pickerContext.getSelection();
	}

	@property({ type: String })
	public override set value(selectionString: string | undefined) {
		this.selection = splitStringToArray(selectionString);
	}
	public override get value(): string | undefined {
		return this.selection.length > 0 ? this.selection.join(',') : undefined;
	}

	@state()
	private _items?: Array<UmbSectionItemModel>;

	#pickerContext = new UmbSectionPickerInputContext(this);

	constructor() {
		super();

		this.addValidator(
			'rangeUnderflow',
			() =>
				this.minMessage ||
				this.localize.term(
					'validation_entriesShort',
					this.min,
					(this.min ?? 0) - this.#pickerContext.getSelection().length,
				),
			() => isAboveZeroAndBelowMinimum(this.#pickerContext.getSelection().length, this.min),
		);

		this.addValidator(
			'rangeOverflow',
			() =>
				this.maxMessage ||
				this.localize.term(
					'validation_entriesExceed',
					this.max,
					this.#pickerContext.getSelection().length - (this.max ?? 0),
				),
			() => !!this.max && this.#pickerContext.getSelection().length > this.max,
		);

		this.observe(this.#pickerContext.selection, (selection) => (this.value = selection.join(',')), null);
		this.observe(
			this.#pickerContext.selectedItems,
			(selectedItems) => (this._items = [...selectedItems].sort((a, b) => (b.weight ?? 0) - (a.weight ?? 0))),
			null,
		);
	}

	protected override getFormElement() {
		return undefined;
	}

	override render() {
		return html`
			<uui-ref-list
				>${repeat(
					this._items ?? [],
					(item) => item.unique,
					(item) => this._renderItem(item),
				)}</uui-ref-list
			>
			<uui-button
				id="btn-add"
				look="placeholder"
				@click=${() => this.#pickerContext.openPicker()}
				label=${this.localize.term('general_choose')}></uui-button>
		`;
	}

	private _renderItem(item: UmbSectionItemModel) {
		if (!item.unique) return;
		return html` <umb-ref-section .item=${item}>
			<uui-action-bar slot="actions">
				<uui-button @click=${() => this.#pickerContext.requestRemoveItem(item.unique)} label="Remove ${item.name}"
					>Remove</uui-button
				>
			</uui-action-bar>
		</umb-ref-section>`;
	}

	static override styles = [
		css`
			#btn-add {
				width: 100%;
			}
		`,
	];
}

export default UmbInputSectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-section': UmbInputSectionElement;
	}
}
