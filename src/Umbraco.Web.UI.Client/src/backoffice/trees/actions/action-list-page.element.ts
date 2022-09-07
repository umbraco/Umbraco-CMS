import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import type { ManifestEntityAction, ManifestTree } from '../../../core/models';
import UmbActionElement from './action.element';
import { UmbExtensionRegistry } from '../../../core/extension';
import { UmbContextConsumerMixin } from '../../../core/context';
import { map, Subscription } from 'rxjs';
import { UmbSectionContext } from '../../sections/section.context';

@customElement('umb-action-list-page')
export class UmbActionListPageElement extends UmbContextConsumerMixin(UmbActionElement) {
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

	@state()
	private _actions: Array<ManifestEntityAction> = [];

	@state()
	private _activeTree?: ManifestTree;

	private _extensionRegistry?: UmbExtensionRegistry;
	private _sectionContext?: UmbSectionContext;

	private _treeItemActionsSubscription?: Subscription;
	private _activeTreeSubscription?: Subscription;

	connectedCallback() {
		super.connectedCallback();

		this.consumeContext('umbExtensionRegistry', (extensionRegistry) => {
			this._extensionRegistry = extensionRegistry;
			this._observeTreeItemActions();
		});

		this.consumeContext('umbSectionContext', (sectionContext) => {
			this._sectionContext = sectionContext;
			this._observeActiveTree();
			this._observeTreeItemActions();
		});
	}

	private _observeTreeItemActions() {
		if (!this._extensionRegistry || !this._sectionContext) return;

		this._treeItemActionsSubscription?.unsubscribe();

		this._treeItemActionsSubscription = this._extensionRegistry
			?.extensionsOfType('treeItemAction')
			.pipe(map((actions) => actions.filter((action) => action.meta.trees.includes(this._activeTree?.alias))))
			.subscribe((actions) => {
				this._actions = actions;
			});
	}

	private _observeActiveTree() {
		this._activeTreeSubscription?.unsubscribe();

		this._activeTreeSubscription = this._sectionContext?.activeTree.subscribe((tree) => {
			this._activeTree = tree;
		});
	}

	private _renderActions() {
		return this._actions
			.sort((a, b) => a.meta.weight - b.meta.weight)
			.map((action) => {
				return html`<umb-tree-action .treeAction=${action}></umb-tree-action> `;
			});
	}

	disconnectCallback(): void {
		super.disconnectCallback();
		this._treeItemActionsSubscription?.unsubscribe();
		this._activeTreeSubscription?.unsubscribe();
	}

	render() {
		return html`
			<div id="title">
				<h3>${this._entity.name}</h3>
			</div>
			<div id="action-list">${this._renderActions()}</div>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-action-list-page': UmbActionListPageElement;
	}
}
