import type { UmbMediaDetailModel } from '../../types.js';
import { UmbMediaDetailRepository } from '../../repository/index.js';
import { UmbMediaTreeRepository } from '../../tree/media-tree.repository.js';
import type { UmbMediaTreeItemModel } from '../../tree/types.js';
import type { UmbDropzoneMediaElement } from '../../components/index.js';
import type { UmbMediaPickerModalData, UmbMediaPickerModalValue } from './media-picker-modal.token.js';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { css, html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import type { UUIInputElement, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbMediaTypeFileType } from '@umbraco-cms/backoffice/media-type';

// Folder media type unique from backend
const FOLDER = 'f38bd2d7-65d0-48e6-95dc-87ce06ec2d3d';

interface MediaPath {
	name: string;
	unique: string | null;
}

@customElement('umb-media-picker-modal')
export class UmbMediaPickerModalElement extends UmbModalBaseElement<UmbMediaPickerModalData, UmbMediaPickerModalValue> {
	#mediaTreeRepository = new UmbMediaTreeRepository(this);
	#mediaDetailRepository = new UmbMediaDetailRepository(this);

	@state()
	private _items: Array<UmbMediaTreeItemModel> = [];

	@state()
	private _selectableFolders = false;

	@state()
	private _paths: Array<MediaPath> = [{ name: 'Media', unique: null }];

	@state()
	private _currentPath: string | null = null;

	@state()
	private _typingNewFolder = false;

	connectedCallback(): void {
		super.connectedCallback();
		this._selectableFolders = this.data?.selectableFolders ?? false;
		this._currentPath = this.data?.startNode ?? null;
		this.#loadPath();
	}

	async #loadPath() {
		if (this._currentPath) {
			const { data } = await this.#mediaTreeRepository.requestTreeItemAncestors({
				descendantUnique: this._currentPath,
			});
			const paths = data?.map((item) => ({ name: item.name, unique: item.unique })) ?? [];
			this._paths = [...this._paths, ...paths];
		}

		this.#loadMediaFolder();
	}

	async #loadMediaFolder() {
		if (this._currentPath) {
			const { data } = await this.#mediaTreeRepository.requestTreeItemsOf({
				parentUnique: this._currentPath,
				skip: 0,
				take: 100,
			});
			this._items = data?.items ?? [];
		} else {
			const { data } = await this.#mediaTreeRepository.requestRootTreeItems({ skip: 0, take: 100 });
			this._items = data?.items ?? [];
		}
	}

	#goToFolder(unique: string | null) {
		this._paths = [...this._paths].slice(0, this._paths.findIndex((path) => path.unique === unique) + 1);
		this._currentPath = unique;
		this.#loadMediaFolder();
	}

	#onFolderOpen(item: UmbMediaTreeItemModel) {
		if (item.mediaType.unique !== FOLDER) return;
		this._paths = [...this._paths, { name: item.name, unique: item.unique }];
		this._currentPath = item.unique;
		this.#loadMediaFolder();
	}

	#focusFolderInput() {
		this._typingNewFolder = true;
		requestAnimationFrame(() => {
			const element = this.getHostElement().shadowRoot!.querySelector('#new-folder') as UUIInputElement;
			element.focus();
			element.select();
		});
	}

	async #addFolder(e: UUIInputEvent) {
		const name = e.target.value as string;

		if (name) {
			const unique = UmbId.new();
			const parentUnique = this._paths[this._paths.length - 1].unique;
			this._paths = [...this._paths, { name, unique }];
			this._currentPath = unique;

			const preset: Partial<UmbMediaDetailModel> = {
				unique,
				mediaType: {
					unique: UmbMediaTypeFileType.FOLDER,
					collection: null,
				},
				variants: [
					{
						culture: null,
						segment: null,
						name: name,
						createDate: null,
						updateDate: null,
					},
				],
			};
			const { data } = await this.#mediaDetailRepository.createScaffold(preset);
			if (data) {
				await this.#mediaDetailRepository.create(data, parentUnique);
			}
		}
		this._typingNewFolder = false;
		this.#loadMediaFolder();
	}

	#onSelected(item: UmbMediaTreeItemModel) {
		const selection = this.data?.multiple ? [...this.value.selection, item.unique] : [item.unique];
		this.modalContext?.setValue({ selection });
		if (this.data?.submitOnSelection) {
			this._submitModal();
		}
	}

	#onDeselected(item: UmbMediaTreeItemModel) {
		const selection = this.value.selection.filter((value) => value !== item.unique);
		this.modalContext?.setValue({ selection });
	}

	#onUploadClick() {
		const dropzoneHandler = this.shadowRoot?.querySelector('#dropzone') as UmbDropzoneMediaElement;
		dropzoneHandler?.getDropzoneElement()?.browse();
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
		return html`${this.#renderToolbar()}
		<umb-dropzone-media id="dropzone" @change=${() => this.#loadMediaFolder()} .parentUnique=${this._currentPath}></umb-dropzone-media>
				${
					!this._items.length
						? html`<div class="container"><p>${this.localize.term('content_listViewNoItems')}</p></div>`
						: html`<div id="media-grid">
								${repeat(
									this._items,
									(item) => item.unique,
									(item) => this.#renderCard(item),
								)}
							</div>`
				}
			</div>`;
	}

	#renderToolbar() {
		return html`<div id="toolbar">
				<div id="search">
					<uui-input
						label=${this.localize.term('general_search')}
						placeholder=${this.localize.term('placeholders_search') + '(TODO)'}>
						<uui-icon slot="prepend" name="icon-search"></uui-icon>
					</uui-input>
					<uui-checkbox label=${this.localize.term('general_excludeFromSubFolders') + '(TODO)'} disabled></uui-checkbox>
				</div>
				<uui-button
					label=${this.localize.term('general_upload')}
					look="primary"
					@click=${this.#onUploadClick}></uui-button>
			</div>
			${this.#renderPath()}`;
	}

	#renderPath() {
		return html`<div id="path">
			${repeat(
				this._paths,
				(path) => path.unique,
				(path) =>
					html`<uui-button
							compact
							.label=${path.name}
							?disabled=${this._currentPath == path.unique}
							@click=${() => this.#goToFolder(path.unique)}></uui-button
						>/`,
			)}${this._typingNewFolder
				? html`<uui-input
						id="new-folder"
						label="enter a name"
						value="new folder name"
						@blur=${this.#addFolder}
						auto-width></uui-input>`
				: html`<uui-button label="add folder" compact @click=${this.#focusFolderInput}>+</uui-button>`}
		</div>`;
	}

	#renderCard(item: UmbMediaTreeItemModel) {
		return html`
			<uui-card-media
				.name=${item.name ?? 'Unnamed Media'}
				@open=${() => this.#onFolderOpen(item)}
				@selected=${() => this.#onSelected(item)}
				@deselected=${() => this.#onDeselected(item)}
				?selected=${this.value?.selection?.find((value) => value === item.unique)}
				?selectable=${item.mediaType.unique !== FOLDER || this._selectableFolders}
				?select-only=${item.mediaType.unique !== FOLDER}
				file-ext=${item.mediaType.unique !== FOLDER ? item.mediaType.icon : ''}>
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

			#path {
				display: flex;
				align-items: center;
				margin-bottom: var(--uui-size-3);
			}
			#path uui-button {
				font-weight: bold;
			}
			#path uui-input {
				height: 100%;
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
