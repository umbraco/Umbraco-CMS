import type { UmbTreeItemModel } from '../../types.js';
import { UmbTreeViewElementBase } from '../tree-view-element-base.js';
import { css, customElement, html, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';

import '../../tree-item-card/tree-item-card.element.js';

@customElement('umb-card-tree-view')
export class UmbCardTreeViewElement extends UmbTreeViewElementBase {
	@state()
	private _items: UmbTreeItemModel[] = [];

	protected override _observeContext() {
		super._observeContext();

		this.observe(
			this._treeContext?.rootItems,
			(items) => {
				this._items = items ?? [];
			},
			'_observeRootItems',
		);
	}

	override render() {
		if (!this._items.length) return nothing;

		return html`
			<div id="grid">
				${repeat(
					this._items,
					(item) => item.unique,
					(item) => html` <umb-tree-item-card .entityType=${item.entityType} .item=${item}></umb-tree-item-card> `,
				)}
			</div>
		`;
	}

	static override styles = css`
		:host {
			display: block;
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
