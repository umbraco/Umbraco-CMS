import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { UUIModalSidebarSize } from '@umbraco-ui/uui-modal-sidebar';
import { UmbModalRouteBuilder, UmbRouteContext, UMB_ROUTE_CONTEXT_TOKEN } from '@umbraco-cms/router';
import { UmbLinkPickerLink, UMB_LINK_PICKER_MODAL_TOKEN } from '../../modals/link-picker';
import { UmbLitElement } from '@umbraco-cms/element';

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
		this._urls = [...data]; // Unfreeze data coming from State, so we can manipulate it.
		super.value = this._urls.map((x) => x.url).join(',');
	}

	get urls() {
		return this._urls;
	}

	private _urls: Array<UmbLinkPickerLink> = [];
	//private _modalContext?: UmbModalContext;
	private _routeContext?: UmbRouteContext;

	private _linkPickerURL?: UmbModalRouteBuilder;

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

		/*
		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
		*/

		this.consumeContext(UMB_ROUTE_CONTEXT_TOKEN, (instance) => {
			this._routeContext = instance;

			// Registre the routes of this UI:
			// TODO: To avoid retriving the property alias, we might make use of the property context?
			// Or maybe its not the property-alias, but something unique? as this might not be in a property?.
			this._linkPickerURL = this._routeContext.registerModal(UMB_LINK_PICKER_MODAL_TOKEN, {
				path: `${'to-do-myPropertyAlias'}/:index`,
				onSetup: (routingInfo) => {
					// Get index from routeInfo:
					const indexParam = routingInfo.match.params.index;
					if (!indexParam) return false;
					let index: number | null = parseInt(routingInfo.match.params.index);
					if (Number.isNaN(index)) return false;

					// Use the index to find data:
					console.log('onSetup modal index:', index);

					let data: UmbLinkPickerLink | null = null;
					if (index >= 0 && index < this.urls.length) {
						data = this._getItemByIndex(index);
					} else {
						index = null;
					}

					console.log('onSetup modal got data:', data);

					const modalData = {
						index: index,
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
					};
					return modalData;
				},
				onSubmit: (submitData) => {
					console.log('On submit in property editor input');
					if (!submitData) return;
					this._setSelection(submitData.link, submitData.index);
				},
				onReject: () => {
					console.log('User cancelled dialog.');
				},
			});
		});
	}

	private _removeItem(index: number) {
		this.urls.splice(index, 1);
		this._dispatchChangeEvent();
	}

	private _getItemByIndex(index: number) {
		return this.urls[index];
	}

	private _setSelection(selection: UmbLinkPickerLink, index: number | null) {
		if (index !== null && index >= 0) {
			this.urls[index] = selection;
		} else {
			this.urls.push(selection);
		}

		this._dispatchChangeEvent();
	}

	private _dispatchChangeEvent() {
		this.requestUpdate();
		this.dispatchEvent(new CustomEvent('change', { composed: true, bubbles: true }));
	}

	private _openPicker(data?: UmbLinkPickerLink, index?: number) {
		console.log('JS open picker, should fail for now,');
		/*
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
		*/
	}

	render() {
		return html`${this.urls?.map((link, index) => this._renderItem(link, index))}
			<uui-button look="placeholder" label="Add" .href=${this._linkPickerURL?.({ index: -1 })}>Add</uui-button>`;
		// "modal/Umb.Modal.LinkPicker/${'to-do-myPropertyAlias'}/-1"
	}

	private _renderItem(link: UmbLinkPickerLink, index: number) {
		return html`<uui-ref-node
			.name="${link.name || ''}"
			.detail="${(link.url || '') + (link.queryString || '')}"
			@open="${() => this._openPicker(link, index)}">
			<uui-icon slot="icon" name="${link.icon || 'umb:link'}"></uui-icon>
			<uui-action-bar slot="actions">
				<uui-button .href=${this._linkPickerURL?.({ index })} label="Edit link">Edit</uui-button>
				<uui-button @click="${() => this._removeItem(index)}" label="Remove link">Remove</uui-button>
			</uui-action-bar>
		</uui-ref-node>`;
		// "modal/Umb.Modal.LinkPicker/${'to-do-myPropertyAlias'}/${index}"
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-multi-url-picker': UmbInputMultiUrlPickerElement;
	}
}
