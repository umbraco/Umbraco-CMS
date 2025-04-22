import { UMB_HELP_MENU_ALIAS } from '../menu/index.js';
import { css, customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbExtensionsManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbHeaderAppButtonElement } from '@umbraco-cms/backoffice/components';
import type { ManifestMenu } from '@umbraco-cms/backoffice/menu';

@customElement('umb-help-header-app')
export class UmbHelpHeaderAppElement extends UmbHeaderAppButtonElement {
	@state()
	private _helpMenuHasMenuItems = false;

	constructor() {
		super();

		new UmbExtensionsManifestInitializer(
			this,
			umbExtensionsRegistry,
			'menuItem',
			(manifest) => manifest.meta.menus.includes(UMB_HELP_MENU_ALIAS),
			(menuItems) => {
				const manifests = menuItems.map((menuItem) => menuItem.manifest);
				this._helpMenuHasMenuItems = manifests.length > 0;
			},
		);
	}

	override render() {
		return html`${this.#renderButton()} ${this.#renderPopover()}`;
	}

	#renderButton() {
		if (!this._helpMenuHasMenuItems) return nothing;

		return html`
			<uui-button compact label=${this.localize.term('general_help')} look="primary" popovertarget="help-menu-popover">
				<uui-icon name="icon-help-alt"></uui-icon>
			</uui-button>
		`;
	}

	#renderPopover() {
		return html`
			<uui-popover-container id="help-menu-popover" placement="top-end">
				<umb-popover-layout>
					<uui-scroll-container>
						<umb-extension-slot
							type="menu"
							.filter=${(menu: ManifestMenu) => menu.alias === UMB_HELP_MENU_ALIAS}
							default-element="umb-menu"></umb-extension-slot>
					</uui-scroll-container>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	static override styles = [
		...UmbHeaderAppButtonElement.styles,
		css`
			:host {
				--uui-menu-item-flat-structure: 1;
			}
		`,
	];
}

export { UmbHelpHeaderAppElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-help-header-app': UmbHelpHeaderAppElement;
	}
}
