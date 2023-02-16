import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { UmbLitElement } from '@umbraco-cms/element';
import { DocumentTreeItemModel, EntityTreeItemModel, FolderTreeItemModel } from '@umbraco-cms/backend-api';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/modal';
import { UmbObserverController } from '@umbraco-cms/observable-api';

export interface Link {
	icon?: string;
	name?: string;
	published?: boolean;
	queryString?: string;
	target?: string;
	trashed?: boolean;
	udi?: string;
	url?: string;
}

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

	@property()
	ignoreUserStartNodes?: boolean;

	/**
	 * @type {"small" | "medium" | "large"}
	 * @attr
	 * @default "small"
	 */
	@property()
	overlaySize?: 'small' | 'medium' | 'large' | 'full';

	@property()
	links: Array<Link> = [];

	private _modalService?: UmbModalService;
	private _pickedItemsObserver?: UmbObserverController<FolderTreeItemModel>;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_SERVICE_CONTEXT_TOKEN, (instance) => {
			this._modalService = instance;
		});
	}

	private async _observePickedDocumentsOrMedias() {
		this._pickedItemsObserver?.destroy();
	}

	private _removeItem(index: number) {
		this.links.splice(index, 1);
		this.requestUpdate();
	}

	private _openPicker(data?: Link, index?: number) {
		const modalHandler = this._modalService?.linkPicker(
			{
				name: data?.name || undefined,
				published: data?.published || undefined,
				queryString: data?.queryString || undefined,
				target: data?.target || undefined,
				trashed: data?.trashed || undefined,
				udi: data?.udi || undefined,
				url: data?.url || undefined,
			},
			{
				hideAnchor: this.hideAnchor,
				ignoreUserStartNodes: this.ignoreUserStartNodes,
				overlaySize: this.overlaySize || 'small',
			}
		);
		modalHandler?.onClose().then((newUrl: Link) => {
			if (!newUrl) return;

			if (index !== undefined && index >= 0) this.links[index] = newUrl;
			else this.links.push(newUrl);
			this.requestUpdate();
		});
	}

	render() {
		return html`${this.links?.map((link, index) => this._renderItem(link, index))}
			<uui-button look="placeholder" label="Add" @click=${this._openPicker}>Add</uui-button>`;
	}

	private _renderItem(link: Link, index: number) {
		return html`<uui-ref-node
			@open="${() => this._openPicker(link, index)}"
			.name="${link.name || ''}"
			.detail="${(link.url || '') + (link.queryString || '')}">
			<uui-icon slot="icon" name="${link.icon || 'umb:link'}"></uui-icon>
			<uui-action-bar slot="actions">
				<uui-button @click="${() => this._removeItem(index)}" label="Remove link">Remove</uui-button>
			</uui-action-bar>
		</uui-ref-node>`;
	}
}

export default UmbInputMultiUrlPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-multi-url-picker': UmbInputMultiUrlPickerElement;
	}
}
