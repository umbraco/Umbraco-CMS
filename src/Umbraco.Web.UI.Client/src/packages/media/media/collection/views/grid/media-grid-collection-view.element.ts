import { UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN } from '../../../paths.js';
import type { UmbMediaCollectionItemModel } from '../../types.js';
import type { UmbMediaCollectionContext } from '../../media-collection.context.js';
import { UMB_MEDIA_COLLECTION_CONTEXT } from '../../media-collection.context-token.js';
import { UMB_MEDIA_PLACEHOLDER_ENTITY_TYPE } from '../../../entity.js';
import { css, customElement, html, ifDefined, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbFileDropzoneItemStatus } from '@umbraco-cms/backoffice/dropzone';
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';

import '@umbraco-cms/backoffice/imaging';

@customElement('umb-media-grid-collection-view')
export class UmbMediaGridCollectionViewElement extends UmbLitElement {
	@state()
	private _workspacePathBuilder?: UmbModalRouteBuilder;

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
			this.observe(
				collectionContext?.workspacePathBuilder,
				(builder) => {
					this._workspacePathBuilder = builder;
				},
				'observePath',
			);
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

	#onToggleSelect(item: UmbMediaCollectionItemModel) {
		if (item.unique) {
			this.#collectionContext?.selection.toggleSelect(item.unique);
		}
	}

	#isSelected(item: UmbMediaCollectionItemModel) {
		return this.#collectionContext?.selection.isSelected(item.unique);
	}

	#getEditUrl(item: UmbMediaCollectionItemModel) {
		return item.unique && this._workspacePathBuilder
			? this._workspacePathBuilder({ entityType: item.entityType }) +
					UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN.generateLocal({
						unique: item.unique,
					})
			: '';
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
			<div class="media-card">
				<uui-checkbox ?checked=${this.#isSelected(item)} @change=${() => this.#onToggleSelect(item)}></uui-checkbox>
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
						unique=${item.unique}
						alt=${ifDefined(item.name)}
						icon=${ifDefined(item.icon)}></umb-imaging-thumbnail>
				</uui-card-media>
			</div>
		`;
	}

	#renderPlaceholder(item: UmbMediaCollectionItemModel) {
		const complete = item.status === UmbFileDropzoneItemStatus.COMPLETE;
		const error = item.status !== UmbFileDropzoneItemStatus.WAITING && !complete;
		return html`
			<div class="media-card">
				<uui-card-media disabled class="media-placeholder-item" name=${ifDefined(item.name)}>
					<umb-temporary-file-badge
						.progress=${item.progress ?? 0}
						?complete=${complete}
						?error=${error}></umb-temporary-file-badge>
				</uui-card-media>
			</div>
		`;
	}

	static override styles = [
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

				.media-card {
					display: flex;
					position: relative;

					> uui-checkbox {
						position: absolute;
						top: var(--uui-size-space-4);
						left: var(--uui-size-space-4);
						opacity: 0;
						transition: opacity 120ms;
						z-index: 2;
					}

					&:has(:focus, :focus-within, :hover) > uui-checkbox,
					> uui-checkbox[checked] {
						opacity: 1;
					}
				}
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
