import { UMB_MEDIA_COLLECTION_CONTEXT } from '../../media-collection.context-token.js';
import type { UmbMediaCollectionItemModel } from '../../types.js';
import { css, customElement, html, nothing, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbCollectionViewElementBase } from '@umbraco-cms/backoffice/collection';

@customElement('umb-media-grid-collection-view')
export class UmbMediaGridCollectionViewElement extends UmbCollectionViewElementBase<UmbMediaCollectionItemModel> {
	constructor() {
		super();
		this.consumeContext(UMB_MEDIA_COLLECTION_CONTEXT, (collectionContext) => {
			collectionContext?.setupView(this);
		});
	}

	override render() {
		if (this._loading) return nothing;
		return html`
			<div id="media-grid">
				${repeat(
					this._items,
					(item) => item.unique + item.status,
					(item) => this.#renderItem(item),
				)}
			</div>
		`;
	}

	#renderItem(item: UmbMediaCollectionItemModel) {
		const href = item.unique ? this._itemHrefs.get(item.unique) : undefined;
		return html` <umb-entity-collection-item-card
			.item=${item}
			href=${href ?? nothing}
			?selectable=${this._isSelectableItem(item)}
			?select-only=${this._selectOnly}
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

			#media-grid {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
				grid-auto-rows: 200px;
				gap: var(--uui-size-space-5);
			}
		`,
	];
}

export default UmbMediaGridCollectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-grid-collection-view': UmbMediaGridCollectionViewElement;
	}
}
