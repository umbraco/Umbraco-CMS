import { UmbCollectionViewElementBase } from '../umb-collection-view-element-base.js';
import type { UmbCollectionItemModel } from '../../types.js';
import { css, customElement, html, nothing, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-card-collection-view')
export class UmbCardCollectionViewElement extends UmbCollectionViewElementBase {
	override render() {
		if (this._loading) return nothing;
		return html`
			<div id="card-grid">
				${repeat(
					this._items,
					(item) => item.unique,
					(item) => this.#renderItem(item),
				)}
			</div>
		`;
	}

	#renderItem(item: UmbCollectionItemModel) {
		const href = item.unique ? this._itemHrefs.get(item.unique) : undefined;
		return html` <umb-entity-collection-item-card
			.item=${item}
			href=${href ?? nothing}
			?selectable=${this._selectable}
			?select-only=${this._selection.length > 0}
			?selected=${this._isSelectedItem(item.unique)}
			@selected=${() => this._selectItem(item.unique)}
			@deselected=${() => this._deselectItem(item.unique)}>
			<umb-entity-actions-bundle
				slot="actions"
				.entityType=${item.entityType}
				.unique=${item.unique}></umb-entity-actions-bundle>
		</umb-entity-collection-item-card>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
			}

			#card-grid {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
				gap: var(--uui-size-space-4);
			}
		`,
	];
}

export { UmbCardCollectionViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-card-collection-view': UmbCardCollectionViewElement;
	}
}
