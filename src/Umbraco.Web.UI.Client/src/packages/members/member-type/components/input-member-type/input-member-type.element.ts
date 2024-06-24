import { UmbMemberTypePickerContext } from './input-member-type.context.js';
import { css, html, customElement, property, state, repeat, when } from '@umbraco-cms/backoffice/external/lit';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbUniqueItemModel } from '@umbraco-cms/backoffice/models';

@customElement('umb-input-member-type')
export class UmbInputMemberTypeElement extends UUIFormControlMixin(UmbLitElement, '') {
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

	public set selection(ids: Array<string>) {
		this.#pickerContext.setSelection(ids);
	}
	public get selection(): Array<string> {
		return this.#pickerContext.getSelection();
	}

	@property()
	public override set value(idsString: string) {
		// Its with full purpose we don't call super.value, as thats being handled by the observation of the context selection. [NL]
		this.selection = splitStringToArray(idsString);
	}
	public override get value(): string {
		return this.selection.join(',');
	}

	@state()
	private _items?: Array<UmbUniqueItemModel>;

	#pickerContext = new UmbMemberTypePickerContext(this);

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

	#openPicker() {
		this.#pickerContext.openPicker({
			hideTreeRoot: true,
		});
	}

	override render() {
		return html` ${this.#renderItems()} ${this.#renderAddButton()} `;
	}

	#renderItems() {
		if (!this._items) return;
		return html`
			<uui-ref-list>
				${repeat(
					this._items,
					(item) => item.unique,
					(item) => this.#renderItem(item),
				)}
			</uui-ref-list>
		`;
	}

	#renderAddButton() {
		if (this.max === 1 && this.selection.length >= this.max) return;
		return html`
			<uui-button
				id="btn-add"
				look="placeholder"
				@click=${this.#openPicker}
				label="${this.localize.term('general_choose')}"
				>${this.localize.term('general_choose')}</uui-button
			>
		`;
	}

	#renderItem(item: UmbUniqueItemModel) {
		if (!item.unique) return;
		return html`
			<uui-ref-node-document-type name=${item.name}>
				${when(item.icon, () => html`<umb-icon slot="icon" name=${item.icon!}></umb-icon>`)}
				<uui-action-bar slot="actions">
					<uui-button
						@click=${() => this.#pickerContext.requestRemoveItem(item.unique!)}
						label="Remove Member Type ${item.name}"
						>${this.localize.term('general_remove')}</uui-button
					>
				</uui-action-bar>
			</uui-ref-node-document-type>
		`;
	}

	static override styles = [
		css`
			#btn-add {
				width: 100%;
			}
		`,
	];
}

export default UmbInputMemberTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-member-type': UmbInputMemberTypeElement;
	}
}
