import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import type { UUIModalSidebarSize } from '@umbraco-ui/uui';
import type { UmbVariantId } from '../../variants/variant-id.class';
import {
	UmbLinkPickerLink,
	UMB_LINK_PICKER_MODAL,
	UmbModalRouteRegistrationController,
} from '@umbraco-cms/backoffice/modal';
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

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

	@property()
	public set alias(value: string | undefined) {
		this.myModalRegistration.setUniqueIdentifier('propertyAlias', value);
	}

	@property()
	public set variantId(value: string | UmbVariantId | undefined) {
		this.myModalRegistration.setUniqueIdentifier('variantId', value?.toString());
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

	@state()
	private _modalRoute?: UmbModalRouteBuilder;

	private myModalRegistration;

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

		this.myModalRegistration = new UmbModalRouteRegistrationController(
			this,
			UMB_LINK_PICKER_MODAL,
			`:index`,
			new Map([
				['propertyAlias', undefined],
				['variantId', undefined],
			])
		)
			.onSetup((params) => {
				// Get index:
				const indexParam = params.index;
				if (!indexParam) return false;
				let index: number | null = parseInt(params.index);
				if (Number.isNaN(index)) return false;

				// Use the index to find data:
				let data: UmbLinkPickerLink | null = null;
				if (index >= 0 && index < this.urls.length) {
					data = this._getItemByIndex(index);
				} else {
					// If not then make a new pick:
					index = null;
				}

				return {
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
			})
			.onSubmit((submitData) => {
				if (!submitData) return;
				this._setSelection(submitData.link, submitData.index);
			})
			.observeRouteBuilder((routeBuilder) => {
				this._modalRoute = routeBuilder;
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

	// TODO: We should get a href property on uui-ref-node, and not use this method:
	private _temporary_onClick_editItem(index: number) {
		this.myModalRegistration.open({ index });
	}

	render() {
		return html`${this.urls?.map((link, index) => this._renderItem(link, index))}
			<uui-button look="placeholder" label="Add" .href=${this._modalRoute?.({ index: -1 })}>Add</uui-button>`;
	}

	private _renderItem(link: UmbLinkPickerLink, index: number) {
		return html`<uui-ref-node
			.name="${link.name || ''}"
			.detail="${(link.url || '') + (link.queryString || '')}"
			@open="${() => this._temporary_onClick_editItem(index)}">
			<uui-icon slot="icon" name="${link.icon || 'umb:link'}"></uui-icon>
			<uui-action-bar slot="actions">
				<uui-button .href=${this._modalRoute?.({ index })} label="Edit link">Edit</uui-button>
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
