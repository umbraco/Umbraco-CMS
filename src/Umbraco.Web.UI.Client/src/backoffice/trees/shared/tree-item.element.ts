import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../core/context';
import { UmbTreeContextBase } from '../tree.context';
import { UUIMenuItemEvent } from '@umbraco-ui/uui';
import { UmbSectionContext } from '../../sections/section.context';
import { map, Subscription } from 'rxjs';
import { Entity } from '../../../mocks/data/entity.data';
import { UmbTreeContextMenuService } from './context-menu/tree-context-menu.service';
import { repeat } from 'lit/directives/repeat.js';

@customElement('umb-tree-item')
export class UmbTreeItem extends UmbContextConsumerMixin(LitElement) {
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
	private _sectionContext?: UmbSectionContext;
	private _treeContextMenuService?: UmbTreeContextMenuService;

	private _sectionSubscription?: Subscription;
	private _childrenSubscription?: Subscription;
	private _selectableSubscription?: Subscription;
	private _selectionSubscription?: Subscription;
	private _activeTreeItemSubscription?: Subscription;

	constructor() {
		super();

		this.consumeContext('umbTreeContext', (treeContext: UmbTreeContextBase) => {
			this._treeContext = treeContext;
			this._observeSelectable();
			this._observeSelection();
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
		});
	}

	private _observeSection() {
		this._sectionSubscription?.unsubscribe();

		this._sectionSubscription = this._sectionContext?.data.subscribe((section) => {
			this._href = this._constructPath(section.meta.pathname, this.treeItem.type, this.treeItem.key);
		});
	}

	private _observeSelectable() {
		this._selectableSubscription?.unsubscribe();
		this._selectableSubscription = this._treeContext?.selectable?.subscribe((value) => {
			this._selectable = value;
		});
	}

	private _observeSelection() {
		this._selectionSubscription?.unsubscribe();
		this._selectionSubscription = this._treeContext?.selection
			.pipe(map((keys) => keys.includes(this.treeItem.key)))
			.subscribe((isSelected) => {
				this._selected = isSelected;
			});
	}

	private _observeActiveTreeItem() {
		this._activeTreeItemSubscription?.unsubscribe();

		this._activeTreeItemSubscription = this._sectionContext?.activeTreeItem.subscribe((treeItem) => {
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

		this._childrenSubscription?.unsubscribe();

		this._childrenSubscription = this._treeContext?.childrenChanges(this.treeItem.key).subscribe((items) => {
			if (items?.length === 0) return;
			this._childItems = items;
			this._loading = false;
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._sectionSubscription?.unsubscribe();
		this._childrenSubscription?.unsubscribe();
		this._selectableSubscription?.unsubscribe();
		this._selectionSubscription?.unsubscribe();
		this._activeTreeItemSubscription?.unsubscribe();
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
