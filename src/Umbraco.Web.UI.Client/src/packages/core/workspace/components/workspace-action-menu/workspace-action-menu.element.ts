import { css, html, customElement, property, state, nothing, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { ManifestWorkspaceActionMenuItem } from '@umbraco-cms/backoffice/workspace';
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
	_popoverOpen = false;

	#onPopoverToggle(event: ToggleEvent) {
		// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this._popoverOpen = event.newState === 'open';
	}

	override render() {
		if (!this.items?.length) return nothing;

		return html`<uui-button
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
				@toggle=${this.#onPopoverToggle}>
				<umb-popover-layout>
					<uui-scroll-container>
						${repeat(
							this.items,
							(ext) => ext.alias,
							(ext) => ext.component,
						)}
					</uui-scroll-container>
				</umb-popover-layout>
			</uui-popover-container>`;
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
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-action-menu': UmbWorkspaceActionMenuElement;
	}
}
