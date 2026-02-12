import { UmbCollectionViewElementBase } from '../umb-collection-view-element-base.js';
import type { UmbCollectionItemModel } from '../../types.js';
import { css, customElement, html, nothing, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-ref-collection-view')
export class UmbRefCollectionViewElement extends UmbCollectionViewElementBase {
	override render() {
		if (this._loading) return nothing;
		return html`
			<uui-box>
				${repeat(
					this._items,
					(item) => item.unique,
					(item) => this.#renderItem(item),
				)}
			</uui-box>
		`;
	}

	#renderItem(item: UmbCollectionItemModel) {
		const href = item.unique ? this._itemHrefs.get(item.unique) : undefined;
		return html`<umb-entity-collection-item-ref
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
		</umb-entity-collection-item-ref>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
			}

			umb-entity-collection-item-ref {
				margin-bottom: var(--uui-size-4);

				&:last-of-type {
					margin-bottom: 0;
				}
			}
		`,
	];
}

export { UmbRefCollectionViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-ref-collection-view': UmbRefCollectionViewElement;
	}
}
