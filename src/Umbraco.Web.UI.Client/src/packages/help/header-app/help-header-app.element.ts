import { UMB_HELP_MENU_ALIAS } from '../menu/index.js';
import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import { css, html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbHeaderAppButtonElement } from '@umbraco-cms/backoffice/components';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestMenu } from '@umbraco-cms/backoffice/menu';
import { UmbExtensionsManifestInitializer } from '@umbraco-cms/backoffice/extension-api';

const elementName = 'umb-help-header-app';
@customElement(elementName)
export class UmbHelpHeaderAppElement extends UmbHeaderAppButtonElement {
	@state()
	private _popoverOpen = false;

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

	#onPopoverToggle(event: ToggleEvent) {
		// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this._popoverOpen = event.newState === 'open';
	}

	override render() {
		return html` ${this.#renderButton()} ${this.#renderPopover()} `;
	}

	#renderButton() {
		if (!this._helpMenuHasMenuItems) return nothing;

		return html`
			<uui-button popovertarget="help-menu-popover" look="primary" label="help" compact>
				<uui-icon name="icon-help-alt"></uui-icon>
			</uui-button>
		`;
	}

	#renderPopover() {
		return html`
			<uui-popover-container id="help-menu-popover" @toggle=${this.#onPopoverToggle}>
				<umb-popover-layout>
					<uui-scroll-container>
						<umb-extension-slot
							type="menu"
							.filter="${(menu: ManifestMenu) => menu.alias === UMB_HELP_MENU_ALIAS}"
							default-element="umb-menu"></umb-extension-slot>
					</uui-scroll-container>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	static override styles: CSSResultGroup = [UmbHeaderAppButtonElement.styles, css``];
}

export { UmbHelpHeaderAppElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbHelpHeaderAppElement;
	}
}
