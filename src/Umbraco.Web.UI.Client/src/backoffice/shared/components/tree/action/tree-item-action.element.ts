import { customElement, property, state } from 'lit/decorators.js';
import { UmbSectionContext } from '../../section/section.context';
import { UmbTreeContextMenuPageService } from '../context-menu/tree-context-menu-page.service';
import { UmbTreeContextMenuService } from '../context-menu/tree-context-menu.service';
import type { Entity, ManifestTreeItemAction, ManifestTree } from '@umbraco-cms/models';
import { UmbLitElement } from '@umbraco-cms/element';

export type ActionPageEntity = {
	key: string;
	name: string;
};

@customElement('umb-tree-item-action')
export default class UmbTreeItemActionElement extends UmbLitElement {
	@property({ attribute: false })
	public treeAction?: ManifestTreeItemAction;

	@state()
	protected _entity: ActionPageEntity = { name: '', key: '' };

	protected _activeTree?: ManifestTree;
	protected _activeTreeItem?: Entity;

	protected _sectionContext?: UmbSectionContext;
	protected _treeContextMenuService?: UmbTreeContextMenuService;
	protected _actionPageService?: UmbTreeContextMenuPageService;

	connectedCallback() {
		super.connectedCallback();

		this.consumeContext('umbSectionContext', (sectionContext) => {
			this._sectionContext = sectionContext;
			this._observeActiveTree();
			this._observeActiveTreeItem();
		});

		this.consumeContext('umbTreeContextMenuService', (treeContextMenuService: UmbTreeContextMenuService) => {
			this._treeContextMenuService = treeContextMenuService;
		});

		this.consumeContext('umbTreeContextMenuPageService', (actionPageService: UmbTreeContextMenuPageService) => {
			this._actionPageService = actionPageService;
			this._observeEntity();
		});
	}

	private _observeEntity() {
		if (!this._actionPageService) return;

		this.observe(this._actionPageService.entity, (entity) => {
			this._entity = entity;
		});
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
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-item-action': UmbTreeItemActionElement;
	}
}
