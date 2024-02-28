import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
@customElement('umb-workspace-action-menu')
export class UmbWorkspaceActionMenuElement extends UmbLitElement {
	private _workspaceContext?: typeof UMB_WORKSPACE_CONTEXT.TYPE;

	@state()
	_entityId?: string;

	@state()
	_entityType?: string;

	@state()
	_popoverOpen = false;

	constructor() {
		super();

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (context) => {
			this._workspaceContext = context;
			this._observeInfo();
		});
	}

	private _observeInfo() {
		if (!this._workspaceContext) return;
		this._entityId = this._workspaceContext.getUnique();
		this._entityType = this._workspaceContext.getEntityType();
	}

	#onActionExecuted(event: UmbActionExecutedEvent) {
		event.stopPropagation();
	}

	// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
	// eslint-disable-next-line @typescript-eslint/ban-ts-comment
	// @ts-ignore
	#onPopoverToggle(event: ToggleEvent) {
		this._popoverOpen = event.newState === 'open';
	}

	render() {
		return html` ${this.#renderActionsMenu()} `;
	}

	#renderActionsMenu() {
		return this._entityId && this._entityType
			? html`
					<uui-button popovertarget="workspace-action-menu-popover" label="Actions">
						Actions
						<uui-symbol-expand .open=${this._popoverOpen}></uui-symbol-expand>
					</uui-button>
					<uui-popover-container
						id="workspace-action-menu-popover"
						placement="bottom-end"
						@toggle=${this.#onPopoverToggle}>
						<umb-popover-layout>
							<uui-scroll-container>
								<umb-entity-action-list
									@action-executed=${this.#onActionExecuted}
									.entityType=${this._entityType}
									.unique=${this._entityId}>
								</umb-entity-action-list>
							</uui-scroll-container>
						</umb-popover-layout>
					</uui-popover-container>
			  `
			: nothing;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				height: 100%;
			}

			:host > uui-button {
				height: 100%;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-action-menu': UmbWorkspaceActionMenuElement;
	}
}
