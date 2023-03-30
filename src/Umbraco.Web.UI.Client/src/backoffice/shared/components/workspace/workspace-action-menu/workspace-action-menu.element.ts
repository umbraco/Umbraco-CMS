import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbExecutedEvent } from '@umbraco-cms/backoffice/events';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-workspace-action-menu')
export class UmbWorkspaceActionMenuElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			#action-menu-popover {
				display: block;
			}
			#action-menu-dropdown {
				overflow: hidden;
				z-index: -1;
				background-color: var(--uui-combobox-popover-background-color, var(--uui-color-surface));
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
				width: 100%;
				height: 100%;
				box-sizing: border-box;
				box-shadow: var(--uui-shadow-depth-3);
				width: 250px;
				position: absolute;
				right: 5px;
				height: auto;
			}
		`,
	];

	@state()
	private _actionMenuIsOpen = false;

	private _workspaceContext?: typeof UMB_ENTITY_WORKSPACE_CONTEXT.TYPE;

	@state()
	_entityKey?: string;

	@state()
	_entityType?: string;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (context) => {
			this._workspaceContext = context;
			this._observeInfo();
		});
	}

	private _observeInfo() {
		if (!this._workspaceContext) return;
		this._entityKey = this._workspaceContext.getEntityKey();
		this._entityType = this._workspaceContext.getEntityType();
	}

	#close() {
		this._actionMenuIsOpen = false;
	}

	#open() {
		this._actionMenuIsOpen = true;
	}

	#onActionExecuted(event: UmbExecutedEvent) {
		event.stopPropagation();
		this.#close();
	}

	render() {
		return html` ${this.#renderActionsMenu()} `;
	}

	#renderActionsMenu() {
		return this._entityKey
			? html`
			<uui-popover  id="action-menu-popover" .open=${this._actionMenuIsOpen} @close=${this.#close}>
				<uui-button slot="trigger" label="Actions" @click=${this.#open}></uui-button>
				<div id="action-menu-dropdown" slot="popover">
					<uui-scroll-container>
						<umb-entity-action-list @executed=${this.#onActionExecuted} entity-type=${this._entityType as string} unique=${
					this._entityKey
			  }></umb-entity-action-list>
					</uui-scroll-container>
				</div>
			</uui-popover>
			</div>`
			: '';
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-action-menu': UmbWorkspaceActionMenuElement;
	}
}
