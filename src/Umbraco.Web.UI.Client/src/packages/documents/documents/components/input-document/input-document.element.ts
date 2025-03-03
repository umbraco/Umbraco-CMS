import { UmbDocumentPickerInputContext } from './input-document.context.js';
import { css, customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbDocumentItemModel } from '@umbraco-cms/backoffice/document';
import type { UmbTreeStartNode } from '@umbraco-cms/backoffice/tree';
import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '@umbraco-cms/backoffice/document-type';
import type { UmbSortedEvent } from '@umbraco-cms/backoffice/sorter';
import type { UmbEntityItemRefListElement } from '@umbraco-cms/backoffice/entity-item';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-input-document')
export class UmbInputDocumentElement extends UmbFormControlMixin<string | undefined, typeof UmbLitElement>(
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
	@property({ type: String, attribute: 'max-message' })
	maxMessage = 'This field exceeds the allowed amount of items';

	public set selection(ids: Array<string>) {
		this.#pickerContext.setSelection(ids);
	}
	public get selection(): Array<string> {
		return this.#pickerContext.getSelection();
	}

	@property({ type: Object, attribute: false })
	startNode?: UmbTreeStartNode;

	@property({ type: Array })
	allowedContentTypeIds?: string[] | undefined;

	@property({ type: Boolean })
	showOpenButton?: boolean;

	@property({ type: String })
	public override set value(selectionString: string | undefined) {
		this.selection = splitStringToArray(selectionString);
	}
	public override get value(): string | undefined {
		return this.selection.length > 0 ? this.selection.join(',') : undefined;
	}

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	@state()
	private _items?: Array<UmbDocumentItemModel>;

	#pickerContext = new UmbDocumentPickerInputContext(this);

	constructor() {
		super();

		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() => !!this.min && this.selection.length < this.min,
		);

		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !!this.max && this.selection.length > this.max,
		);

		this.observe(this.#pickerContext.selection, (selection) => (this.value = selection.join(',')), '_observeSelection');
		this.observe(this.#pickerContext.selectedItems, (selectedItems) => (this._items = selectedItems), '_observerItems');
	}

	#openPicker() {
		this.#pickerContext.openPicker(
			{
				hideTreeRoot: true,
				startNode: this.startNode,
			},
			{
				allowedContentTypes: this.allowedContentTypeIds?.map((id) => ({
					unique: id,
					entityType: UMB_DOCUMENT_TYPE_ENTITY_TYPE,
				})),
			},
		);
	}

	#onRemove(item: UmbDocumentItemModel) {
		this.#pickerContext.requestRemoveItem(item.unique);
	}

	#onRefListChange(event: UmbSortedEvent) {
		event.stopPropagation();
		const target = event.target as UmbEntityItemRefListElement;
		const uniques = target.items?.map((item) => item.unique) ?? [];
		this.selection = uniques;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`${this.#renderItems()} ${this.#renderAddButton()}`;
	}

	#renderAddButton() {
		if (this.selection.length >= this.max) return nothing;
		if (this.readonly && this.selection.length > 0) {
			return nothing;
		} else {
			return html`
				<uui-button
					id="btn-add"
					look="placeholder"
					@click=${this.#openPicker}
					label=${this.localize.term('general_choose')}
					?disabled=${this.readonly}></uui-button>
			`;
		}
	}

	#renderItems() {
		if (!this._items) return;
		return html`
			<umb-entity-item-ref-list
				.items=${this._items}
				?readonly=${this.readonly}
				?standalone=${this.max === 1}
				sortable
				@change=${this.#onRefListChange}></umb-entity-item-ref-list>
		`;
	}

	static override styles = [
		css`
			#btn-add {
				display: block;
			}
		`,
	];
}

export { UmbInputDocumentElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-document': UmbInputDocumentElement;
	}
}
