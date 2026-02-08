import { UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN } from '../../../paths.js';
import { UMB_MEDIA_COLLECTION_CONTEXT } from '../../media-collection.context-token.js';
import { UMB_MEDIA_PLACEHOLDER_ENTITY_TYPE } from '../../../entity.js';
import type { UmbMediaCollectionContext } from '../../media-collection.context.js';
import type { UmbMediaCollectionItemModel } from '../../types.js';
import { css, customElement, html, ifDefined, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbFileDropzoneItemStatus } from '@umbraco-cms/backoffice/dropzone';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

import '@umbraco-cms/backoffice/imaging';

@customElement('umb-media-grid-collection-view')
export class UmbMediaGridCollectionViewElement extends UmbLitElement {
	@state()
	private _items: Array<UmbMediaCollectionItemModel> = [];

	@state()
	private _selection: Array<string | null> = [];

	#collectionContext?: UmbMediaCollectionContext;

	constructor() {
		super();
		this.consumeContext(UMB_MEDIA_COLLECTION_CONTEXT, (collectionContext) => {
			this.#collectionContext = collectionContext;
			collectionContext?.setupView(this);
			this.#observeCollectionContext();
		});
	}

	#observeCollectionContext() {
		if (!this.#collectionContext) return;

		this.observe(this.#collectionContext.items, (items) => (this._items = items), '_observeItems');

		this.observe(
			this.#collectionContext.selection.selection,
			(selection) => (this._selection = selection),
			'_observeSelection',
		);
	}

	#onSelect(item: UmbMediaCollectionItemModel) {
		if (item.unique) {
			this.#collectionContext?.selection.select(item.unique);
		}
	}

	#onDeselect(item: UmbMediaCollectionItemModel) {
		if (item.unique) {
			this.#collectionContext?.selection.deselect(item.unique);
		}
	}

	#isSelected(item: UmbMediaCollectionItemModel) {
		return this.#collectionContext?.selection.isSelected(item.unique);
	}

	#getEditUrl(item: UmbMediaCollectionItemModel) {
		return UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: item.unique });
	}

	override render() {
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
		if (item.entityType === UMB_MEDIA_PLACEHOLDER_ENTITY_TYPE) {
			return this.#renderPlaceholder(item);
		}
		return html`
			<uui-card-media
				name=${ifDefined(item.name)}
				data-mark="${item.entityType}:${item.unique}"
				selectable
				?select-only=${this._selection.length > 0}
				?selected=${this.#isSelected(item)}
				href=${this.#getEditUrl(item)}
				@selected=${() => this.#onSelect(item)}
				@deselected=${() => this.#onDeselect(item)}>
				<umb-imaging-thumbnail
					.unique=${item.unique}
					alt=${ifDefined(item.name)}
					icon=${ifDefined(item.icon)}></umb-imaging-thumbnail>
			</uui-card-media>
		`;
	}

	#renderPlaceholder(item: UmbMediaCollectionItemModel) {
		const complete = item.status === UmbFileDropzoneItemStatus.COMPLETE;
		const error = item.status !== UmbFileDropzoneItemStatus.WAITING && !complete;
		return html`<uui-card-media disabled class="media-placeholder-item" name=${ifDefined(item.name)}>
			<umb-temporary-file-badge
				.progress=${item.progress ?? 0}
				?complete=${complete}
				?error=${error}></umb-temporary-file-badge>
		</uui-card-media>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
			}

			.container {
				display: flex;
				justify-content: center;
				align-items: center;
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
