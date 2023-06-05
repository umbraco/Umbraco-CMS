import { UmbMediaRepository } from '../../repository/media.repository.js';
import { css, html, nothing, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles, FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import {
	UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_CONFIRM_MODAL,
	UMB_MEDIA_TREE_PICKER_MODAL,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-input-media-picker')
export class UmbInputMediaPickerElement extends FormControlMixin(UmbLitElement) {
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

	// TODO: do we need both selectedIds and value? If we just use value we follow the same pattern as native form controls.
	private _selectedIds: Array<string> = [];
	public get selectedIds(): Array<string> {
		return this._selectedIds;
	}
	public set selectedIds(ids: Array<string>) {
		this._selectedIds = ids;
		super.value = ids.join(',');
		this._observePickedMedias();
	}

	@property()
	public set value(idsString: string) {
		if (idsString !== this._value) {
			this.selectedIds = idsString.split(/[ ,]+/);
		}
	}

	@state()
	private _items?: Array<EntityTreeItemResponseModel>;

	private _modalContext?: UmbModalManagerContext;
	private _pickedItemsObserver?: UmbObserverController<EntityTreeItemResponseModel[]>;
	private _repository = new UmbMediaRepository(this);

	constructor() {
		super();

		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() => !!this.min && this._selectedIds.length < this.min
		);
		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !!this.max && this._selectedIds.length > this.max
		);

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	connectedCallback(): void {
		super.connectedCallback();
		this._observePickedMedias();
	}

	protected getFormElement() {
		return undefined;
	}

	private async _observePickedMedias() {
		this._pickedItemsObserver?.destroy();

		// TODO: consider changing this to the list data endpoint when it is available
		const { asObservable } = await this._repository.requestItemsLegacy(this._selectedIds);

		if (!asObservable) return;

		this._pickedItemsObserver = this.observe(asObservable(), (items) => {
			this._items = items;
		});
	}

	private _openPicker() {
		// We send a shallow copy(good enough as its just an array of ids) of our this._selectedIds, as we don't want the modal to manipulate our data:
		const modalHandler = this._modalContext?.open(UMB_MEDIA_TREE_PICKER_MODAL, {
			multiple: this.max === 1 ? false : true,
			selection: [...this._selectedIds],
		});

		modalHandler?.onSubmit().then(({ selection }: any) => {
			this._setSelection(selection);
		});
	}

	private _removeItem(item: EntityTreeItemResponseModel) {
		const modalHandler = this._modalContext?.open(UMB_CONFIRM_MODAL, {
			color: 'danger',
			headline: `Remove ${item.name}?`,
			content: 'Are you sure you want to remove this item',
			confirmLabel: 'Remove',
		});

		modalHandler?.onSubmit().then(() => {
			const newSelection = this._selectedIds.filter((value) => value !== item.id);
			this._setSelection(newSelection);
		});
	}

	private _setSelection(newSelection: Array<string>) {
		this.selectedIds = newSelection;
		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true }));
	}

	render() {
		return html` ${this._items?.map((item) => this._renderItem(item))} ${this._renderButton()} `;
	}
	private _renderButton() {
		if (this._items && this.max && this._items.length >= this.max) return;
		return html`<uui-button id="add-button" look="placeholder" @click=${this._openPicker} label="open">
			<uui-icon name="umb:add"></uui-icon>
			Add
		</uui-button>`;
	}

	private _renderItem(item: EntityTreeItemResponseModel) {
		// TODO: remove when we have a way to handle trashed items
		const tempItem = item as EntityTreeItemResponseModel & { isTrashed: boolean };

		return html`
			<uui-card-media
				name=${ifDefined(item.name === null ? undefined : item.name)}
				detail=${ifDefined(item.id)}
				file-ext="jpg">
				${tempItem.isTrashed ? html` <uui-tag size="s" slot="tag" color="danger">Trashed</uui-tag> ` : nothing}
				<uui-action-bar slot="actions">
					<uui-button label="Copy media">
						<uui-icon name="umb:documents"></uui-icon>
					</uui-button>
					<uui-button @click=${() => this._removeItem(item)} label="Remove media ${item.name}">
						<uui-icon name="umb:trash"></uui-icon>
					</uui-button>
				</uui-action-bar>
			</uui-card-media>
		`;
		//TODO: <uui-button-inline-create vertical></uui-button-inline-create>
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: grid;
				gap: var(--uui-size-space-3);
				grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
			}
			#add-button {
				text-align: center;
				height: 160px;
			}

			uui-icon {
				display: block;
				margin: 0 auto;
			}
		`,
	];
}

export default UmbInputMediaPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-media-picker': UmbInputMediaPickerElement;
	}
}
