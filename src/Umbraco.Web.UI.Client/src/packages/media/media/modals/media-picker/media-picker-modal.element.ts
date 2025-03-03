import { UmbMediaItemRepository } from '../../repository/index.js';
import { UmbMediaTreeRepository } from '../../tree/media-tree.repository.js';
import { UMB_MEDIA_ROOT_ENTITY_TYPE } from '../../entity.js';
import type { UmbDropzoneElement } from '../../dropzone/dropzone.element.js';
import type { UmbMediaTreeItemModel, UmbMediaSearchItemModel, UmbMediaItemModel } from '../../types.js';
import { UmbMediaSearchProvider } from '../../search/index.js';
import type { UmbMediaPathModel } from './types.js';
import type { UmbMediaPickerFolderPathElement } from './components/media-picker-folder-path.element.js';
import type { UmbMediaPickerModalData, UmbMediaPickerModalValue } from './media-picker-modal.token.js';
import {
	css,
	html,
	customElement,
	state,
	repeat,
	ifDefined,
	query,
	type PropertyValues,
	nothing,
} from '@umbraco-cms/backoffice/external/lit';
import { debounce, UmbPaginationManager } from '@umbraco-cms/backoffice/utils';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UMB_CONTENT_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/content';
import type { UUIInputEvent, UUIPaginationEvent } from '@umbraco-cms/backoffice/external/uui';
import { isUmbracoFolder } from '@umbraco-cms/backoffice/media-type';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

import '@umbraco-cms/backoffice/imaging';

const root: UmbMediaPathModel = { name: 'Media', unique: null, entityType: UMB_MEDIA_ROOT_ENTITY_TYPE };

// TODO: investigate how we can reuse the picker-search-field element, picker context etc.
@customElement('umb-media-picker-modal')
export class UmbMediaPickerModalElement extends UmbModalBaseElement<UmbMediaPickerModalData, UmbMediaPickerModalValue> {
	#mediaTreeRepository = new UmbMediaTreeRepository(this);
	#mediaItemRepository = new UmbMediaItemRepository(this);
	#mediaSearchProvider = new UmbMediaSearchProvider(this);

	#dataType?: { unique: string };

	@state()
	private _selectableFilter: (item: UmbMediaTreeItemModel | UmbMediaSearchItemModel) => boolean = () => true;

	@state()
	private _currentChildren: Array<UmbMediaTreeItemModel> = [];

	@state()
	private _currentPage = 1;

	@state()
	private _currentTotalPages = 0;

	@state()
	private _searchResult: Array<UmbMediaSearchItemModel> = [];

	@state()
	private _searchFrom: UmbEntityModel | undefined;

	@state()
	private _searchQuery = '';

	@state()
	private _currentMediaEntity: UmbMediaPathModel = root;

	@state()
	private _isSelectionMode = false;

	@state()
	private _startNode: UmbMediaItemModel | undefined;

	@state()
	_searching: boolean = false;

	@query('#dropzone')
	private _dropzone!: UmbDropzoneElement;

	#pagingMap = new Map<string, UmbPaginationManager>();

	constructor() {
		super();

		this.consumeContext(UMB_CONTENT_PROPERTY_CONTEXT, (context) => {
			this.observe(context.dataType, (dataType) => {
				this.#dataType = dataType;
			});
		});
	}

	override async connectedCallback(): Promise<void> {
		super.connectedCallback();
		if (this.data?.pickableFilter) {
			this._selectableFilter = this.data?.pickableFilter;
		}
	}

	protected override async firstUpdated(_changedProperties: PropertyValues): Promise<void> {
		super.firstUpdated(_changedProperties);

		const startNode = this.data?.startNode;

		if (startNode) {
			const { data } = await this.#mediaItemRepository.requestItems([startNode.unique]);
			this._startNode = data?.[0];

			if (this._startNode) {
				this._currentMediaEntity = {
					name: this._startNode.name,
					unique: this._startNode.unique,
					entityType: this._startNode.entityType,
				};

				this._searchFrom = { unique: this._startNode.unique, entityType: this._startNode.entityType };
			}
		}

		this.#loadChildrenOfCurrentMediaItem();
	}

	async #loadChildrenOfCurrentMediaItem() {
		const key = this._currentMediaEntity.entityType + this._currentMediaEntity.unique;
		let paginationManager = this.#pagingMap.get(key);

		if (!paginationManager) {
			paginationManager = new UmbPaginationManager();
			paginationManager.setPageSize(100);
			this.#pagingMap.set(key, paginationManager);
		}

		const skip = paginationManager.getSkip();
		const take = paginationManager.getPageSize();

		const { data } = await this.#mediaTreeRepository.requestTreeItemsOf({
			parent: {
				unique: this._currentMediaEntity.unique,
				entityType: this._currentMediaEntity.entityType,
			},
			dataType: this.#dataType,
			skip,
			take,
		});

		this._currentChildren = data?.items ?? [];
		paginationManager.setTotalItems(data?.total ?? 0);
		this._currentPage = paginationManager.getCurrentPageNumber();
		this._currentTotalPages = paginationManager.getTotalPages();
	}

	#onOpen(item: UmbMediaTreeItemModel | UmbMediaSearchItemModel) {
		this.#clearSearch();

		this._currentMediaEntity = {
			name: item.name,
			unique: item.unique,
			entityType: UMB_MEDIA_ROOT_ENTITY_TYPE,
		};

		// If the user has navigated into an item, we default to search only within that item.
		this._searchFrom = this._currentMediaEntity.unique
			? { unique: this._currentMediaEntity.unique, entityType: this._currentMediaEntity.entityType }
			: undefined;

		this.#loadChildrenOfCurrentMediaItem();
	}

	#onSelected(item: UmbMediaTreeItemModel | UmbMediaSearchItemModel) {
		const selection = this.data?.multiple ? [...this.value.selection, item.unique!] : [item.unique!];
		this._isSelectionMode = selection.length > 0;
		this.modalContext?.setValue({ selection });
	}

	#onDeselected(item: UmbMediaTreeItemModel | UmbMediaSearchItemModel) {
		const selection = this.value.selection.filter((value) => value !== item.unique);
		this._isSelectionMode = selection.length > 0;
		this.modalContext?.setValue({ selection });
	}

	#clearSearch() {
		this._searchQuery = '';
		this._searchResult = [];
	}

	async #searchMedia() {
		if (!this._searchQuery) {
			this.#clearSearch();
			this._searching = false;
			return;
		}

		const query = this._searchQuery;
		const { data } = await this.#mediaSearchProvider.search({
			query,
			searchFrom: this._searchFrom,
			...this.data?.search?.queryParams,
		});

		if (!data) {
			// No search results.
			this._searchResult = [];
			this._searching = false;
			return;
		}

		// Map urls for search results as we are going to show for all folders (as long they aren't trashed).
		this._searchResult = data.items;
		this._searching = false;
	}

	#debouncedSearch = debounce(() => {
		this.#searchMedia();
	}, 500);

	#onSearch(e: UUIInputEvent) {
		this._searchQuery = (e.target.value as string).toLocaleLowerCase();
		this._searching = true;
		this.#debouncedSearch();
	}

	#onPathChange(e: CustomEvent) {
		const newPath = e.target as UmbMediaPickerFolderPathElement;

		if (newPath.currentMedia) {
			this._currentMediaEntity = newPath.currentMedia;
		} else if (this._startNode) {
			this._currentMediaEntity = {
				name: this._startNode.name,
				unique: this._startNode.unique,
				entityType: this._startNode.entityType,
			};
		} else {
			this._currentMediaEntity = root;
		}

		if (this._currentMediaEntity.unique) {
			this._searchFrom = { unique: this._currentMediaEntity.unique, entityType: this._currentMediaEntity.entityType };
		} else {
			this._searchFrom = undefined;
		}

		this.#loadChildrenOfCurrentMediaItem();
	}

	#onPageChange(event: UUIPaginationEvent) {
		event.stopPropagation();
		const key = this._currentMediaEntity.entityType + this._currentMediaEntity.unique;

		const paginationManager = this.#pagingMap.get(key);

		if (!paginationManager) {
			throw new Error('Pagination manager not found');
		}

		paginationManager.setCurrentPageNumber(event.target.current);
		this.#pagingMap.set(key, paginationManager);

		this.#loadChildrenOfCurrentMediaItem();
	}

	#allowNavigateToMedia(item: UmbMediaTreeItemModel | UmbMediaSearchItemModel): boolean {
		return isUmbracoFolder(item.mediaType.unique) || item.hasChildren;
	}

	#onSearchFromChange(e: CustomEvent) {
		const checked = (e.target as HTMLInputElement).checked;

		if (checked) {
			this._searchFrom = { unique: this._currentMediaEntity.unique, entityType: this._currentMediaEntity.entityType };
		} else {
			this._searchFrom = undefined;
		}
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('defaultdialogs_chooseMedia')}>
				${this.#renderBody()} ${this.#renderBreadcrumb()}
				<div slot="actions">
					<uui-button label=${this.localize.term('general_close')} @click=${this._rejectModal}></uui-button>
					<uui-button
						label=${this.localize.term('general_choose')}
						look="primary"
						color="positive"
						@click=${this._submitModal}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	#renderBody() {
		return html`${this.#renderToolbar()}
			<umb-dropzone
				id="dropzone"
				multiple
				@complete=${() => this.#loadChildrenOfCurrentMediaItem()}
				.parentUnique=${this._currentMediaEntity.unique}></umb-dropzone>
			${this._searchQuery ? this.#renderSearchResult() : this.#renderCurrentChildren()} `;
	}

	#renderSearchResult() {
		return html`
			${!this._searchResult.length && !this._searching
				? html`<div class="container"><p>${this.localize.term('content_listViewNoItems')}</p></div>`
				: html`<div id="media-grid">
						${repeat(
							this._searchResult,
							(item) => item.unique,
							(item) => this.#renderCard(item),
						)}
					</div>`}
		`;
	}

	#renderCurrentChildren() {
		return html`
			${!this._currentChildren.length
				? html`<div class="container"><p>${this.localize.term('content_listViewNoItems')}</p></div>`
				: html`<div id="media-grid">
							${repeat(
								this._currentChildren,
								(item) => item.unique,
								(item) => this.#renderCard(item),
							)}
						</div>
						${this._currentTotalPages > 1
							? html`<uui-pagination
									.current=${this._currentPage}
									.total=${this._currentTotalPages}
									@change=${this.#onPageChange}></uui-pagination>`
							: nothing}`}
		`;
	}

	#renderToolbar() {
		/**<umb-media-picker-create-item .node=${this._currentMediaEntity.unique}></umb-media-picker-create-item>
		 * We cannot route to a workspace without the media picker modal is a routeable. Using regular upload button for now... */
		return html`
			<div id="toolbar">
				<div id="search">
					<uui-input
						label=${this.localize.term('general_search')}
						placeholder=${this.localize.term('placeholders_search')}
						@input=${this.#onSearch}
						value=${this._searchQuery}>
						<div slot="prepend">
							${this._searching
								? html`<uui-loader-circle id="searching-indicator"></uui-loader-circle>`
								: html`<uui-icon name="search"></uui-icon>`}
						</div>
					</uui-input>

					${this._currentMediaEntity.unique && this._currentMediaEntity.unique !== this._startNode?.unique
						? html`<uui-checkbox
								?checked=${this._searchFrom?.unique === this._currentMediaEntity.unique}
								@change=${this.#onSearchFromChange}
								label="Search only in ${this._currentMediaEntity.name}"></uui-checkbox>`
						: nothing}
				</div>
				<uui-button
					@click=${() => this._dropzone.browse()}
					label=${this.localize.term('general_upload')}
					look="outline"
					color="default"></uui-button>
			</div>
		`;
	}

	#renderCard(item: UmbMediaTreeItemModel | UmbMediaSearchItemModel) {
		const canNavigate = this.#allowNavigateToMedia(item);
		const selectable = this._selectableFilter(item);
		const disabled = !(selectable || canNavigate);
		return html`
			<uui-card-media
				class=${ifDefined(disabled ? 'not-allowed' : undefined)}
				.name=${item.name}
				data-mark="${item.entityType}:${item.unique}"
				@open=${() => this.#onOpen(item)}
				@selected=${() => this.#onSelected(item)}
				@deselected=${() => this.#onDeselected(item)}
				?selected=${this.value?.selection?.find((value) => value === item.unique)}
				?selectable=${selectable}
				?select-only=${this._isSelectionMode || canNavigate === false}>
				<umb-imaging-thumbnail
					unique=${item.unique}
					alt=${item.name}
					icon=${item.mediaType.icon}></umb-imaging-thumbnail>
			</uui-card-media>
		`;
	}

	#renderBreadcrumb() {
		// hide the breadcrumb when doing a global search within another item
		// We do this to avoid confusion that the current search result is within the item shown in the breadcrumb.
		if (this._searchQuery && this._currentMediaEntity.unique && !this._searchFrom) {
			return nothing;
		}

		const startNode: UmbMediaPathModel | undefined = this._startNode
			? {
					entityType: this._startNode.entityType,
					unique: this._startNode.unique,
					name: this._startNode.name,
				}
			: undefined;

		return html`<umb-media-picker-folder-path
			slot="footer-info"
			.currentMedia=${this._currentMediaEntity}
			.startNode=${startNode}
			@change=${this.#onPathChange}></umb-media-picker-folder-path>`;
	}

	static override styles = [
		css`
			#toolbar {
				display: flex;
				gap: var(--uui-size-6);
				align-items: flex-start;
				margin-bottom: var(--uui-size-3);
			}
			#search {
				flex: 1;
			}

			#search uui-input {
				width: 100%;
				margin-bottom: var(--uui-size-3);
			}

			#searching-indicator {
				margin-left: 7px;
				margin-top: 4px;
			}

			#media-grid {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
				grid-auto-rows: 150px;
				gap: var(--uui-size-space-5);
				padding-bottom: 5px; /** The modal is a bit jumpy due to the img card focus/hover border. This fixes the issue. */
			}

			/** TODO: Remove this fix when UUI gets upgrade to 1.3 */
			umb-imaging-thumbnail {
				pointer-events: none;
			}

			umb-icon {
				font-size: var(--uui-size-8);
			}

			img {
				background-image: url('data:image/svg+xml;charset=utf-8,<svg xmlns="http://www.w3.org/2000/svg" width="100" height="100" fill-opacity=".1"><path d="M50 0h50v50H50zM0 50h50v50H0z"/></svg>');
				background-size: 10px 10px;
				background-repeat: repeat;
			}

			#actions {
				max-width: 100%;
			}

			.not-allowed {
				cursor: not-allowed;
				opacity: 0.5;
			}

			uui-pagination {
				display: block;
				margin-top: var(--uui-size-layout-1);
			}
		`,
	];
}

export default UmbMediaPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-picker-modal': UmbMediaPickerModalElement;
	}
}
