import { UmbMediaPickerContext } from './input-media.context.js';
import { css, html, customElement, property, state, ifDefined, repeat } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { MediaItemResponseModel, MediaTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';

@customElement('umb-input-media')
export class UmbInputMediaElement extends FormControlMixin(UmbLitElement) {
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
	private _items?: Array<MediaItemResponseModel>;

	#pickerContext = new UmbMediaPickerContext(this);

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

	#pickableFilter: (item: MediaItemResponseModel) => boolean = (item) => {
		/* TODO: Media item doesn't have the content/media-type ID available to query.
			 Commenting out until the Management API model is updated. [LK]
		*/
		// if (this.allowedContentTypeIds && this.allowedContentTypeIds.length > 0) {
		// 	return this.allowedContentTypeIds.includes(item.contentTypeId);
		// }
		return true;
	};

	#openPicker() {
		// TODO: Configure the media picker, with `allowedContentTypeIds` and `ignoreUserStartNodes` [LK]
		console.log('#openPicker', [this.allowedContentTypeIds, this.ignoreUserStartNodes]);
		this.#pickerContext.openPicker({
			hideTreeRoot: true,
			pickableFilter: this.#pickableFilter,
		});
	}

	#openItem(item: MediaItemResponseModel) {
		// TODO: Implement the Media editing infinity editor. [LK]
		console.log('TODO: _openItem', item);
	}

	render() {
		return html` ${this.#renderItems()} ${this.#renderAddButton()} `;
	}

	#renderItems() {
		if (!this._items) return;
		// TODO: Add sorting. [LK]
		return html`
			${repeat(
				this._items,
				(item) => item.id,
				(item) => this.#renderItem(item),
			)}
		`;
	}

	#renderAddButton() {
		if (this._items && this.max && this._items.length >= this.max) return;
		return html`
			<uui-button
				id="add-button"
				look="placeholder"
				@click=${this.#openPicker}
				label=${this.localize.term('general_choose')}>
				<uui-icon name="icon-add"></uui-icon>
				${this.localize.term('general_choose')}
			</uui-button>
		`;
	}

	#renderItem(item: MediaItemResponseModel) {
		// TODO: `file-ext` value has been hardcoded here. Find out if API model has value for it. [LK]
		// TODO: How to handle the `showOpenButton` option? [LK]
		return html`
			<uui-card-media
				name=${ifDefined(item.name === null ? undefined : item.name)}
				detail=${ifDefined(item.id)}
				file-ext="jpg">
				${this.#renderIsTrashed(item)}
				<uui-action-bar slot="actions">
					<uui-button label="Copy media">
						<uui-icon name="icon-documents"></uui-icon>
					</uui-button>
					<uui-button @click=${() => this.#pickerContext.requestRemoveItem(item.id!)} label="Remove media ${item.name}">
						<uui-icon name="icon-trash"></uui-icon>
					</uui-button>
				</uui-action-bar>
			</uui-card-media>
		`;
	}

	#renderIsTrashed(item: MediaItemResponseModel) {
		if (!item.isTrashed) return;
		return html`<uui-tag size="s" slot="tag" color="danger">Trashed</uui-tag>`;
	}

	static styles = [
		css`
			:host {
				display: grid;
				gap: var(--uui-size-space-3);
				grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
				grid-template-rows: repeat(auto-fill, minmax(160px, 1fr));
			}

			#add-button {
				text-align: center;
				height: 100%;
			}

			uui-icon {
				display: block;
				margin: 0 auto;
			}
		`,
	];
}

export default UmbInputMediaElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-media': UmbInputMediaElement;
	}
}
