import { css, html, LitElement } from 'lit';
import { repeat } from 'lit/directives/repeat.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { UmbSectionContext } from '../../sections/section.context';
import { UmbTreeContext } from '../tree.context';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import type { Entity, ManifestSection, ManifestTree } from '@umbraco-cms/models';
import { UmbDataStore } from 'src/core/stores/store';

import './tree-item.element';
import { UmbDocumentTypeStore } from '@umbraco-cms/stores/document-type/document-type.store';

@customElement('umb-tree-navigator')
export class UmbTreeNavigator extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles = [UUITextStyles, css``];

	@state()
	private _loading = true;

	@state()
	private _items: Entity[] = [];

	@state()
	private _tree?: ManifestTree;

	@state()
	private _href?: string;

	private _treeStore?: UmbDataStore<unknown>;
	private _sectionContext?: UmbSectionContext;

	constructor() {
		super();

		this.consumeContext('umbTreeStore', (treeStore) => {
			this._treeStore = treeStore;
		});

		this.consumeContext('umbTreeContext', (treeContext: UmbTreeContext) => {
			this._tree = treeContext.tree;
		});

		this.consumeContext('umbSectionContext', (sectionContext: UmbSectionContext) => {
			this._sectionContext = sectionContext;
			this._observeSection();
		});
	}

	private _onShowRoot() {
		this._observeTreeRoot();
	}

	private _observeTreeRoot() {
		if (!this._treeStore?.getTreeRoot) return;

		this._loading = true;

		this.observe<Entity[]>(this._treeStore.getTreeRoot(), (rootItems) => {
			if (rootItems?.length === 0) return;
			this._items = rootItems;
			this._loading = false;
		});
	}

	private _observeSection() {
		if (!this._sectionContext) return;

		this.observe<ManifestSection>(this._sectionContext?.data, (section) => {
			this._href = this._constructPath(section.meta.pathname, this._tree?.meta.rootNodeEntityType);
		});
	}

	// TODO: how do we handle this?
	private _constructPath(sectionPathname: string, type: string | undefined) {
		return type ? `section/${sectionPathname}/${type}` : undefined;
	}

	render() {
		// TODO: how do we know if a tree has children?
		return html`<uui-menu-item
			label="${ifDefined(this._tree?.meta.label)}"
			@show-children=${this._onShowRoot}
			href="${ifDefined(this._href)}"
			has-children>
			<uui-icon slot="icon" name="${ifDefined(this._tree?.meta.icon)}"></uui-icon>
			${this._renderRootItems()}
		</uui-menu-item>`;
	}

	private _renderRootItems() {
		return html`
			${repeat(
				this._items,
				(item) => item.key,
				(item) => html`<umb-tree-item .treeItem=${item} .loading=${this._loading}></umb-tree-item>`
			)}
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-navigator': UmbTreeNavigator;
	}
}
