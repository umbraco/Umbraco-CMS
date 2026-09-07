import type { UmbLinkPickerLink } from '../../link-picker-modal/types.js';
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
		const previous = this.#link;
		this.#link = value;
		this.requestUpdate('link', previous);
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

	@state()
	protected _resolvedName?: string;

	@state()
	protected _resolvedUrl?: string;

	// What is looked up for a link is derived from its unique, so it is only looked up once. A unique is
	// marked before its lookup starts, so re-assigning the same link — which happens on every render of
	// the list this element sits in, a re-order included — neither fires a second request nor waits for
	// one, and un-marked again when nothing came back, so a lookup that failed can be retried.
	#requestedName?: string;
	#requestedUrl?: string;

	async #resolveName() {
		const link = this.#link;
		const unique = link?.unique;
		if (!unique || link.name || unique === this.#requestedName) return;

		this.#requestedName = unique;

		const name = await this._requestName(unique);
		if (this.#requestedName !== unique) return;

		if (!name) {
			this.#requestedName = undefined;
			return;
		}

		this._resolvedName = name;
	}

	async #resolveUrl() {
		const link = this.#link;
		const unique = link?.unique;
		// A link picked for a specific culture carries the URL of that culture already.
		if (!unique || link.culture || unique === this.#requestedUrl) return;

		this.#requestedUrl = unique;

		const url = await this._requestUrl(unique);
		if (this.#requestedUrl !== unique) return;

		if (!url) {
			this.#requestedUrl = undefined;
			return;
		}

		this._resolvedUrl = url;
	}

	/**
	 * Requests the name of the item this link points at. Resolves to nothing unless a link type that
	 * has a name to look up implements it.
	 * @param {string} _unique The unique of the item this link points at.
	 * @returns {Promise<string | undefined>} The name of the item, or undefined when it has none.
	 */
	protected async _requestName(_unique: string): Promise<string | undefined> {
		return undefined;
	}

	/**
	 * Requests the URL of the item this link points at. Resolves to nothing unless a link type that
	 * has a URL to look up implements it.
	 * @param {string} _unique The unique of the item this link points at.
	 * @returns {Promise<string | undefined>} The URL of the item, or undefined when it has none.
	 */
	protected async _requestUrl(_unique: string): Promise<string | undefined> {
		return undefined;
	}

	override render() {
		const link = this.#link;
		if (!link) return nothing;

		const name = this.displayName;
		const url = (this._resolvedUrl ?? link.url ?? '') + (link.queryString || '');

		return html`
			<uui-ref-node
				name=${name || url}
				detail=${ifDefined(name ? url : undefined)}
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
