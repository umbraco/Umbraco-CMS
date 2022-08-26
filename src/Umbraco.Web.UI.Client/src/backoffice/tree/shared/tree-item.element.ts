import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../core/context';
import { ITreeService } from '../tree.service';
import { UUIMenuItemEvent } from '@umbraco-ui/uui';
import { UmbSectionContext } from '../../sections/section.context';
import { map, Subscription } from 'rxjs';
import { UmbEntityStore } from '../../../core/stores/entity.store';

@customElement('umb-tree-item')
export class UmbTreeItem extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	@property({ type: Boolean })
	hasChildren = false;

	@property({ type: Number })
	itemId = -1;

	@property({ type: String })
	label = '';

	@property({ type: String })
	href = '';

	@state()
	childItems: any[] = [];

	@state()
	private _loading = false;

	@state()
	private _pathName? = '';

	@state()
	private _sectionPathname?: string;

	private _treeService?: ITreeService;

	private _entityStore?: UmbEntityStore;

	private _sectionContext?: UmbSectionContext;
	private _sectionSubscription?: Subscription;

	@state()
	private _itemName = '';

	constructor() {
		super();

		this.consumeContext('umbTreeService', (treeService: ITreeService) => {
			this._treeService = treeService;
			this._pathName = this._treeService?.tree?.meta?.pathname;
		});

		this.consumeContext('umbSectionContext', (sectionContext: UmbSectionContext) => {
			this._sectionContext = sectionContext;
			this._useSection();
		});

		this.consumeContext('umbEntityStore', (entityStore: UmbEntityStore) => {
			this._entityStore = entityStore;

			this._entityStore?.entities
				.pipe(map((items) => items.filter((item) => item.id === this.itemId)))
				.subscribe((items) => {
					this._itemName = items?.[0]?.name;
					console.log(this.label, items);
				});
		});
	}

	private _useSection() {
		this._sectionSubscription?.unsubscribe();

		this._sectionSubscription = this._sectionContext?.data.subscribe((section) => {
			this._sectionPathname = section.meta.pathname;
		});
	}

	// TODO: how do we handle this?
	private _constructPath(id: number) {
		return `/section/${this._sectionPathname}/${this._pathName}/${id}`;
	}

	private _onShowChildren(event: UUIMenuItemEvent) {
		event.stopPropagation();
		if (this.childItems.length > 0) return;

		this._loading = true;

		this._treeService?.getChildren(this.itemId).then((items) => {
			this.childItems = items;
			this._loading = false;
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._sectionSubscription?.unsubscribe();
	}

	private _renderChildItems() {
		return this.childItems.map((item) => {
			return html`<umb-tree-item
				.label=${item.name}
				.hasChildren=${item.hasChildren}
				.itemId=${item.id}
				href="${this._constructPath(item.id)}">
			</umb-tree-item>`;
		});
	}

	render() {
		return html`
			<uui-menu-item
				@show-children=${this._onShowChildren}
				.loading=${this._loading}
				.hasChildren=${this.hasChildren}
				label="${this._itemName}"
				href="${this._constructPath(this.itemId)}">
				${this._renderChildItems()}
			</uui-menu-item>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-item': UmbTreeItem;
	}
}
