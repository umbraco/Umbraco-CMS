import type { UmbTreeItemModel } from '../../types.js';
import type { UmbTreeExpansionModel } from '../../expansion-manager/types.js';
import type { UmbTreeItemOpenEvent } from '../../tree-item/events/tree-item-open.event.js';
import { UmbTreeViewElementBase } from '../tree-view-element-base.js';
import type { UmbEntityExpansionEntryModel } from '@umbraco-cms/backoffice/utils';
import type { UmbSelectedEvent, UmbDeselectedEvent } from '@umbraco-cms/backoffice/event';
import { css, customElement, html, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';

import '../../tree-item-card/tree-item-card.element.js';

@customElement('umb-card-tree-view')
export class UmbCardTreeViewElement extends UmbTreeViewElementBase {
	#nameCache = new Map<string | null, string>();

	@state()
	private _items: UmbTreeItemModel[] = [];

	@state()
	private _expansion: UmbTreeExpansionModel = [];

	protected override _observeContext() {
		super._observeContext();

		this.observe(
			this._treeContext?.treeRoot,
			(root) => this.#nameCache.set(null, root?.name ?? ''),
			'_observeRootCache',
		);

		this.observe(
			this._treeContext?.rootItems,
			(items) => {
				if (this._expansion.length === 0) {
					this._items = items ?? [];
					this.#cacheItemNames(items ?? []);
				}
			},
			'_observeRootItems',
		);

		this.observe(
			this._treeContext?.expansion.expansion,
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
			this._items = this._treeContext?.rootItems
				? await new Promise((resolve) => {
						const sub = this._treeContext!.rootItems.subscribe((items) => {
							resolve(items ?? []);
							sub.unsubscribe();
						});
					})
				: [];
			this.#cacheItemNames(this._items);
			return;
		}

		const repository = this._treeContext?.getRepository();
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

	#onOpen(e: UmbTreeItemOpenEvent) {
		e.stopPropagation();
		const item = this._items.find((i) => i.unique === e.unique);
		if (item) this.#nameCache.set(e.unique, item.name);

		const current = this._treeContext?.expansion.getExpansion() ?? [];
		this._treeContext?.expansion.setExpansion([...current, { unique: e.unique, entityType: e.entityType }]);
	}

	#navigateTo(index: number) {
		this._treeContext?.expansion.setExpansion(this._expansion.slice(0, index + 1));
	}

	#navigateToRoot() {
		this._treeContext?.expansion.setExpansion([]);
	}

	override render() {
		return html`
			${this.#renderBreadcrumb()}
			${this._items.length
				? html`
						<div
							id="grid"
							@selected=${(e: UmbSelectedEvent) => this._selectItem(e.unique)}
							@deselected=${(e: UmbDeselectedEvent) => this._deselectItem(e.unique)}
							@umb-tree-item-open=${this.#onOpen}>
							${repeat(
								this._items,
								(item) => item.unique,
								(item) => html`
									<umb-tree-item-card
										.entityType=${item.entityType}
										.item=${item}
										.selectable=${this._isSelectableItem(item)}
										.selectOnly=${this._selectOnly}
										.selected=${this._isSelectedItem(item.unique)}></umb-tree-item-card>
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
				<uui-breadcrumb-item @click=${this.#navigateToRoot}>${this._treeRoot?.name ?? ''}</uui-breadcrumb-item>
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
