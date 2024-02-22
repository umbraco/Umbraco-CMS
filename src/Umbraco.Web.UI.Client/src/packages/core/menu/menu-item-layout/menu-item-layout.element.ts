import { html, customElement, property, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

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

	render() {
		return html`<uui-menu-item href="${ifDefined(this.href)}" label=${this.label} ?has-children=${this.hasChildren}>
			<uui-icon slot="icon" name=${this.iconName}></uui-icon>
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

	static styles = [UmbTextStyles];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-menu-item-layout': UmbMenuItemLayoutElement;
	}
}
