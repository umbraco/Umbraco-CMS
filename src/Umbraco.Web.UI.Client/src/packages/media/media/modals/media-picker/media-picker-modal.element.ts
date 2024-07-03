import { UmbMediaItemRepository, UmbMediaUrlRepository } from '../../repository/index.js';
import { UmbMediaTreeRepository } from '../../tree/media-tree.repository.js';
import { UMB_MEDIA_ROOT_ENTITY_TYPE } from '../../entity.js';
import type { UmbDropzoneElement } from '../../dropzone/dropzone.element.js';
import type { UmbMediaItemModel } from '../../repository/index.js';
import type { UmbMediaCardItemModel, UmbMediaPathModel } from './types.js';
import type { UmbMediaPickerFolderPathElement } from './components/media-picker-folder-path.element.js';
import type { UmbMediaPickerModalData, UmbMediaPickerModalValue } from './media-picker-modal.token.js';
import { css, html, customElement, state, repeat, ifDefined, query } from '@umbraco-cms/backoffice/external/lit';
import { debounce } from '@umbraco-cms/backoffice/utils';
import { ImageCropModeModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbImagingRepository } from '@umbraco-cms/backoffice/imaging';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UMB_CONTENT_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/content';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

const root: UmbMediaPathModel = { name: 'Media', unique: null, entityType: UMB_MEDIA_ROOT_ENTITY_TYPE };

@customElement('umb-media-picker-modal')
export class UmbMediaPickerModalElement extends UmbModalBaseElement<
	UmbMediaPickerModalData<unknown>,
	UmbMediaPickerModalValue
> {
	#mediaTreeRepository = new UmbMediaTreeRepository(this); // used to get file structure
	#mediaItemRepository = new UmbMediaItemRepository(this); // used to search
	#imagingRepository = new UmbImagingRepository(this); // used to get image renditions

	#dataType?: { unique: string };

	@state()
	private _filter: (item: UmbMediaCardItemModel) => boolean = () => true;

	@state()
	private _selectableFilter: (item: UmbMediaCardItemModel) => boolean = () => true;

	#mediaItemsCurrentFolder: Array<UmbMediaCardItemModel> = [];

	@state()
	private _mediaFilteredList: Array<UmbMediaCardItemModel> = [];

	@state()
	private _searchOnlyThisFolder = false;

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

		if (this.data?.filter) this._filter = this.data?.filter;
		if (this.data?.pickableFilter) this._selectableFilter = this.data?.pickableFilter;

		if (this.data?.startNode) {
			const { data } = await this.#mediaItemRepository.requestItems([this.data.startNode]);

			if (data?.length) {
				this._currentMediaEntity = { name: data[0].name, unique: data[0].unique, entityType: data[0].entityType };
			}
		}

		this.#loadMediaFolder();
	}

	async #loadMediaFolder() {
		const { data } = await this.#mediaTreeRepository.requestTreeItemsOf({
			parent: {
				unique: this._currentMediaEntity.unique,
				entityType: this._currentMediaEntity.entityType,
			},
			dataType: this.#dataType,
			skip: 0,
			take: 100,
		});

		this.#mediaItemsCurrentFolder = await this.#mapMediaUrls(data?.items ?? []);
		this.#filterMediaItems();
	}

	async #mapMediaUrls(items: Array<UmbMediaItemModel>): Promise<Array<UmbMediaCardItemModel>> {
		if (!items.length) return [];

		const { data } = await this.#imagingRepository.requestResizedItems(
			items.map((item) => item.unique),
			{ height: 400, width: 400, mode: ImageCropModeModel.MIN },
		);

		return items
			.map((item): UmbMediaCardItemModel => {
				const src = data?.find((media) => media.unique === item.unique)?.url;
				return { ...item, src };
			})
			.filter((item) => this._filter(item));
	}

	#onOpen(item: UmbMediaCardItemModel) {
		this._currentMediaEntity = {
			name: item.name,
			unique: item.unique,
			entityType: UMB_MEDIA_ROOT_ENTITY_TYPE,
		};
		this.#loadMediaFolder();
	}

	#onSelected(item: UmbMediaCardItemModel) {
		const selection = this.data?.multiple ? [...this.value.selection, item.unique!] : [item.unique!];
		this.modalContext?.setValue({ selection });
	}

	#onDeselected(item: UmbMediaCardItemModel) {
		const selection = this.value.selection.filter((value) => value !== item.unique);
		this.modalContext?.setValue({ selection });
	}

	async #filterMediaItems() {
		if (!this._searchQuery) {
			// No search query, show all media items in current folder.
			this._mediaFilteredList = this.#mediaItemsCurrentFolder;
			return;
		}

		const query = this._searchQuery;
		const { data } = await this.#mediaItemRepository.search({ query, skip: 0, take: 100 });

		if (!data) {
			// No search results.
			this._mediaFilteredList = [];
			return;
		}

		if (this._searchOnlyThisFolder) {
			// Don't have to map urls here, because we already have everything loaded within this folder.
			this._mediaFilteredList = this.#mediaItemsCurrentFolder.filter((media) =>
				data.find((item) => item.unique === media.unique),
			);
			return;
		}

		// Map urls for search results as we are going to show for all folders (as long they aren't trashed).
		this._mediaFilteredList = await this.#mapMediaUrls(data.filter((found) => found.isTrashed === false));
	}

	#debouncedSearch = debounce(() => {
		this.#filterMediaItems();
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
		this.#loadMediaFolder();
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('defaultdialogs_selectMedia')}>
				${this.#renderBody()} ${this.#renderPath()}
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
		<umb-dropzone id="dropzone" @change=${() => this.#loadMediaFolder()} .parentUnique=${this._currentMediaEntity.unique}></umb-dropzone>
				${
					!this._mediaFilteredList.length
						? html`<div class="container"><p>${this.localize.term('content_listViewNoItems')}</p></div>`
						: html`<div id="media-grid">
								${repeat(
									this._mediaFilteredList,
									(item) => item.unique,
									(item) => this.#renderCard(item),
								)}
							</div>`
				}
			</div>`;
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
					<uui-checkbox
						@change=${() => (this._searchOnlyThisFolder = !this._searchOnlyThisFolder)}
						label=${this.localize.term('general_excludeFromSubFolders')}></uui-checkbox>
				</div>
				<uui-button
					@click=${() => this._dropzone.browse()}
					label=${this.localize.term('general_upload')}
					look="primary"></uui-button>
			</div>
		`;
	}

	#renderCard(item: UmbMediaCardItemModel) {
		const disabled = !this._selectableFilter(item);
		return html`
			<uui-card-media
				class=${ifDefined(disabled ? 'not-allowed' : undefined)}
				.name=${item.name}
				@open=${() => this.#onOpen(item)}
				@selected=${() => this.#onSelected(item)}
				@deselected=${() => this.#onDeselected(item)}
				?selected=${this.value?.selection?.find((value) => value === item.unique)}
				?selectable=${!disabled}>
				${item.src
					? html`<img src=${item.src} alt=${ifDefined(item.name)} />`
					: html`<umb-icon .name=${item.mediaType.icon}></umb-icon>`}
			</uui-card-media>
		`;
	}

	#renderPath() {
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
