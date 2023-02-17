import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { UUIModalSidebarSize } from '@umbraco-ui/uui-modal-sidebar';
import { UmbLitElement } from '@umbraco-cms/element';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/modal';

export interface MultiUrlData {
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
	minMessage = 'This field needs more items';

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
	overlaySize?: UUIModalSidebarSize;

	@property({ attribute: 'urls' })
	multiUrls: Array<MultiUrlData> = [];

	private _modalService?: UmbModalService;

	constructor() {
		super();
		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() => !!this.min && this.multiUrls.length < this.min
		);
		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !!this.max && this.multiUrls.length > this.max
		);

		this.consumeContext(UMB_MODAL_SERVICE_CONTEXT_TOKEN, (instance) => {
			this._modalService = instance;
		});
	}

	private _removeItem(index: number) {
		this.multiUrls.splice(index, 1);
		this.requestUpdate();
	}

	private _openPicker(data?: MultiUrlData, index?: number) {
		const modalHandler = this._modalService?.linkPicker({
			link: {
				name: data?.name,
				published: data?.published,
				queryString: data?.queryString,
				target: data?.target,
				trashed: data?.trashed,
				udi: data?.udi,
				url: data?.url,
			},
			config: {
				hideAnchor: this.hideAnchor,
				ignoreUserStartNodes: this.ignoreUserStartNodes,
				overlaySize: this.overlaySize || 'small',
			},
		});
		modalHandler?.onClose().then((newUrl: MultiUrlData) => {
			if (!newUrl) return;

			if (index !== undefined && index >= 0) this.multiUrls[index] = newUrl;
			else this.multiUrls.push(newUrl);

			this.requestUpdate();
			//TODO: onChange event?
		});
	}

	render() {
		return html`${this.multiUrls?.map((link, index) => this._renderItem(link, index))}
			<uui-button look="placeholder" label="Add" @click=${this._openPicker}>Add</uui-button>`;
	}

	private _renderItem(link: MultiUrlData, index: number) {
		return html`<uui-ref-node .name="${link.name || ''}" .detail="${(link.url || '') + (link.queryString || '')}">
			<uui-icon slot="icon" name="${link.icon || 'umb:link'}"></uui-icon>
			<uui-action-bar slot="actions">
				<uui-button @click="${() => this._openPicker(link, index)}" label="Edit link">Edit</uui-button>
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
