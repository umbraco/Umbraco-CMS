import type { UmbSectionItemModel } from '../../repository/index.js';
import { UmbSectionPickerContext } from './input-section.context.js';
import { css, html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';

@customElement('umb-input-section')
export class UmbInputSectionElement extends UUIFormControlMixin(UmbLitElement, '') {
	/**
	 * This is a minimum amount of selected items in this input.
	 * @type {number}
	 * @attr
	 * @default 0
	 */
	@property({ type: Number })
	public set min(value: number) {
		this.#pickerContext.min = value;
	}
	public get min(): number {
		return this.#pickerContext.min;
	}

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
	 * @default Infinity
	 */
	@property({ type: Number })
	public set max(value: number) {
		this.#pickerContext.max = value;
	}
	public get max(): number {
		return this.#pickerContext.max;
	}

	/**
	 * Max validation message.
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: String, attribute: 'min-message' })
	maxMessage = 'This field exceeds the allowed amount of items';

	public set selection(uniques: Array<string>) {
		this.#pickerContext.setSelection(uniques);
	}
	public get selection(): Array<string> {
		return this.#pickerContext.getSelection();
	}

	@property()
	public override set value(selectionString: string) {
		// Its with full purpose we don't call super.value, as thats being handled by the observation of the context selection.
		if (typeof selectionString !== 'string') return;
		if (selectionString === this.value) return;
		this.selection = splitStringToArray(selectionString);
	}
	public override get value(): string {
		return this.selection.join(',');
	}

	@state()
	private _items?: Array<UmbSectionItemModel>;

	#pickerContext = new UmbSectionPickerContext(this);

	constructor() {
		super();

		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() => !!this.min && this.#pickerContext.getSelection().length < this.min,
		);

		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !!this.max && this.#pickerContext.getSelection().length > this.max,
		);

		this.observe(this.#pickerContext.selection, (selection) => (this.value = selection.join(',')));
		this.observe(this.#pickerContext.selectedItems, (selectedItems) => (this._items = selectedItems));
	}

	protected getFormElement() {
		return undefined;
	}

	override render() {
		return html`
			<uui-ref-list>${this._items?.map((item) => this._renderItem(item))}</uui-ref-list>
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
