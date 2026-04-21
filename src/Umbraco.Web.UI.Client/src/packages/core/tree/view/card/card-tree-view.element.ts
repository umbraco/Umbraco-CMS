import type { UmbTreeContext } from '../../tree.context.interface.js';
import type { UmbTreeItemModel } from '../../types.js';
import type { UmbTreeExpansionModel } from '../../expansion-manager/types.js';
import { UMB_TREE_CONTEXT } from '../../tree.context.token.js';
import type { UmbTreeItemOpenEvent } from '../../tree-item/events/tree-item-open.event.js';
import type { UmbEntityExpansionEntryModel } from '@umbraco-cms/backoffice/utils';
import type { UmbSelectedEvent, UmbDeselectedEvent } from '@umbraco-cms/backoffice/event';
import { css, customElement, html, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import '../../tree-item-card/tree-item-card.element.js';

@customElement('umb-card-tree-view')
export class UmbCardTreeViewElement extends UmbLitElement {
	#treeContext?: UmbTreeContext;
	#nameCache = new Map<string | null, string>();

	@state()
	private _items: UmbTreeItemModel[] = [];

	@state()
	private _selectable = false;

	@state()
	private _selectOnly = false;

	@state()
	private _selection: Array<string | null> = [];

	@state()
	private _expansion: UmbTreeExpansionModel = [];

	@state()
	private _rootName = '';

	constructor() {
		super();
		this.consumeContext(UMB_TREE_CONTEXT, (context) => {
			this.#treeContext = context;
			this.#observeContext();
		});
	}

	#observeContext() {
		if (!this.#treeContext) return;

		this.observe(
			this.#treeContext.treeRoot,
			(root) => {
				this._rootName = root?.name ?? '';
				this.#nameCache.set(null, root?.name ?? '');
			},
			'_observeRoot',
		);

		this.observe(
			this.#treeContext.rootItems,
			(items) => {
				// Only use rootItems when not drilled in.
				if (this._expansion.length === 0) {
					this._items = items ?? [];
					this.#cacheItemNames(items ?? []);
				}
			},
			'_observeRootItems',
		);

		this.observe(
			this.#treeContext.selection.selectable,
			(selectable) => (this._selectable = selectable ?? false),
			'_observeSelectable',
		);

		this.observe(
			this.#treeContext.selectOnly,
			(selectOnly) => (this._selectOnly = selectOnly ?? false),
			'_observeSelectOnly',
		);

		this.observe(
			this.#treeContext.selection.selection,
			(selection) => (this._selection = selection ?? []),
			'_observeSelection',
		);

		this.observe(
			this.#treeContext.expansion.expansion,
			async (expansion) => {
				this._expansion = expansion ?? [];
				await this.#loadItemsForCurrentLocation();
			},
			'_observeExpansion',
		);
	}

	#cacheItemNames(items: UmbTreeItemModel[]) {
		for (const item of items) {
			this.#nameCache.set(item.unique, item.name);
		}
	}

	async #loadItemsForCurrentLocation() {
		const currentParent = this.#getCurrentParent();

		if (!currentParent) {
			// At the root — items come from the context's rootItems observable.
			this._items = this.#treeContext?.rootItems
				? await new Promise((resolve) => {
						const sub = this.#treeContext!.rootItems.subscribe((items) => {
							resolve(items ?? []);
							sub.unsubscribe();
						});
					})
				: [];
			this.#cacheItemNames(this._items);
			return;
		}

		const repository = this.#treeContext?.getRepository();
		if (!repository) return;

		const { data } = await repository.requestTreeItemsOf({
			parent: { unique: currentParent.unique, entityType: currentParent.entityType },
		});

		this._items = (data?.items as UmbTreeItemModel[]) ?? [];
		this.#cacheItemNames(this._items);
	}

	#getCurrentParent(): UmbEntityExpansionEntryModel | undefined {
		return this._expansion.length > 0 ? this._expansion[this._expansion.length - 1] : undefined;
	}

	#onSelected(e: UmbSelectedEvent) {
		e.stopPropagation();
		this.#treeContext?.selection.select(e.unique);
	}

	#onDeselected(e: UmbDeselectedEvent) {
		e.stopPropagation();
		this.#treeContext?.selection.deselect(e.unique);
	}

	#onOpen(e: UmbTreeItemOpenEvent) {
		e.stopPropagation();
		// Cache the name before _items is replaced by the drill-in load.
		const item = this._items.find((i) => i.unique === e.unique);
		if (item) this.#nameCache.set(e.unique, item.name);

		const current = this.#treeContext?.expansion.getExpansion() ?? [];
		this.#treeContext?.expansion.setExpansion([...current, { unique: e.unique, entityType: e.entityType }]);
	}

	#navigateTo(index: number) {
		this.#treeContext?.expansion.setExpansion(this._expansion.slice(0, index + 1));
	}

	#navigateToRoot() {
		this.#treeContext?.expansion.setExpansion([]);
	}

	override render() {
		return html`
			${this.#renderBreadcrumb()}
			${this._items.length
				? html`
						<div
							id="grid"
							@selected=${this.#onSelected}
							@deselected=${this.#onDeselected}
							@umb-tree-item-open=${this.#onOpen}>
							${repeat(
								this._items,
								(item) => item.unique,
								(item) => html`
									<umb-tree-item-card
										.entityType=${item.entityType}
										.item=${item}
										.selectable=${this._selectable}
										.selectOnly=${this._selectOnly}
										.selected=${this._selection.includes(item.unique)}></umb-tree-item-card>
								`,
							)}
						</div>
					`
				: nothing}
		`;
	}

	#renderBreadcrumb() {
		if (!this._expansion.length) return nothing;

		return html`
			<uui-breadcrumbs>
				<uui-breadcrumb-item @click=${this.#navigateToRoot}>${this._rootName}</uui-breadcrumb-item>
				${this._expansion.map((entry, index) => {
					const isLast = index === this._expansion.length - 1;
					const name = this.#nameCache.get(entry.unique) ?? '...';
					return html`
						<uui-breadcrumb-item ?last-item=${isLast} @click=${!isLast ? () => this.#navigateTo(index) : undefined}>
							${name}
						</uui-breadcrumb-item>
					`;
				})}
			</uui-breadcrumbs>
		`;
	}

	static override styles = css`
		:host {
			display: block;
		}

		uui-breadcrumbs {
			padding: var(--uui-size-space-3) var(--uui-size-space-4) 0;
		}

		#grid {
			display: grid;
			grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
			gap: var(--uui-size-space-4);
			padding: var(--uui-size-space-4);
		}
	`;
}

export default UmbCardTreeViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-card-tree-view': UmbCardTreeViewElement;
	}
}
