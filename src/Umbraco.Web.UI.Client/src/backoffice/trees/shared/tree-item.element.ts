import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UUIMenuItemEvent } from '@umbraco-ui/uui';
import { map } from 'rxjs';
import { repeat } from 'lit/directives/repeat.js';
import { UmbContextConsumerMixin } from '../../../core/context';
import { UmbTreeContextBase } from '../tree.context';
import { UmbSectionContext } from '../../sections/section.context';
import { Entity } from '../../../mocks/data/entities';
import { UmbObserverMixin } from '../../../core/observer';
import type { ManifestSection } from '../../../core/models';
import { UmbTreeDataContextBase } from '../tree-data.context';
import { UmbTreeContextMenuService } from './context-menu/tree-context-menu.service';

@customElement('umb-tree-item')
export class UmbTreeItem extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles = [UUITextStyles, css``];

	@property({ type: Object, attribute: false })
	treeItem!: Entity;

	@state()
	private _childItems: Entity[] = [];

	@state()
	private _href = '';

	@state()
	private _loading = false;

	@state()
	private _selectable = false;

	@state()
	private _selected = false;

	@state()
	private _isActive = false;

	private _treeContext?: UmbTreeContextBase;
	private _treeDataContext?: UmbTreeDataContextBase;
	private _sectionContext?: UmbSectionContext;
	private _treeContextMenuService?: UmbTreeContextMenuService;

	constructor() {
		super();

		this.consumeContext('umbTreeContext', (treeContext: UmbTreeContextBase) => {
			this._treeContext = treeContext;
			this._observeSelectable();
			this._observeSelection();
		});

		this.consumeContext('umbTreeDataContext', (treeDataContext: UmbTreeDataContextBase) => {
			this._treeDataContext = treeDataContext;
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
		this.addEventListener('selected', (e) => {
			e.stopPropagation();
			this._treeContext?.select(this.treeItem.key);
			this.dispatchEvent(new CustomEvent('change', { composed: true, bubbles: true }));
		});
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

	private _observeSelection() {
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
		return `/section/${sectionPathname}/${type}/${key}`;
	}

	private _onShowChildren(event: UUIMenuItemEvent) {
		event.stopPropagation();
		if (this._childItems.length > 0) return;

		this._loading = true;

		if (!this._treeDataContext) return;

		this.observe<Entity[]>(this._treeDataContext.childrenChanges(this.treeItem.key), (childItems) => {
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
				href="${this._href}"
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
