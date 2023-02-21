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

/**
 * @element umb-input-multi-url-picker
 * @fires change - when the value of the input changes
 * @fires blur - when the input loses focus
 * @fires focus - when the input gains focus
 */
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
	 * @type {UUIModalSidebarSize}
	 * @attr
	 * @default "small"
	 */
	@property()
	overlaySize?: UUIModalSidebarSize;

	/**
	 * @type {Array<MultiUrlData>}
	 * @default []
	 */
	@property({ attribute: false })
	set urls(data: Array<MultiUrlData>) {
		this._urls = data;
		super.value = this._urls.map((x) => x.url).join(',');
	}

	get urls() {
		return this._urls;
	}

	private _urls: Array<MultiUrlData> = [];
	private _modalService?: UmbModalService;

	constructor() {
		super();
		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() => !!this.min && this.urls.length < this.min
		);
		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !!this.max && this.urls.length > this.max
		);

		this.consumeContext(UMB_MODAL_SERVICE_CONTEXT_TOKEN, (instance) => {
			this._modalService = instance;
		});
	}

	private _removeItem(index: number) {
		this.urls.splice(index, 1);
		this._dispatchChangeEvent();
	}

	private _setSelection(selection: MultiUrlData, index?: number) {
		if (index !== undefined && index >= 0) this.urls[index] = selection;
		else this.urls.push(selection);

		this._dispatchChangeEvent();
	}

	private _dispatchChangeEvent() {
		this.requestUpdate();
		this.dispatchEvent(new CustomEvent('change', { composed: true, bubbles: true }));
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
			this._setSelection(newUrl, index);
		});
	}

	render() {
		return html`${this.urls?.map((link, index) => this._renderItem(link, index))}
			<uui-button look="placeholder" label="Add" @click=${this._openPicker}>Add</uui-button>`;
	}

	private _renderItem(link: MultiUrlData, index: number) {
		return html`<uui-ref-node
			.name="${link.name || ''}"
			.detail="${(link.url || '') + (link.queryString || '')}"
			@open="${() => this._openPicker(link, index)}">
			<uui-icon slot="icon" name="${link.icon || 'umb:link'}"></uui-icon>
			<uui-action-bar slot="actions">
				<uui-button @click="${() => this._openPicker(link, index)}" label="Edit link">Edit</uui-button>
				<uui-button @click="${() => this._removeItem(index)}" label="Remove link">Remove</uui-button>
			</uui-action-bar>
		</uui-ref-node>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-multi-url-picker': UmbInputMultiUrlPickerElement;
	}
}
