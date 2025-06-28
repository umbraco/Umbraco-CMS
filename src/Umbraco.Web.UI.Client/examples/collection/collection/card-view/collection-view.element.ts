import type { ExampleCollectionItemModel } from '../repository/types.js';
import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import { UMB_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import { css, html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('example-card-collection-view')
export class ExampleCardCollectionViewElement extends UmbLitElement {
	@state()
	private _items: Array<ExampleCollectionItemModel> = [];

	#collectionContext?: UmbDefaultCollectionContext<ExampleCollectionItemModel>;

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
			this.#observeCollectionItems();
		});
	}

	#observeCollectionItems() {
		this.observe(this.#collectionContext?.items, (items) => (this._items = items || []), 'umbCollectionItemsObserver');
	}

	override render() {
		return html`
			<div id="card-grid">
				${repeat(
					this._items,
					(item) => item.unique,
					(item) =>
						html` <uui-card>
							<uui-icon name="icon-newspaper"></uui-icon>
							<div>${item.name}</div>
						</uui-card>`,
				)}
			</div>
		`;
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
				grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
				grid-auto-rows: 200px;
				gap: var(--uui-size-space-5);
			}

			uui-card {
				display: flex;
				flex-direction: column;
				align-items: center;
				justify-content: center;
				text-align: center;
				height: 100%;

				uui-icon {
					font-size: 2em;
					margin-bottom: var(--uui-size-space-4);
				}
			}
		`,
	];
}

export { ExampleCardCollectionViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'example-card-collection-view': ExampleCardCollectionViewElement;
	}
}
