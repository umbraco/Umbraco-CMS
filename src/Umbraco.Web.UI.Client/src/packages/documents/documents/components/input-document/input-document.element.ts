import { UmbDocumentPickerContext } from './input-document.context.js';
import { css, html, customElement, property, state, ifDefined, repeat } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { DocumentItemResponseModel, DocumentTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';

@customElement('umb-input-document')
export class UmbInputDocumentElement extends FormControlMixin(UmbLitElement) {
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

	@property({ type: String })
	startNodeId?: string;

	@property({ type: Array })
	allowedContentTypeIds?: string[] | undefined;

	@property({ type: Boolean })
	showOpenButton?: boolean;

	@property({ type: Boolean })
	ignoreUserStartNodes?: boolean;

	@property()
	public set value(idsString: string) {
		// Its with full purpose we don't call super.value, as thats being handled by the observation of the context selection.
		this.selectedIds = splitStringToArray(idsString);
	}

	@state()
	private _items?: Array<DocumentItemResponseModel>;

	#pickerContext = new UmbDocumentPickerContext(this);

	constructor() {
		super();
	}

	connectedCallback() {
		super.connectedCallback();

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

	#pickableFilter: (item: DocumentTreeItemResponseModel) => boolean = (item) => {
		if (this.allowedContentTypeIds && this.allowedContentTypeIds.length > 0) {
			return this.allowedContentTypeIds.includes(item.contentTypeId);
		}
		return true;
	}

	#openPicker() {
		// TODO: Configure the content picker, with `startNodeId` and `ignoreUserStartNodes` [LK]
		console.log('_openPicker', [this.startNodeId, this.ignoreUserStartNodes]);
		this.#pickerContext.openPicker({
			hideTreeRoot: true,
			pickableFilter: this.#pickableFilter,
		});
	}

	#openItem(item: DocumentItemResponseModel) {
		// TODO: Implement the Content editing infinity editor. [LK]
		console.log('TODO: _openItem', item);
	}

	render() {
		return html` ${this.#renderItems()} ${this.#renderAddButton()} `;
	}

	#renderItems() {
		if (!this._items) return;
		// TODO: Add sorting. [LK]
		return html`<uui-ref-list
			>${repeat(
				this._items,
				(item) => item.id,
				(item) => this.#renderItem(item),
			)}
		</uui-ref-list>`;
	}

	#renderAddButton() {
		if (this.max > 0 && this.selectedIds.length >= this.max) return;
		return html`<uui-button
			id="add-button"
			look="placeholder"
			@click=${this.#openPicker}
			label=${this.localize.term('general_choose')}></uui-button>`;
	}

	#renderItem(item: DocumentItemResponseModel) {
		if (!item.id) return;
		return html`
			<uui-ref-node name=${ifDefined(item.name)} detail=${ifDefined(item.id)}>
				${this.#renderIsTrashed(item)}
				<uui-action-bar slot="actions">
					${this.#renderOpenButton(item)}
					<uui-button
						@click=${() => this.#pickerContext.requestRemoveItem(item.id!)}
						label="Remove document ${item.name}"
						>${this.localize.term('general_remove')}</uui-button
					>
				</uui-action-bar>
			</uui-ref-node>
		`;
	}

	#renderIsTrashed(item: DocumentItemResponseModel) {
		if (!item.isTrashed) return;
		return html`<uui-tag size="s" slot="tag" color="danger">Trashed</uui-tag>`;
	}

	#renderOpenButton(item: DocumentItemResponseModel) {
		if (!this.showOpenButton) return;
		return html`<uui-button @click=${() => this.#openItem(item)} label="Open document ${item.name}"
			>${this.localize.term('general_open')}</uui-button
		>`;
	}

	static styles = [
		css`
			#add-button {
				width: 100%;
			}
		`,
	];
}

export default UmbInputDocumentElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-document': UmbInputDocumentElement;
	}
}
