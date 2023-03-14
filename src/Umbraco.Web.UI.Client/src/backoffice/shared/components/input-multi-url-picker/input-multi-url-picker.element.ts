import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { UUIModalSidebarSize } from '@umbraco-ui/uui-modal-sidebar';
import { UmbLinkPickerLink, UMB_LINK_PICKER_MODAL_TOKEN } from '../../modals/link-picker';
import { UmbLitElement } from '@umbraco-cms/element';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/modal';

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
	 * @type {Array<UmbLinkPickerLink>}
	 * @default []
	 */
	@property({ attribute: false })
	set urls(data: Array<UmbLinkPickerLink>) {
		if (!data) return;
		this._urls = data;
		super.value = this._urls.map((x) => x.url).join(',');
	}

	get urls() {
		return this._urls;
	}

	private _urls: Array<UmbLinkPickerLink> = [];
	private _modalContext?: UmbModalContext;

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

		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	private _removeItem(index: number) {
		this.urls.splice(index, 1);
		this._dispatchChangeEvent();
	}

	private _setSelection(selection: UmbLinkPickerLink, index?: number) {
		if (index !== undefined && index >= 0) this.urls[index] = selection;
		else this.urls.push(selection);

		this._dispatchChangeEvent();
	}

	private _dispatchChangeEvent() {
		this.requestUpdate();
		this.dispatchEvent(new CustomEvent('change', { composed: true, bubbles: true }));
	}

	private _openPicker(data?: UmbLinkPickerLink, index?: number) {
		const modalHandler = this._modalContext?.open(UMB_LINK_PICKER_MODAL_TOKEN, {
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
		modalHandler?.onSubmit().then((newUrl: UmbLinkPickerLink) => {
			if (!newUrl) return;

			this._setSelection(newUrl, index);
		});
	}

	render() {
		return html`${this.urls?.map((link, index) => this._renderItem(link, index))}
			<uui-button look="placeholder" label="Add" @click=${this._openPicker}>Add</uui-button>`;
	}

	private _renderItem(link: UmbLinkPickerLink, index: number) {
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
