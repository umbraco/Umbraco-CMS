import type { UmbStylesheetItemModel } from '../../types.js';
import { UmbStylesheetPickerInputContext } from './stylesheet-input.context.js';
import { css, html, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';

@customElement('umb-stylesheet-input')
export class UmbStylesheetInputElement extends UUIFormControlMixin(UmbLitElement, '') {
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
	 * Max validation message.
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: String, attribute: 'min-message' })
	maxMessage = 'This field exceeds the allowed amount of items';

	@property({ type: Array })
	public set selection(ids: Array<string> | undefined) {
		this.#pickerContext.setSelection(ids ?? []);
	}
	public get selection(): Array<string> | undefined {
		return this.#pickerContext.getSelection();
	}

	@property()
	public override set value(idsString: string) {
		// Its with full purpose we don't call super.value, as thats being handled by the observation of the context selection.
		this.selection = splitStringToArray(idsString);
	}
	public override get value() {
		return this.#pickerContext.getSelection().join(',');
	}

	@state()
	private _items: Array<UmbStylesheetItemModel> = [];

	#pickerContext = new UmbStylesheetPickerInputContext(this);

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

	protected override getFormElement() {
		return undefined;
	}

	override render() {
		return html`
			<uui-ref-list>
				${repeat(
					this._items,
					(item) => item.unique,
					(item) => this.#renderItem(item),
				)}
			</uui-ref-list>
			<uui-button
				id="btn-add"
				look="placeholder"
				@click=${() => this.#pickerContext.openPicker()}
				label="Add stylesheet"></uui-button>
		`;
	}

	#renderItem(item: UmbStylesheetItemModel) {
		if (!item.unique) return;
		return html`
			<uui-ref-node-data-type name=${item.name}>
				<uui-action-bar slot="actions">
					<uui-button
						@click=${() => this.#pickerContext.requestRemoveItem(item.unique!)}
						label=${this.localize.term('general_remove')}></uui-button>
				</uui-action-bar>
			</uui-ref-node-data-type>
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

export default UmbStylesheetInputElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-stylesheet-input': UmbStylesheetInputElement;
	}
}
