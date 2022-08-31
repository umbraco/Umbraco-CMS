import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { UmbActionService } from './actions.service';
import { UmbContextConsumerMixin } from '../../core/context';
import type { ManifestEntityAction } from '../../core/models';

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
				display: flex;
				padding: var(--uui-size-2) var(--uui-size-4);
				border-bottom: 1px solid var(--uui-color-divider);
				cursor: pointer;
				align-items: center;
				gap: var(--uui-size-3);
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

	private _actionList: ManifestEntityAction[] = [
		{
			name: 'create',
			alias: 'action.create',
			meta: {
				label: 'Create',
				icon: 'add',
				weight: 10,
			},
			type: 'entityAction',
		},
		{
			name: 'rename',
			alias: 'action.rename',
			meta: {
				label: 'Rename',
				icon: 'edit',
				weight: 20,
			},
			type: 'entityAction',
		},
		{
			name: 'delete',
			alias: 'action.delete',
			meta: {
				label: 'Delete',
				icon: 'delete',
				weight: 30,
			},
			type: 'entityAction',
		},
		{
			name: 'reload',
			alias: 'action.reload',
			meta: {
				label: 'Reload',
				icon: 'sync',
				weight: 40,
			},
			type: 'entityAction',
		},
	];

	renderActions() {
		return this._actionList
			.sort((a, b) => a.meta.weight - b.meta.weight)
			.map((action) => {
				return html`
					<div class="action" @keydown=${() => ''} @click=${() => this._actionService?.execute(action)}>
						<uui-icon .name=${action.meta.icon}></uui-icon>
						${action.meta.label}
					</div>
				`;
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
