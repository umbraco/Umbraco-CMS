import { UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN } from '../../../paths.js';
import type { UmbMediaCollectionItemModel } from '../../types.js';
import type { UmbMediaCollectionContext } from '../../media-collection.context.js';
import { UMB_MEDIA_COLLECTION_CONTEXT } from '../../media-collection.context-token.js';
import { UmbFileDropzoneItemStatus } from '../../../dropzone/types.js';
import { UMB_MEDIA_PLACEHOLDER_ENTITY_TYPE } from '../../../entity.js';
import { css, customElement, html, ifDefined, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';

import '@umbraco-cms/backoffice/imaging';

@customElement('umb-media-grid-collection-view')
export class UmbMediaGridCollectionViewElement extends UmbLitElement {
	@state()
	private _editMediaPath = '';

	@state()
	private _items: Array<UmbMediaCollectionItemModel> = [];

	@state()
	private _selection: Array<string | null> = [];

	#collectionContext?: UmbMediaCollectionContext;

	constructor() {
		super();
		this.consumeContext(UMB_MEDIA_COLLECTION_CONTEXT, (collectionContext) => {
			this.#collectionContext = collectionContext;
			this.#observeCollectionContext();
		});

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('media')
			.onSetup(() => {
				return { data: { entityType: 'media', preset: {} } };
			})
			.onReject(() => {
				this.#collectionContext?.requestCollection();
			})
			.onSubmit(() => {
				this.#collectionContext?.requestCollection();
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editMediaPath = routeBuilder({});
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

	#onOpen(event: Event, unique: string) {
		event.preventDefault();
		event.stopPropagation();

		const url = this._editMediaPath + UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN.generateLocal({ unique });
		window.history.pushState(null, '', url);
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
				selectable
				?select-only=${this._selection && this._selection.length > 0}
				?selected=${this.#isSelected(item)}
				@open=${(event: Event) => this.#onOpen(event, item.unique)}
				@selected=${() => this.#onSelect(item)}
				@deselected=${() => this.#onDeselect(item)}
				class="media-item">
				<umb-imaging-thumbnail
					unique=${item.unique}
					alt=${ifDefined(item.name)}
					icon=${ifDefined(item.icon)}></umb-imaging-thumbnail>
			</uui-card-media>
		`;
	}

	#renderPlaceholder(item: UmbMediaCollectionItemModel) {
		const complete = item.status === UmbFileDropzoneItemStatus.COMPLETE;
		return html`<uui-card-media disabled class="media-placeholder-item" name=${ifDefined(item.name)}>
			<umb-temporary-file-badge ?complete=${complete}></umb-temporary-file-badge>
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

			.media-placeholder-item {
				font-style: italic;
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
