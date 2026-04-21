import type { UmbTreeContext } from '../../tree.context.interface.js';
import type { UmbTreeItemModel, UmbTreeRootModel } from '../../types.js';
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

	@state()
	private _items: UmbTreeItemModel[] = [];

	@state()
	private _selectable = false;

	@state()
	private _selection: Array<string | null> = [];

	@state()
	private _expansion: UmbTreeExpansionModel = [];

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
			this.#treeContext.rootItems,
			(items) => {
				// Only use rootItems when not drilled in.
				if (this._expansion.length === 0) {
					this._items = items ?? [];
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
			return;
		}

		const repository = this.#treeContext?.getRepository();
		if (!repository) return;

		const { data } = await repository.requestTreeItemsOf({
			parent: { unique: currentParent.unique, entityType: currentParent.entityType },
		});

		this._items = (data?.items as UmbTreeItemModel[]) ?? [];
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
		const current = this.#treeContext?.expansion.getExpansion() ?? [];
		this.#treeContext?.expansion.setExpansion([...current, { unique: e.unique, entityType: e.entityType }]);
	}

	override render() {
		if (!this._items.length) return nothing;

		return html`
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
							.selected=${this._selection.includes(item.unique)}></umb-tree-item-card>
					`,
				)}
			</div>
		`;
	}

	static override styles = css`
		:host {
			display: contents;
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
