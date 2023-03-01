import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { ManifestMenu, ManifestMenuItem } from '@umbraco-cms/extensions-registry';
import { UmbLitElement } from '@umbraco-cms/element';

import '../menu-item/menu-item.element';

@customElement('umb-menu')
export class UmbMenuElement extends UmbLitElement {
	static styles = [UUITextStyles];

	@property()
	manifest?: ManifestMenu;

	render() {
		return html` <umb-extension-slot
			type="menuItem"
			.filter=${(items: ManifestMenuItem) => items.meta.menus.includes(this.manifest!.alias)}
			default-element="umb-menu-item"></umb-extension-slot>`;
	}
}

export default UmbMenuElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-menu': UmbMenuElement;
	}
}
