import { UMB_HELP_MENU_ALIAS } from '../menu/index.js';
import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbHeaderAppButtonElement } from '@umbraco-cms/backoffice/components';
import type { ManifestMenu } from '@umbraco-cms/backoffice/extension-registry';

const elementName = 'umb-help-header-app';
@customElement(elementName)
export class UmbHelpHeaderAppElement extends UmbHeaderAppButtonElement {
	@state()
	private _popoverOpen = false;

	#onPopoverToggle(event: ToggleEvent) {
		// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this._popoverOpen = event.newState === 'open';
	}

	override render() {
		return html`
			<uui-button popovertarget="help-menu-popover" look="primary" label="help" compact>
				<uui-icon name="icon-help-alt"></uui-icon>
			</uui-button>

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
