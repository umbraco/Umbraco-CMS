import type { UmbDropzoneElement } from '../../dropzone/dropzone.element.js';
import {
	UmbMediaDetailRepository,
	type UmbMediaItemModel,
	UmbMediaItemRepository,
	UmbMediaUrlRepository,
} from '../../repository/index.js';
import { UmbMediaTreeRepository } from '../../tree/media-tree.repository.js';
import type { UmbMediaCardItemModel } from './types.js';
import type { UmbMediaPickerFolderPathElement } from './components/media-picker-folder-path.element.js';
import type { UmbMediaPickerModalData, UmbMediaPickerModalValue } from './media-picker-modal.token.js';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { css, html, customElement, state, repeat, query, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { isUmbracoFolder } from '@umbraco-cms/backoffice/media-type';
import { mime } from '@umbraco-cms/backoffice/external/mime';

@customElement('umb-media-picker-modal')
export class UmbMediaPickerModalElement extends UmbModalBaseElement<UmbMediaPickerModalData, UmbMediaPickerModalValue> {
	#mediaTreeRepository = new UmbMediaTreeRepository(this); // used to get file structure
	#mediaDetailRepository = new UmbMediaDetailRepository(this); // used to create folders
	#mediaUrlRepository = new UmbMediaUrlRepository(this); // used to get urls
	#mediaItemRepository = new UmbMediaItemRepository(this); // used to search

	#mediaItemsCurrentFolder: Array<UmbMediaCardItemModel> = [];

	@state()
	private _mediaFilteredList: Array<UmbMediaCardItemModel> = [];

	@state()
	private _searchOnlyThisFolder = false;

	@state()
	private _searchQuery = '';

	@state()
	private _currentPath: string | null = null;

	@state()
	private _selectableNonImages = true;

	@query('#dropzone')
	private _dropzone!: UmbDropzoneElement;

	connectedCallback(): void {
		super.connectedCallback();
		this._selectableNonImages = this.data?.selectableNonImages ?? true;
		this._currentPath = this.data?.startNode ?? null;
		this.#loadMediaFolder();
	}

	async #loadMediaFolder() {
		const { data } = await this.#mediaTreeRepository.requestTreeItemsOf({
			parentUnique: this._currentPath,
			skip: 0,
			take: 100,
		});

		this.#mediaItemsCurrentFolder = await this.#mapMediaUrls(data?.items ?? []);
		this.#filterMediaItems();
	}

	async #mapMediaUrls(items: Array<UmbMediaItemModel>): Promise<Array<UmbMediaCardItemModel>> {
		if (!items.length) return [];

		const { data } = await this.#mediaUrlRepository.requestItems(items.map((item) => item.unique));

		return items.map((item): UmbMediaCardItemModel => {
			const url = data?.find((media) => media.unique === item.unique)?.url;
			const extension = url?.split('.').pop();
			const isImage = url ? !!mime.getType(url)?.startsWith('image/') : false;

			return { name: item.name, unique: item.unique, isImage, url, extension };
		});
	}

	#onFolderOpen(item: UmbMediaCardItemModel) {
		if (!isUmbracoFolder(item.unique)) return;
		this._currentPath = item.unique!;
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
		this._mediaFilteredList = await this.#mapMediaUrls(
			data.filter((found) => {
				found.isTrashed ? false : false;
			}),
		);
	}

	#onSearch(e: UUIInputEvent) {
		this._searchQuery = (e.target.value as string).toLocaleLowerCase();
		this.#filterMediaItems();
	}

	#onPathChange(e: CustomEvent) {
		this._currentPath = (e.target as UmbMediaPickerFolderPathElement).currentPath;
		this.#loadMediaFolder();
	}

	render() {
		return html`
			<umb-body-layout headline=${this.localize.term('defaultdialogs_selectMedia')}>
				${this.#renderBody()}
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
		return html`${this.#renderToolbar()}${this.#renderPath()}
		<umb-dropzone id="dropzone" @change=${() => this.#loadMediaFolder()} .parentUnique=${this._currentPath}></umb-dropzone>
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

	#renderPath() {
		return html`<umb-media-picker-folder-path
			.currentPath=${this._currentPath}
			@change=${this.#onPathChange}></umb-media-picker-folder-path>`;
	}

	#renderToolbar() {
		return html`<div id="toolbar">
			<div id="search">
				<uui-input
					label=${this.localize.term('general_search')}
					placeholder=${this.localize.term('placeholders_search')}
					@change=${this.#onSearch}>
					<uui-icon slot="prepend" name="icon-search"></uui-icon>
				</uui-input>
				<uui-checkbox
					@change=${() => (this._searchOnlyThisFolder = !this._searchOnlyThisFolder)}
					label=${this.localize.term('general_excludeFromSubFolders')}></uui-checkbox>
			</div>
			<uui-button
				label=${this.localize.term('general_upload')}
				look="primary"
				@click=${() => this._dropzone?.browse()}></uui-button>
		</div>`;
	}

	#renderCard(item: UmbMediaCardItemModel) {
		return html`
			<uui-card-media
				.name=${item.name ?? 'Unnamed Media'}
				@open=${() => this.#onFolderOpen(item)}
				@selected=${() => this.#onSelected(item)}
				@deselected=${() => this.#onDeselected(item)}
				?selected=${this.value?.selection?.find((value) => value === item.unique)}
				?selectable=${item.isImage || this._selectableNonImages}
				file-ext=${ifDefined(item.extension)}>
				${item.isImage && item.url ? html`<img src=${item.url} alt=${ifDefined(item.name)} />` : ''}
			</uui-card-media>
		`;
	}

	static styles = [
		css`
			#toolbar {
				display: flex;
				gap: var(--uui-size-6);
				align-items: flex-start;
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
				grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
				grid-template-rows: repeat(auto-fill, 200px);
				gap: var(--uui-size-space-5);
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
