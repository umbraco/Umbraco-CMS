import { UmbDocumentTypePickerContext } from './input-document-type.context.js';
import { css, html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { DocumentTypeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';

@customElement('umb-input-document-type')
export class UmbInputDocumentTypeElement extends FormControlMixin(UmbLitElement) {
	/**
	 * Selects the element types only
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, attribute: 'element-types-only' })
	elementTypesOnly: boolean = false;

	/**
	 * This is a minimum amount of selected items in this input.
	 * @type {number}
	 * @attr
	 * @default 0
	 */
	@property({ type: Number })
	public get min(): number {
		return this.#pickerContext.min;
	}
	public set min(value: number) {
		this.#pickerContext.min = value;
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
	public get max(): number {
		return this.#pickerContext.max;
	}
	public set max(value: number) {
		this.#pickerContext.max = value;
	}

	/**
	 * Max validation message.
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: String, attribute: 'min-message' })
	maxMessage = 'This field exceeds the allowed amount of items';

	public get selectedIds(): Array<string> {
		return this.#pickerContext.getSelection();
	}
	public set selectedIds(ids: Array<string>) {
		this.#pickerContext.setSelection(ids);
	}

	@property()
	public set value(idsString: string) {
		// Its with full purpose we don't call super.value, as thats being handled by the observation of the context selection.
		this.selectedIds = splitStringToArray(idsString);
	}

	@state()
	private _items?: Array<DocumentTypeItemResponseModel>;

	#pickerContext = new UmbDocumentTypePickerContext(this);

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

		this.observe(this.#pickerContext.selection, (selection) => (super.value = selection.join(',')));
		this.observe(this.#pickerContext.selectedItems, (selectedItems) => (this._items = selectedItems));
	}

	protected getFormElement() {
		return undefined;
	}

	#openPicker() {
		if (this.elementTypesOnly) {
			this.#pickerContext.openPicker({
				hideTreeRoot: true,
				pickableFilter: (x) => x.isElement,
			});
		} else {
			this.#pickerContext.openPicker({ hideTreeRoot: true });
		}
	}

	render() {
		return html`
			<uui-ref-list>${this._items?.map((item) => this._renderItem(item))}</uui-ref-list>
			<uui-button id="add-button" look="placeholder" @click=${this.#openPicker} label="open">Add</uui-button>
		`;
	}

	private _renderItem(item: DocumentTypeItemResponseModel) {
		if (!item.id) return;
		return html`
			<uui-ref-node-document-type name=${ifDefined(item.name)}>
				<uui-action-bar slot="actions">
					<uui-button
						@click=${() => this.#pickerContext.requestRemoveItem(item.id!)}
						label="Remove Document Type ${item.name}"
						>Remove</uui-button
					>
				</uui-action-bar>
			</uui-ref-node-document-type>
		`;
	}

	static styles = [
		css`
			#add-button {
				width: 100%;
			}
		`,
	];
}

export default UmbInputDocumentTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-document-type': UmbInputDocumentTypeElement;
	}
}
