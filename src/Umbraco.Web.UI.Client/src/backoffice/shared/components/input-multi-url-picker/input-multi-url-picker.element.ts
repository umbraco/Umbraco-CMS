import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { UmbLitElement } from '@umbraco-cms/element';
import { DocumentTreeItemModel, FolderTreeItemModel } from '@umbraco-cms/backend-api';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/modal';

export type OverlaySize = 'small' | 'medium' | 'large';

@customElement('umb-input-multi-url-picker')
export class UmbInputMultiUrlPickerElement extends FormControlMixin(UmbLitElement) {
	static styles = [
		UUITextStyles,
		css`
			uui-button {
				width: 100%;
			}
		`,
	];

	protected getFormElement() {
		return undefined;
	}
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
	 @attr 'hide-anchor'
	 */
	@property({ type: Boolean, attribute: 'hide-anchor' })
	hideAnchor?: boolean;

	/**
	 * @type {"small" | "medium" | "large"}
	 * @attr
	 * @default "small"
	 */
	@property()
	overlaySize: OverlaySize = 'small';

	// TODO: do we need both selectedKeys and value? If we just use value we follow the same pattern as native form controls.
	private _selectedKeys: Array<string> = [];
	public get selectedKeys(): Array<string> {
		return this._selectedKeys;
	}
	public set selectedKeys(keys: Array<string>) {
		this._selectedKeys = keys;
		super.value = keys.join(',');
	}

	@property()
	public set value(keysString: string) {
		if (keysString !== this._value) {
			this.selectedKeys = keysString.split(/[ ,]+/);
		}
	}

	@state()
	private _items?: Array<DocumentTreeItemModel>;

	private _modalService?: UmbModalService;

	constructor() {
		super();

		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() => !!this.min && this._selectedKeys.length < this.min
		);
		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !!this.max && this._selectedKeys.length > this.max
		);

		this.consumeContext(UMB_MODAL_SERVICE_CONTEXT_TOKEN, (instance) => {
			this._modalService = instance;
		});
	}

	private _openPicker() {
		const modalHandler = this._modalService?.multiUrlPicker();
		modalHandler?.onClose().then(({ selection }: any) => {
			//this._setSelection(selection);
			console.log(selection);
		});
	}

	render() {
		return html`${this._items?.map((item) => this._renderItem(item))}
			<uui-button look="placeholder" label="Add" @click=${this._openPicker}>Add</uui-button>`;
	}

	private _renderItem(item: FolderTreeItemModel) {
		// TODO: remove when we have a way to handle trashed items
		const tempItem = item as FolderTreeItemModel & { isTrashed: boolean };

		return html`
			<uui-ref-node name=${ifDefined(item.name === null ? undefined : item.name)} detail=${ifDefined(item.key)}>
				${tempItem.isTrashed ? html` <uui-tag size="s" slot="tag" color="danger">Trashed</uui-tag> ` : nothing}
				<uui-action-bar slot="actions"> </uui-action-bar>
			</uui-ref-node>
		`;
	}
}

export default UmbInputMultiUrlPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-multi-url-picker': UmbInputMultiUrlPickerElement;
	}
}
