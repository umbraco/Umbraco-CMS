import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../core/context';
import { ITreeService } from '../tree.service';
import { UUIMenuItemEvent } from '@umbraco-ui/uui';

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

	private _treeService?: ITreeService;

	constructor() {
		super();

		this.consumeContext('umbTreeService', (treeService: ITreeService) => {
			this._treeService = treeService;
			this._pathName = this._treeService?.tree?.meta?.pathname;
		});
	}

	// TODO: how do we handle this?
	private _constructPath(id: number) {
		return `/section/members/${this._pathName}/${id}`;
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
				label=${this.label}
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
