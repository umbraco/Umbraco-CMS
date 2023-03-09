import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { UmbLitElement } from '@umbraco-cms/element';
import { UmbExecutedEvent } from '@umbraco-cms/events';

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

	@property({ type: String })
	public unique?: string;

	@property({ type: String, attribute: 'entity-type' })
	public entityType?: string;

	@state()
	private _actionMenuIsOpen = false;

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
		return html`
			<uui-popover  id="action-menu-popover" .open=${this._actionMenuIsOpen} @close=${this.#close}>
				<uui-button slot="trigger" label="Actions" @click=${this.#open}></uui-button>
				<div id="action-menu-dropdown" slot="popover">
					<uui-scroll-container>
						<umb-entity-action-list @executed=${this.#onActionExecuted} entity-type=${ifDefined(
			this.entityType
		)} unique=${ifDefined(this.unique)}></umb-entity-action-list>
					</uui-scroll-container>
				</div>
			</uui-popover>
			</div>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-action-menu': UmbWorkspaceActionMenuElement;
	}
}
