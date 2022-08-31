import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { UmbActionService } from './actions.service';
import { UmbContextConsumerMixin } from '../../core/context';
import type { ManifestEntityAction } from '../../core/models';
import './shared/tree-action.element';

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

	private _actionList: Array<ManifestEntityAction & { loader?: () => Promise<object | HTMLElement> }> = [
		{
			name: 'create',
			alias: 'action.create',
			meta: {
				label: 'Create',
				icon: 'add',
				weight: 10,
			},
			loader: () => import('./actions/tree-action-create.element'),
			type: 'entityAction',
		},
		{
			name: 'delete',
			alias: 'action.delete',
			meta: {
				label: 'Delete',
				icon: 'delete',
				weight: 20,
			},
			loader: () => import('./actions/tree-action-delete.element'),
			type: 'entityAction',
		},
		{
			name: 'reload',
			alias: 'action.reload',
			meta: {
				label: 'Reload',
				icon: 'sync',
				weight: 30,
			},
			loader: () => import('./actions/tree-action-reload.element'),
			type: 'entityAction',
		},
	];

	renderActions() {
		return this._actionList
			.sort((a, b) => a.meta.weight - b.meta.weight)
			.map((action) => {
				return html`<umb-tree-action .treeAction=${action}></umb-tree-action> `;
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
