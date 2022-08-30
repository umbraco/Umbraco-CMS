import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { UmbActionService } from './actions.service';
import { UmbContextConsumerMixin } from '../../core/context';

@customElement('umb-actions-modal')
export class UmbActionsModal extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			#title {
				display: flex;
				flex-direction: column;
				justify-content: center;
				padding: 0 var(--uui-size-4);
				height: 70px;
				box-sizing: border-box;
				border-bottom: 1px solid var(--uui-color-divider-standalone);
			}
			#title > * {
				margin: 0;
			}

			.action {
				padding: var(--uui-size-2) var(--uui-size-4);
				border-bottom: 1px solid var(--uui-color-divider);
				cursor: pointer;
			}
			.action:hover {
				background-color: var(--uui-color-surface-emphasis);
				color: var(--uui-color-interactive-emphasis);
			}
		`,
	];

	private _actionService?: UmbActionService;

	constructor() {
		super();

		this.consumeContext('umbActionService', (actionService: UmbActionService) => {
			this._actionService = actionService;
		});
	}

	@property()
	name = '';

	private _actionList = ['create', 'rename', 'delete', 'reload'];

	renderActions() {
		return this._actionList.map((action) => {
			return html` <div class="action" @click=${() => this._actionService?.execute(action)}>${action}</div> `;
		});
	}

	render() {
		return html`
			<div id="title">
				<h3>${this.name}</h3>
			</div>
			<div id="action-list">${this.renderActions()}</div>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-actions-modal': UmbActionsModal;
	}
}
