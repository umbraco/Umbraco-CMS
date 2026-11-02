import type { ManifestWorkspaceActionMenuItem } from '../../extensions/types.js';
import { umbOrderActionsByGroup, umbRenderActionMenu } from '@umbraco-cms/backoffice/action';
import type { ManifestActionGroupBase } from '@umbraco-cms/backoffice/action';
import {
	css,
	html,
	customElement,
	property,
	state,
	nothing,
	query,
	eventOptions,
} from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIInterfaceColor, UUIInterfaceLook } from '@umbraco-cms/backoffice/external/uui';
import type { UmbExtensionElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';

@customElement('umb-workspace-action-menu')
export class UmbWorkspaceActionMenuElement extends UmbLitElement {
	@property()
	look: UUIInterfaceLook = 'secondary';

	@property()
	color: UUIInterfaceColor = 'default';

	@property({ type: Array, attribute: false })
	items: Array<UmbExtensionElementAndApiInitializer<ManifestWorkspaceActionMenuItem>> = [];

	@state()
	private _popoverOpen = false;

	@state()
	private _groups: Array<ManifestActionGroupBase> = [];

	@query('#workspace-action-popover')
	private _popover?: HTMLElement;

	constructor() {
		super();
		this.observe(
			umbExtensionsRegistry.byType('workspaceActionMenuItemGroup'),
			(groups) => (this._groups = groups),
			null,
		);
	}

	#onPopoverToggle(event: ToggleEvent) {
		// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this._popoverOpen = event.newState === 'open';
	}

	// Capture phase so this runs before a menu item's action opens its modal: WebKit otherwise
	// leaves the popovertarget toggle state "open" after the modal light-dismisses the popover,
	// and the trigger then stops responding until the page is reloaded (#22777).
	@eventOptions({ capture: true })
	private _onPopoverClickCapture() {
		if (this._popoverOpen) {
			this._popover?.hidePopover();
		}
	}

	override render() {
		if (!this.items?.length) return nothing;

		return html`
			<uui-button
				id="popover-trigger"
				popovertarget="workspace-action-popover"
				look="${this.look}"
				color="${this.color}"
				label=${this.localize.term('visuallyHiddenTexts_tabExpand')}
				compact>
				<uui-symbol-expand id="expand-symbol" .open=${this._popoverOpen}></uui-symbol-expand>
			</uui-button>
			<uui-popover-container
				id="workspace-action-popover"
				margin="6"
				placement="top-end"
				@toggle=${this.#onPopoverToggle}
				@click=${this._onPopoverClickCapture}>
				<umb-popover-layout id="workspace-action-popover-layout">
					<uui-scroll-container>
						${umbRenderActionMenu(
							umbOrderActionsByGroup(this.items, this._groups, 'workspaceActionMenuItem'),
							(ext) => ext.component,
							this.localize,
						)}
					</uui-scroll-container>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				--uui-menu-item-flat-structure: 1;
			}

			#expand-symbol {
				/* TODO: remove this hack and use a proper UUI symbol for this */
				transform: rotate(-90deg);
			}

			#expand-symbol[open] {
				transform: rotate(0deg);
			}

			#workspace-action-popover {
				min-width: 200px;
			}

			#popover-trigger {
				--uui-button-padding-top-factor: 0;
				--uui-button-padding-bottom-factor: 0.125;
			}

			#workspace-action-popover-layout {
				overflow: visible;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-action-menu': UmbWorkspaceActionMenuElement;
	}
}
