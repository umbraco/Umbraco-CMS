import { UmbMediaItemRepository } from '../../repository/index.js';
import { UmbMediaTreeRepository } from '../../tree/media-tree.repository.js';
import { UMB_MEDIA_ROOT_ENTITY_TYPE } from '../../entity.js';
import type { UmbDropzoneElement } from '../../dropzone/dropzone.element.js';
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
import { debounce } from '@umbraco-cms/backoffice/utils';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UMB_CONTENT_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/content';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { isUmbracoFolder } from '@umbraco-cms/backoffice/media-type';
import type { UmbMediaTreeItemModel } from '../../tree/index.js';
import { UmbMediaSearchProvider, type UmbMediaSearchItemModel } from '../../search/index.js';

import '@umbraco-cms/backoffice/imaging';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { not } from 'rxjs/internal/util/not';

const root: UmbMediaPathModel = { name: 'Media', unique: null, entityType: UMB_MEDIA_ROOT_ENTITY_TYPE };

@customElement('umb-media-picker-modal')
export class UmbMediaPickerModalElement extends UmbModalBaseElement<
	UmbMediaPickerModalData<unknown>,
	UmbMediaPickerModalValue
> {
	#mediaTreeRepository = new UmbMediaTreeRepository(this);
	#mediaItemRepository = new UmbMediaItemRepository(this);
	#mediaSearchProvider = new UmbMediaSearchProvider(this);

	#dataType?: { unique: string };

	@state()
	private _selectableFilter: (item: UmbMediaTreeItemModel) => boolean = () => true;

	@state()
	private _currentChildren: Array<UmbMediaTreeItemModel> = [];

	@state()
	private _searchResult: Array<UmbMediaSearchItemModel> = [];

	@state()
	private _searchOnlyWithinCurrentItem = false;

	@state()
	private _searchQuery = '';

	@state()
	private _currentMediaEntity: UmbMediaPathModel = root;

	@query('#dropzone')
	private _dropzone!: UmbDropzoneElement;

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

		if (this.data?.startNode) {
			const { data } = await this.#mediaItemRepository.requestItems([this.data.startNode]);
			const startNode = data?.[0];

			if (startNode) {
				this._currentMediaEntity = {
					name: startNode.name,
					unique: startNode.unique,
					entityType: startNode.entityType,
				};
			}
		}

		this.#loadChildrenOfCurrentMediaItem();
	}

	async #loadChildrenOfCurrentMediaItem() {
		const { data } = await this.#mediaTreeRepository.requestTreeItemsOf({
			parent: {
				unique: this._currentMediaEntity.unique,
				entityType: this._currentMediaEntity.entityType,
			},
			dataType: this.#dataType,
			skip: 0,
			take: 100,
		});

		this._currentChildren = data?.items ?? [];
	}

	#onOpen(item: UmbMediaTreeItemModel | UmbMediaSearchItemModel) {
		this.#clearSearch();

		this._currentMediaEntity = {
			name: item.name,
			unique: item.unique,
			entityType: UMB_MEDIA_ROOT_ENTITY_TYPE,
		};

		this.#loadChildrenOfCurrentMediaItem();
	}

	#onSelected(item: UmbMediaTreeItemModel | UmbMediaSearchItemModel) {
		const selection = this.data?.multiple ? [...this.value.selection, item.unique!] : [item.unique!];
		this.modalContext?.setValue({ selection });
	}

	#onDeselected(item: UmbMediaTreeItemModel | UmbMediaSearchItemModel) {
		const selection = this.value.selection.filter((value) => value !== item.unique);
		this.modalContext?.setValue({ selection });
	}

	#clearSearch() {
		this._searchQuery = '';
		this._searchResult = [];
	}

	async #searchMedia() {
		if (!this._searchQuery) {
			this._searchResult = [];
			return;
		}

		let searchFrom: UmbEntityModel | undefined = undefined;

		if (this._searchOnlyWithinCurrentItem) {
			searchFrom = {
				unique: this._currentMediaEntity.unique,
				entityType: this._currentMediaEntity.entityType,
			};
		}

		const query = this._searchQuery;
		const { data } = await this.#mediaSearchProvider.search({ query, searchFrom });

		if (!data) {
			// No search results.
			this._searchResult = [];
			return;
		}

		// Map urls for search results as we are going to show for all folders (as long they aren't trashed).
		this._searchResult = data.items;
	}

	#debouncedSearch = debounce(() => {
		this.#searchMedia();
	}, 500);

	#onSearch(e: UUIInputEvent) {
		this._searchQuery = (e.target.value as string).toLocaleLowerCase();
		this.#debouncedSearch();
	}

	#onPathChange(e: CustomEvent) {
		this._currentMediaEntity = (e.target as UmbMediaPickerFolderPathElement).currentMedia || {
			unique: null,
			entityType: UMB_MEDIA_ROOT_ENTITY_TYPE,
		};
		this.#loadChildrenOfCurrentMediaItem();
	}

	#allowNavigateToMedia(item: UmbMediaTreeItemModel | UmbMediaSearchItemModel): boolean {
		return isUmbracoFolder(item.mediaType.unique) || item.hasChildren;
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('defaultdialogs_selectMedia')}>
				${this.#renderBody()} ${this.#renderBreadcrumb()}
				<div slot="actions">
					<uui-button label=${this.localize.term('general_close')} @click=${this._rejectModal}></uui-button>
					<uui-button
						label=${this.localize.term('general_submit')}
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
			${!this._searchResult.length
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
					</div>`}
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
						@input=${this.#onSearch}>
						<uui-icon slot="prepend" name="icon-search"></uui-icon>
					</uui-input>

					${this._currentMediaEntity.unique
						? html`<uui-checkbox
								@change=${() => (this._searchOnlyWithinCurrentItem = !this._searchOnlyWithinCurrentItem)}
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
		const disabled = !this._selectableFilter(item);
		return html`
			<uui-card-media
				class=${ifDefined(disabled ? 'not-allowed' : undefined)}
				.name=${item.name}
				@open=${() => this.#onOpen(item)}
				@selected=${() => this.#onSelected(item)}
				@deselected=${() => this.#onDeselected(item)}
				?selected=${this.value?.selection?.find((value) => value === item.unique)}
				?selectable=${!disabled}
				?select-only=${this.#allowNavigateToMedia(item) === false}>
				<umb-imaging-thumbnail
					unique=${item.unique}
					alt=${item.name}
					icon=${item.mediaType.icon}></umb-imaging-thumbnail>
			</uui-card-media>
		`;
	}

	#renderBreadcrumb() {
		return html`<umb-media-picker-folder-path
			slot="footer-info"
			.currentMedia=${this._currentMediaEntity}
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
			#search uui-icon {
				height: 100%;
				display: flex;
				align-items: stretch;
				padding-left: var(--uui-size-3);
			}
			#media-grid {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
				grid-auto-rows: 150px;
				gap: var(--uui-size-space-5);
				padding-bottom: 5px; /** The modal is a bit jumpy due to the img card focus/hover border. This fixes the issue. */
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
		`,
	];
}

export default UmbMediaPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-picker-modal': UmbMediaPickerModalElement;
	}
}
