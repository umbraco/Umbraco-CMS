import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { UUIMenuItemEvent } from '@umbraco-ui/uui';
import { map } from 'rxjs';
import { repeat } from 'lit/directives/repeat.js';
import type { UmbTreeContextBase } from '../tree.context';
import { UmbSectionContext } from '../../sections/shared/section.context';
import { UmbTreeContextMenuService } from './context-menu/tree-context-menu.service';
import type { Entity, ManifestSection } from '@umbraco-cms/models';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { UmbTreeDataStore } from 'src/core/stores/store';

@customElement('umb-tree-item')
export class UmbTreeItem extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles = [UUITextStyles, css``];

	@property({ type: Object, attribute: false })
	treeItem!: Entity;

	@state()
	private _childItems: Entity[] = [];

	@state()
	private _href? = '';

	@state()
	private _loading = false;

	@state()
	private _selectable = false;

	@state()
	private _selected = false;

	@state()
	private _isActive = false;

	private _treeContext?: UmbTreeContextBase;
	private _store?: UmbTreeDataStore<unknown>;
	private _sectionContext?: UmbSectionContext;
	private _treeContextMenuService?: UmbTreeContextMenuService;

	constructor() {
		super();

		this.consumeContext('umbTreeContext', (treeContext: UmbTreeContextBase) => {
			this._treeContext = treeContext;
			this._observeSelectable();
			this._observeIsSelected();
		});

		this.consumeContext('umbStore', (store: UmbTreeDataStore<unknown>) => {
			this._store = store;
		});

		this.consumeContext('umbSectionContext', (sectionContext: UmbSectionContext) => {
			this._sectionContext = sectionContext;
			this._observeSection();
			this._observeActiveTreeItem();
		});

		this.consumeContext('umbTreeContextMenuService', (treeContextMenuService: UmbTreeContextMenuService) => {
			this._treeContextMenuService = treeContextMenuService;
		});
	}

	connectedCallback(): void {
		super.connectedCallback();

		this.addEventListener('selected', this._handleSelectedItem);
		this.addEventListener('unselected', this._handleDeselectedItem);
	}

	private _handleSelectedItem(event: Event) {
		event.stopPropagation();
		this._treeContext?.select(this.treeItem.key);
	}

	private _handleDeselectedItem(event: Event) {
		event.stopPropagation();
		this._treeContext?.deselect(this.treeItem.key);
	}

	private _observeSection() {
		if (!this._sectionContext) return;

		this.observe<ManifestSection>(this._sectionContext?.data, (section) => {
			this._href = this._constructPath(section.meta.pathname, this.treeItem.type, this.treeItem.key);
		});
	}

	private _observeSelectable() {
		if (!this._treeContext) return;

		this.observe<boolean>(this._treeContext.selectable, (value) => {
			this._selectable = value;
		});
	}

	private _observeIsSelected() {
		if (!this._treeContext) return;

		this.observe<boolean>(
			this._treeContext.selection.pipe(map((keys) => keys?.includes(this.treeItem.key))),
			(isSelected) => {
				this._selected = isSelected;
			}
		);
	}

	private _observeActiveTreeItem() {
		if (!this._sectionContext) return;

		this.observe<Entity>(this._sectionContext?.activeTreeItem, (treeItem) => {
			this._isActive = treeItem.key === this.treeItem.key;
		});
	}

	// TODO: how do we handle this?
	private _constructPath(sectionPathname: string, type: string, key: string) {
		return type ? `section/${sectionPathname}/${type}/${key}` : undefined;
	}

	private _onShowChildren(event: UUIMenuItemEvent) {
		event.stopPropagation();
		if (this._childItems.length > 0) return;
		this._observeChildren();
	}

	private _observeChildren() {
		if (!this._store?.getTreeItemChildren) return;

		this._loading = true;

		this.observe<Entity[]>(this._store.getTreeItemChildren(this.treeItem.key), (childItems) => {
			if (childItems?.length === 0) return;
			this._childItems = childItems;
			this._loading = false;
		});
	}

	private _renderChildItems() {
		return html`
			${repeat(
				this._childItems,
				(item) => item.key,
				(item) => html`<umb-tree-item .treeItem=${item}></umb-tree-item>`
			)}
		`;
	}

	private _openActions() {
		if (!this._treeContext || !this._sectionContext || !this.treeItem) return;

		this._sectionContext?.setActiveTree(this._treeContext?.tree);
		this._sectionContext?.setActiveTreeItem(this.treeItem);
		this._treeContextMenuService?.open({ name: this.treeItem.name, key: this.treeItem.key });
	}

	render() {
		return html`
			<uui-menu-item
				@show-children=${this._onShowChildren}
				?selectable=${this._selectable}
				?selected=${this._selected}
				.loading=${this._loading}
				.hasChildren=${this.treeItem.hasChildren}
				label="${this.treeItem.name}"
				href="${ifDefined(this._href)}"
				?active=${this._isActive}>
				${this._renderChildItems()}
				<uui-icon slot="icon" name="${this.treeItem.icon}"></uui-icon>
				<uui-action-bar slot="actions">
					<uui-button @click=${this._openActions} label="Open actions menu">
						<uui-symbol-more></uui-symbol-more>
					</uui-button>
				</uui-action-bar>
			</uui-menu-item>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-item': UmbTreeItem;
	}
}
