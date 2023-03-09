import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { map, Observable } from 'rxjs';
import { repeat } from 'lit/directives/repeat.js';
import { UmbSectionContext, UMB_SECTION_CONTEXT_TOKEN } from '../section/section.context';
import {
	UmbSectionSidebarContext,
	UMB_SECTION_SIDEBAR_CONTEXT_TOKEN,
} from '../section/section-sidebar/section-sidebar.context';
import type { UmbTreeContextBase } from './tree.context';
import type { Entity } from '@umbraco-cms/models';
import type { UmbTreeStore } from '@umbraco-cms/store';
import { UmbLitElement } from '@umbraco-cms/element';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-api';

@customElement('umb-tree-item')
export class UmbTreeItem extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	@property({ type: String })
	key = '';

	@property({ type: String })
	parentKey: string | null = null;

	@property({ type: String })
	label = '';

	@property({ type: String })
	icon = '';

	private _entityType = '';
	@property({ type: String })
	get entityType() {
		return this._entityType;
	}
	set entityType(newVal) {
		const oldVal = this._entityType;
		this._entityType = newVal;
		this.requestUpdate('entityType', oldVal);
		this._observeEntityActions();
	}

	@property({ type: Boolean, attribute: 'has-children' })
	hasChildren = false;

	@state()
	private _childItems?: Entity[];

	@state()
	private _href?: string;

	@state()
	private _loading = false;

	@state()
	private _selectable = false;

	@state()
	private _selected = false;

	@state()
	private _isActive = false;

	@state()
	private _hasActions = false;

	private _treeContext?: UmbTreeContextBase;
	private _store?: UmbTreeStore<unknown>;
	private _sectionContext?: UmbSectionContext;
	private _sectionSidebarContext?: UmbSectionSidebarContext;

	constructor() {
		super();

		this.consumeContext('umbTreeContext', (treeContext: UmbTreeContextBase) => {
			this._treeContext = treeContext;
			this._observeSelectable();
			this._observeIsSelected();
		});

		this.consumeContext('umbStore', (store: UmbTreeStore<unknown>) => {
			this._store = store;
		});

		this.consumeContext(UMB_SECTION_CONTEXT_TOKEN, (sectionContext) => {
			this._sectionContext = sectionContext;
			this._observeSection();
			this._observeActiveTreeItem();
		});

		this.consumeContext(UMB_SECTION_SIDEBAR_CONTEXT_TOKEN, (instance) => {
			this._sectionSidebarContext = instance;
		});
	}

	connectedCallback(): void {
		super.connectedCallback();

		this.addEventListener('selected', this._handleSelectedItem);
		this.addEventListener('unselected', this._handleDeselectedItem);
	}

	private _handleSelectedItem(event: Event) {
		event.stopPropagation();
		this._treeContext?.select(this.key);
	}

	private _handleDeselectedItem(event: Event) {
		event.stopPropagation();
		this._treeContext?.deselect(this.key);
	}

	private _observeSection() {
		if (!this._sectionContext) return;

		this.observe(this._sectionContext?.pathname, (pathname) => {
			this._href = this._constructPath(pathname || '', this.entityType, this.key);
		});
	}

	private _observeSelectable() {
		if (!this._treeContext) return;

		this.observe(this._treeContext.selectable, (value) => {
			this._selectable = value || false;
		});
	}

	private _observeIsSelected() {
		if (!this._treeContext) return;

		this.observe(this._treeContext.selection.pipe(map((keys) => keys?.includes(this.key))), (isSelected) => {
			this._selected = isSelected || false;
		});
	}

	private _observeActiveTreeItem() {
		if (!this._sectionContext) return;

		this.observe(this._sectionContext?.activeTreeItem, (treeItem) => {
			this._isActive = this.key === treeItem?.key;
		});
	}

	private _observeEntityActions() {
		// TODO: Stop previous observation, currently we can do this from the UmbElementMixin as its a new subscription when Actions or entityType has changed.
		// Solution: store the current observation controller and if it existing then destroy it.
		// TODO: as long as a tree consist of one entity type we don't have to observe this every time a new tree item is created.
		// Solution: move this to the tree context and observe it once.
		this.observe(
			umbExtensionsRegistry
				.extensionsOfType('entityAction')
				.pipe(map((actions) => actions.filter((action) => action.meta.entityType === this._entityType))),
			(actions) => {
				this._hasActions = actions.length > 0;
			}
		);
	}

	// TODO: how do we handle this?
	private _constructPath(sectionPathname: string, type: string, key: string) {
		return type ? `section/${sectionPathname}/${type}/edit/${key}` : undefined;
	}

	// TODO: do we want to catch and emit a backoffice event here?
	private _onShowChildren() {
		if (this._childItems && this._childItems.length > 0) return;
		this._observeChildren();
		this._observeRepositoryChildren();
	}

	private async _observeRepositoryChildren() {
		if (!this._treeContext?.requestChildrenOf) return;

		// TODO: add loading state
		this._treeContext.requestChildrenOf(this.key);

		this.observe(await this._treeContext.childrenOf(this.key), (childItems) => {
			this._childItems = childItems as Entity[];
		});
	}

	// TODO: remove when repositories are in place
	private _observeChildren() {
		if (!this._store?.getTreeItemChildren) return;

		this._loading = true;

		// TODO: we should do something about these types, stop having our own version of Entity.
		this.observe(this._store.getTreeItemChildren(this.key) as Observable<Entity[]>, (childItems) => {
			this._childItems = childItems;
			this._loading = false;
		});
	}

	private _openActions() {
		if (!this._treeContext || !this._sectionContext) return;

		// This is out-commented as it was not used. only kept if someone need this later:
		//this._sectionContext?.setActiveTree(this._treeContext?.tree);

		this._sectionContext?.setActiveTreeItem({
			key: this.key,
			name: this.label,
			icon: this.icon,
			type: this.entityType,
			hasChildren: this.hasChildren,
			parentKey: this.parentKey,
		});
		this._sectionSidebarContext?.toggleContextMenu(this.entityType, this.key, this.label);
	}

	render() {
		return html`
			<uui-menu-item
				@show-children=${this._onShowChildren}
				?selectable=${this._selectable}
				?selected=${this._selected}
				.loading=${this._loading}
				.hasChildren=${this.hasChildren}
				label="${this.label}"
				href="${ifDefined(this._href)}"
				?active=${this._isActive}>
				${this._renderChildItems()}
				<uui-icon slot="icon" name="${this.icon}"></uui-icon>
				${this._renderActions()}
				<slot></slot>
			</uui-menu-item>
		`;
	}

	private _renderChildItems() {
		return html`
			${this._childItems
				? repeat(
						this._childItems,
						(item) => item.key,
						(item) =>
							html`<umb-tree-item
								.key=${item.key}
								.label=${item.name}
								.icon=${item.icon}
								.entityType=${item.type}
								.hasChildren=${item.hasChildren}></umb-tree-item>`
				  )
				: ''}
		`;
	}

	private _renderActions() {
		return html`
			${this._hasActions
				? html`
						<uui-action-bar slot="actions">
							<uui-button @click=${this._openActions} label="Open actions menu">
								<uui-symbol-more></uui-symbol-more>
							</uui-button>
						</uui-action-bar>
				  `
				: nothing}
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-item': UmbTreeItem;
	}
}
