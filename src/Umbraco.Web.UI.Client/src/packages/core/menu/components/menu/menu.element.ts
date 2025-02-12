import type { ManifestMenu } from '../../menu.extension.js';
import type { ManifestMenuItem } from '../../menu-item.extension.js';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import '../menu-item/menu-item-default.element.js';

@customElement('umb-menu')
export class UmbMenuElement extends UmbLitElement {
	@property({ attribute: false })
	manifest?: ManifestMenu;

	constructor() {
		super();
		//this.provideContext(UMB_MENU_CONTEXT, new UmbMenuContext());
	}

	override render() {
		return html`
			<umb-extension-slot
				type="menuItem"
				default-element="umb-menu-item-default"
				.filter=${(items: ManifestMenuItem) => items.meta.menus.includes(this.manifest!.alias)}>
			</umb-extension-slot>
		`;
	}
}

export default UmbMenuElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-menu': UmbMenuElement;
	}
}
