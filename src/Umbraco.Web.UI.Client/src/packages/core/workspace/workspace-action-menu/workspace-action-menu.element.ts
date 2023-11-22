import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
@customElement('umb-workspace-action-menu')
export class UmbWorkspaceActionMenuElement extends UmbLitElement {
	private _workspaceContext?: typeof UMB_WORKSPACE_CONTEXT.TYPE;

	@state()
	_entityId?: string;

	@state()
	_entityType?: string;

	constructor() {
		super();

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (context) => {
			this._workspaceContext = context;
			this._observeInfo();
		});
	}

	private _observeInfo() {
		if (!this._workspaceContext) return;
		this._entityId = this._workspaceContext.getEntityId();
		this._entityType = this._workspaceContext.getEntityType();
	}

	#onActionExecuted(event: UmbActionExecutedEvent) {
		event.stopPropagation();
	}

	render() {
		return html` ${this.#renderActionsMenu()} `;
	}

	#renderActionsMenu() {
		return this._entityId && this._entityType
			? html`
					<uui-button popovertarget="workspace-action-menu-popover" label="Actions"></uui-button>
					<uui-popover-container id="workspace-action-menu-popover" popover placement="bottom-end">
						<umb-popover-layout style="--umb-popover-layout-padding: 0">
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

	static styles = [UmbTextStyles, css``];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-action-menu': UmbWorkspaceActionMenuElement;
	}
}
