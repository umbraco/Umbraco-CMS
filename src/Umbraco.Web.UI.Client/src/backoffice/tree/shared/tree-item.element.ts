import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../core/context';
import { UmbTreeContext } from '../tree.context';
import { UUIMenuItemEvent } from '@umbraco-ui/uui';
import { UmbSectionContext } from '../../sections/section.context';
import { Subscription } from 'rxjs';
import { Entity } from '../../../mocks/data/entity.data';
import { UmbActionService } from '../actions.service';

@customElement('umb-tree-item')
export class UmbTreeItem extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	@property()
	itemKey = '';

	@property()
	itemType = '';

	@property({ type: String })
	label = '';

	@property({ type: Boolean })
	hasChildren = false;

	@state()
	private _childItems: Entity[] = [];

	@state()
	private _href = '';

	@state()
	private _loading = false;

	private _treeContext?: UmbTreeContext;

	private _sectionContext?: UmbSectionContext;
	private _sectionSubscription?: Subscription;
	private _childrenSubscription?: Subscription;
	private _actionService?: UmbActionService;

	@state()
	private _itemName = '';

	constructor() {
		super();

		this.consumeContext('umbTreeContext', (treeContext: UmbTreeContext) => {
			this._treeContext = treeContext;
		});

		this.consumeContext('umbSectionContext', (sectionContext: UmbSectionContext) => {
			this._sectionContext = sectionContext;
			this._useSection();
		});

		this.consumeContext('umbActionService', (actionService: UmbActionService) => {
			this._actionService = actionService;
		});
	}

	private _useSection() {
		this._sectionSubscription?.unsubscribe();

		this._sectionSubscription = this._sectionContext?.data.subscribe((section) => {
			this._href = this._constructPath(section.meta.pathname, this.itemType, this.itemKey);
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

		this._childrenSubscription = this._treeContext?.fetchChildren(this.itemKey).subscribe((items) => {
			if (items?.length === 0) return;
			this._childItems = items;
			this._loading = false;
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._sectionSubscription?.unsubscribe();
		this._childrenSubscription?.unsubscribe();
	}

	private _renderChildItems() {
		return this._childItems.map((item) => {
			return html`<umb-tree-item
				.label=${item.name}
				.hasChildren=${item.hasChildren}
				.itemKey=${item.key}
				.itemType=${item.type}>
			</umb-tree-item>`;
		});
	}

	private _openActions() {
		this._actionService?.open(this.label);
	}

	render() {
		return html`
			<uui-menu-item
				@show-children=${this._onShowChildren}
				.loading=${this._loading}
				.hasChildren=${this.hasChildren}
				label="${this.label}"
				href="${this._href}">
				${this._renderChildItems()}
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
