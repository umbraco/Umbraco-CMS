import { css, html } from 'lit';
import { repeat } from 'lit/directives/repeat.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { UmbLitElement } from '@umbraco-cms/element';
import { UmbSectionContext } from '../../section/section.context';
import { UmbTreeContextMenuService } from '../context-menu/tree-context-menu.service';
import { UmbTreeContext } from '../tree.context';
import type { Entity } from '@umbraco-cms/models';
import { UmbTreeDataStore } from '@umbraco-cms/stores/store';
import { DocumentTreeItem } from '@umbraco-cms/backend-api';

import '../tree-item.element';

@customElement('umb-tree-navigator')
export class UmbTreeNavigator extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	private _storeAlias = '';
	@property({ attribute: 'store-context-alias' })
	public get storeAlias() {
		return this._storeAlias;
	}

	public set storeAlias(value) {
		this._storeAlias = value;
		this._provideStoreContext();
	}

	@state()
	private _loading = true;

	@state()
	private _items: DocumentTreeItem[] = [];

	@state()
	private _href?: string;

	@state()
	private _treeRootNode: any;

	private _treeContext?: UmbTreeContext;
	private _treeContextMenuService?: UmbTreeContextMenuService;
	private _store?: UmbTreeDataStore<DocumentTreeItem>;
	private _sectionContext?: UmbSectionContext;

	constructor() {
		super();

		this.consumeContext('umbStore', (store) => {
			this._store = store;
		});

		this.consumeContext('umbTreeContext', (treeContext: UmbTreeContext) => {
			this._treeContext = treeContext;

			this._treeRootNode = {
				name: treeContext.tree.meta.label || '',
				icon: treeContext.tree.meta.icon || '',
				type: treeContext.tree.meta.rootNodeEntityType,
				hasChildren: true,
				parentKey: null,
			};
		});

		this.consumeContext('umbTreeContextMenuService', (treeContextMenuService: UmbTreeContextMenuService) => {
			this._treeContextMenuService = treeContextMenuService;
		});

		this.consumeContext('umbSectionContext', (sectionContext: UmbSectionContext) => {
			this._sectionContext = sectionContext;
			this._observeSection();
		});
	}

	private _provideStoreContext() {
		if (!this._storeAlias) return;

		this.consumeContext(this._storeAlias, (store) => {
			this._store = store;
			this.provideContext('umbStore', store);
		});
	}

	private _onShowRoot() {
		this._observeTreeRoot();
	}

	private _observeTreeRoot() {
		if (!this._store?.getTreeRoot) return;

		this._loading = true;

		this.observe(this._store.getTreeRoot(), (rootItems) => {
			if (rootItems?.length === 0) return;
			this._items = rootItems;
			this._loading = false;
		});
	}

	private _observeSection() {
		if (!this._sectionContext) return;

		this.observe(this._sectionContext?.data, (section) => {
			this._href = this._constructPath(section.meta.pathname, this._treeRootNode?.type);
		});
	}

	// TODO: how do we handle this?
	private _constructPath(sectionPathname: string, type: string | undefined) {
		return type ? `section/${sectionPathname}/${type}` : undefined;
	}

	private _openActions() {
		if (!this._treeContext || !this._sectionContext || !this._treeRootNode) return;

		this._sectionContext?.setActiveTree(this._treeContext?.tree);
		this._sectionContext?.setActiveTreeItem(this._treeRootNode);
		this._treeContextMenuService?.open({ name: this._treeRootNode.name, key: this._treeRootNode.key });
	}

	render() {
		// TODO: can we use tree item here instead so we don't have duplicated code?
		// TODO: how do we know if a tree has children?
		return html` <uui-menu-item
			label="${ifDefined(this._treeRootNode?.name)}"
			@show-children=${this._onShowRoot}
			href="${ifDefined(this._href)}"
			has-children>
			${this._renderRootItems()}
			<uui-icon slot="icon" name="${ifDefined(this._treeRootNode?.icon)}"></uui-icon>
			<uui-action-bar slot="actions">
				<uui-button @click=${this._openActions} label="Open actions menu">
					<uui-symbol-more></uui-symbol-more>
				</uui-button>
			</uui-action-bar>
		</uui-menu-item>`;
	}

	private _renderRootItems() {
		// TODO: Fix Type Mismatch ` as Entity` in this template:
		return html`
			${repeat(
				this._items,
				(item) => item.key,
				(item) => html`<umb-tree-item .treeItem=${item as Entity} .loading=${this._loading}></umb-tree-item>`
			)}
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-navigator': UmbTreeNavigator;
	}
}
