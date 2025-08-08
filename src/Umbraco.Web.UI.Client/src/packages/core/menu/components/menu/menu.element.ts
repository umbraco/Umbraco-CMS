import type { ManifestMenu } from '../../menu.extension.js';
import { UmbDefaultMenuItemContext } from '../menu-item/menu-item.context.js';
import type { ManifestMenuItem } from '../menu-item/types.js';
import { UmbDefaultMenuContext } from './menu.context.js';
import type { UmbMenuItemExpansionEntryModel } from './types.js';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbEntityExpansionModel } from '@umbraco-cms/backoffice/utils';

import '../menu-item/menu-item-default.element.js';

@customElement('umb-menu')
export class UmbMenuElement extends UmbLitElement {
	@property({ attribute: false })
	manifest?: ManifestMenu;

	public get expansion(): UmbEntityExpansionModel<UmbMenuItemExpansionEntryModel> {
		return this.#context.expansion.getExpansion();
	}
	public set expansion(value: UmbEntityExpansionModel<UmbMenuItemExpansionEntryModel>) {
		this.#context.expansion.setExpansion(value);
	}

	#context = new UmbDefaultMenuContext(this);

	override render() {
		return html`
			<umb-extension-with-api-slot
				type="menuItem"
				default-element="umb-menu-item-default"
				.defaultApi=${UmbDefaultMenuItemContext}
				.filter=${(items: ManifestMenuItem) => items.meta.menus.includes(this.manifest!.alias)}>
			</umb-extension-with-api-slot>
		`;
	}
}

export default UmbMenuElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-menu': UmbMenuElement;
	}
}
