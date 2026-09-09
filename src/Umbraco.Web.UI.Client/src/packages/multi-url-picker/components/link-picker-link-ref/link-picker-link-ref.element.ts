import type { UmbLinkPickerLink } from '../../link-picker-modal/types.js';
import { UMB_LINK_PICKER_LINK_REF_ATTRIBUTE } from './constants.js';
import type { UmbLinkPickerLinkRefLookup } from './types.js';
import { css, customElement, html, ifDefined, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * Renders a picked link as a reference node.
 *
 * A link that carries everything it displays needs no lookup and is rendered by this element as it is.
 * A link type that points at an item whose name or URL has to be looked up extends this element and
 * implements the lookups; each element holds on to what it resolved, so the list it sits in can render
 * and re-order again without the same information being requested twice.
 * @element umb-link-picker-link-ref
 * @attr name - The name this link is displayed under. Set by the element, not by its consumer.
 * @slot actions - The actions available for this link.
 */
@customElement('umb-link-picker-link-ref')
export class UmbLinkPickerLinkRefElement extends UmbLitElement {
	#link?: UmbLinkPickerLink;

	/**
	 * The link to render.
	 * @type {(UmbLinkPickerLink | undefined)}
	 */
	@property({ type: Object, attribute: false })
	public set link(value: UmbLinkPickerLink | undefined) {
		this.#link = value;
		this.#resolveName();
		this.#resolveUrl();
	}
	public get link(): UmbLinkPickerLink | undefined {
		return this.#link;
	}

	/**
	 * The location this link can be edited at, if it can be edited at all.
	 * @type {(string | undefined)}
	 * @attr
	 */
	@property({ type: String })
	href?: string;

	/**
	 * Renders the link without its actions and without a link to edit it.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	/**
	 * Renders the link as the only one of its list.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	standalone = false;

	/**
	 * The name this link is displayed under, which is the name it carries itself where it has one and
	 * the resolved name of the item it points at otherwise.
	 * @returns {string} The name shown for this link, or an empty string while it is still unknown.
	 */
	public get displayName(): string {
		return this.#link?.name || this._resolvedName || '';
	}

	/**
	 * The URL this link is displayed with, which is the URL of the item it points at where that had to
	 * be resolved and the one it carries itself otherwise, plus any query string.
	 * @returns {string} The URL shown for this link, or an empty string while it is still unknown.
	 */
	public get displayUrl(): string {
		const link = this.#link;
		if (!link) return '';
		return (this._resolvedUrl ?? link.url ?? '') + (link.queryString || '');
	}

	// A link falls back to showing its URL until it has a name to show.
	get #label(): string {
		return this.displayName || this.displayUrl;
	}

	@state()
	protected _resolvedName?: string;

	@state()
	protected _resolvedUrl?: string;

	// What is looked up for a link is derived from its unique, so it is only looked up once per unique.
	// A unique is marked before its lookup starts, so re-assigning the same link — which happens on
	// every render of the list this element sits in, a re-order included — neither fires a second
	// request nor waits for one. It is un-marked again only when the lookup could not be performed, so
	// that one can be retried; a lookup that came back with nothing has answered and is not repeated.
	#requestedName?: string;
	#requestedUrl?: string;

	async #resolveName() {
		const link = this.#link;
		const unique = link?.unique;
		if (!unique || link.name || unique === this.#requestedName) return;

		// Reaching here means this is a unique that has not been looked up, so anything resolved for the
		// one before it no longer describes this link.
		this._resolvedName = undefined;
		this.#requestedName = unique;

		const { value, error } = await this._requestName(unique);
		if (this.#requestedName !== unique) return;

		if (error) {
			this.#requestedName = undefined;
			return;
		}

		this._resolvedName = value;
	}

	async #resolveUrl() {
		const link = this.#link;
		const unique = link?.unique;
		// A link picked for a specific culture carries the URL of that culture already.
		if (!unique || link.culture || unique === this.#requestedUrl) return;

		this._resolvedUrl = undefined;
		this.#requestedUrl = unique;

		const { value, error } = await this._requestUrl(unique);
		if (this.#requestedUrl !== unique) return;

		if (error) {
			this.#requestedUrl = undefined;
			return;
		}

		this._resolvedUrl = value;
	}

	/**
	 * Requests the name of the item this link points at. Resolves to nothing unless a link type that
	 * has a name to look up implements it.
	 * @param {string} _unique The unique of the item this link points at.
	 * @returns {Promise<UmbLinkPickerLinkRefLookup>} The name of the item, or the error that stopped
	 * it from being looked up.
	 */
	protected async _requestName(_unique: string): Promise<UmbLinkPickerLinkRefLookup> {
		return {};
	}

	/**
	 * Requests the URL of the item this link points at. Resolves to nothing unless a link type that
	 * has a URL to look up implements it.
	 * @param {string} _unique The unique of the item this link points at.
	 * @returns {Promise<UmbLinkPickerLinkRefLookup>} The URL of the item, or the error that stopped it
	 * from being looked up.
	 */
	protected async _requestUrl(_unique: string): Promise<UmbLinkPickerLinkRefLookup> {
		return {};
	}

	// Marks this element as one of the link refs, so a consumer that addresses the rendered links as a
	// group — the sorter of the list they sit in, for instance — reaches every link type, including the
	// ones added after it. Set on connect rather than on first render, so it is already there when an
	// observer of the list is notified of this element being inserted.
	override connectedCallback() {
		super.connectedCallback();
		this.toggleAttribute(UMB_LINK_PICKER_LINK_REF_ATTRIBUTE, true);
	}

	// The name is reflected onto this element rather than left on the ref node it renders, so that the
	// element identified by the name is also the one the actions are slotted into — which is what a
	// consumer addressing a link by its name and reaching for one of its actions relies on.
	protected override updated(changedProperties: Map<PropertyKey, unknown>) {
		super.updated(changedProperties);
		this.setAttribute('name', this.#label);
	}

	override render() {
		const link = this.#link;
		if (!link) return nothing;

		const name = this.displayName;

		return html`
			<uui-ref-node
				.name=${this.#label}
				detail=${ifDefined(name ? this.displayUrl : undefined)}
				href=${ifDefined(this.href)}
				?readonly=${this.readonly}
				?standalone=${this.standalone}>
				<umb-icon slot="icon" name=${link.icon || 'icon-link'}></umb-icon>
				<slot name="actions" slot="actions"></slot>
			</uui-ref-node>
		`;
	}

	static override styles = [
		css`
			/* A ref list draws its separators as absolutely positioned pseudo elements on the items it
			   slots, so an item has to be a containing block of its own. */
			:host {
				display: block;
				position: relative;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-link-picker-link-ref': UmbLinkPickerLinkRefElement;
	}
}
