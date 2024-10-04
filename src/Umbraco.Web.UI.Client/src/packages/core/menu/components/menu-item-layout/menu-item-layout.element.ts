import { html, customElement, property, ifDefined, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { debounce } from '@umbraco-cms/backoffice/utils';

/**
 * @element umb-menu-item-layout
 * @description
 * A menu item layout to render he backoffice menu item look.
 * This supports nested menu items, and if a `entityType` is provided, it will render the entity actions for it.
 */
@customElement('umb-menu-item-layout')
export class UmbMenuItemLayoutElement extends UmbLitElement {
	@property({ type: String, attribute: 'entity-type' })
	public entityType?: string;

	/**
	 * The icon name for the icon to show in this menu item.
	 */
	@property({ type: String, attribute: 'icon-name' })
	public iconName = '';

	/**
	 * The label for this menu item.
	 */
	@property({ type: String })
	public label = '';

	/**
	 * Declare if this item has children, this will show the expand symbol.
	 */
	@property({ type: Boolean, attribute: 'has-children' })
	public hasChildren = false;

	/**
	 * Define a href for this menu item.
	 */
	@property({ type: String })
	public href?: string;

	/**
	 * Set an anchor tag target, only used when using href.
	 * @type {string}
	 * @attr
	 * @default undefined
	 */
	@property({ type: String })
	public target?: '_blank' | '_parent' | '_self' | '_top';

	@state()
	private _isActive = false;

	override connectedCallback() {
		super.connectedCallback();
		window.addEventListener('navigationend', this.#debouncedCheckIsActive);
	}

	#debouncedCheckIsActive = debounce(() => this.#checkIsActive(), 100);

	#checkIsActive() {
		if (!this.href) {
			this._isActive = false;
			return;
		}

		const location = window.location.pathname;
		this._isActive = location.includes(this.href);
	}

	override render() {
		return html`<uui-menu-item
			href="${ifDefined(this.href)}"
			label=${this.label}
			.caretLabel=${this.localize.term('visuallyHiddenTexts_expandChildItems') + ' ' + this.label}
			?active=${this._isActive}
			?has-children=${this.hasChildren}
			target=${ifDefined(this.href && this.target ? this.target : undefined)}>
			<umb-icon slot="icon" name=${this.iconName}></umb-icon>
			${this.entityType
				? html`<umb-entity-actions-bundle
						slot="actions"
						.entityType=${this.entityType}
						.unique=${null}
						.label=${this.label}>
					</umb-entity-actions-bundle>`
				: ''}
			<slot></slot>
		</uui-menu-item>`;
	}

	override disconnectedCallback() {
		super.disconnectedCallback();
		window.removeEventListener('navigationend', this.#debouncedCheckIsActive);
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-menu-item-layout': UmbMenuItemLayoutElement;
	}
}
