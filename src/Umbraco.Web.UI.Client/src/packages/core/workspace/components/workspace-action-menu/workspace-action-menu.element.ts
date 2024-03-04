import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import { css, html, customElement, property, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { ManifestWorkspaceActionMenuItem } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIInterfaceColor, UUIInterfaceLook } from '@umbraco-cms/backoffice/external/uui';

function ExtensionApiArgsMethod(manifest: ManifestWorkspaceActionMenuItem) {
	return [{ meta: manifest.meta }];
}
@customElement('umb-workspace-action-menu')
export class UmbWorkspaceActionMenuElement extends UmbLitElement {
	/**
	 * The workspace actions to filter the available actions by.
	 * @example ['Umb.WorkspaceAction.Document.Save', 'Umb.WorkspaceAction.Document.SaveAndPublishNew']
	 */
	@property({ attribute: false })
	public set forWorkspaceActions(value: Array<string>) {
		if (value === this._forWorkspaceActions) return;
		this._forWorkspaceActions = value;
		this._filter = (action) => {
			return Array.isArray(action.forWorkspaceActions)
				? action.forWorkspaceActions.some((alias) => this.forWorkspaceActions.includes(alias))
				: this.forWorkspaceActions.includes(action.forWorkspaceActions);
		};
	}
	public get forWorkspaceActions(): Array<string> {
		return this._forWorkspaceActions;
	}
	private _forWorkspaceActions: Array<string> = [];

	@state()
	_filter?: (action: ManifestWorkspaceActionMenuItem) => boolean;

	@property()
	look: UUIInterfaceLook = 'secondary';

	@property()
	color: UUIInterfaceColor = 'default';

	@state()
	_popoverOpen = false;

	// eslint-disable-next-line @typescript-eslint/ban-ts-comment
	// @ts-ignore
	#onPopoverToggle(event: ToggleEvent) {
		this._popoverOpen = event.newState === 'open';
	}

	#onActionExecuted(event: MouseEvent) {
		// TODO: Explicit close the popover?
		// Should we stop the event as well?
	}

	render() {
		return this._filter
			? html`
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
						margin="5"
						placement="top-end"
						@toggle=${this.#onPopoverToggle}>
						<umb-popover-layout>
							<uui-scroll-container>
								<umb-extension-with-api-slot
									type="workspaceActionMenuItem"
									default-element="umb-workspace-action-menu-item"
									.filter=${this._filter}
									.apiArgs=${ExtensionApiArgsMethod}
									@action-executed=${this.#onActionExecuted}>
								</umb-extension-with-api-slot>
							</uui-scroll-container>
						</umb-popover-layout>
					</uui-popover-container>
			  `
			: nothing;
	}

	static styles: CSSResultGroup = [
		UmbTextStyles,
		css`
			:host {
				--uui-menu-item-flat-structure: 1;
			}

			#expand-symbol {
				transform: rotate(-90deg);
			}

			#expand-symbol[open] {
				transform: rotate(0deg);
			}

			#workspace-action-popover {
				min-width: 200px;
			}

			#popover-trigger {
				--uui-button-padding-top-factor: 0.5;
				--uui-button-padding-bottom-factor: 0.1;
				--uui-button-border-radius: 0;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-action-menu': UmbWorkspaceActionMenuElement;
	}
}
