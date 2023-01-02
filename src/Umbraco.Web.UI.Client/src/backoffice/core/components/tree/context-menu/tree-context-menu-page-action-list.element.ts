import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { map } from 'rxjs';
import { UmbSectionContext } from '../../section/section.context';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import type { Entity, ManifestTreeItemAction, ManifestTree } from '@umbraco-cms/models';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';

@customElement('umb-tree-context-menu-page-action-list')
export class UmbTreeContextMenuPageActionListElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
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
	private _actions: Array<ManifestTreeItemAction> = [];

	@state()
	private _activeTree?: ManifestTree;

	@state()
	private _activeTreeItem?: Entity;

	private _sectionContext?: UmbSectionContext;

	constructor() {
		super();

		this.consumeContext('umbSectionContext', (sectionContext) => {
			this._sectionContext = sectionContext;
			this._observeActiveTree();
			this._observeActiveTreeItem();
			this._observeTreeItemActions();
		});
	}

	private _observeTreeItemActions() {
		if (!this._sectionContext) return;

		this.observe<ManifestTreeItemAction[]>(
			umbExtensionsRegistry
				.extensionsOfType('treeItemAction')
				.pipe(map((actions) => actions.filter((action) => action.meta.trees.includes(this._activeTree?.alias || '')))),
			(actions) => {
				this._actions = actions;
			}
		);
	}

	private _observeActiveTree() {
		if (!this._sectionContext) return;

		this.observe(this._sectionContext.activeTree, (tree) => {
			this._activeTree = tree;
		});
	}

	private _observeActiveTreeItem() {
		if (!this._sectionContext) return;

		this.observe(this._sectionContext.activeTreeItem, (treeItem) => {
			this._activeTreeItem = treeItem;
		});
	}

	private _renderActions() {
		return this._actions.map((action) => {
			return html`<umb-tree-item-action-extension .treeAction=${action}></umb-tree-item-action-extension> `;
		});
	}

	render() {
		return html`
			<div id="title">
				<h3>${this._activeTreeItem?.name}</h3>
			</div>
			<div id="action-list">${this._renderActions()}</div>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-context-menu-page-action-list': UmbTreeContextMenuPageActionListElement;
	}
}
