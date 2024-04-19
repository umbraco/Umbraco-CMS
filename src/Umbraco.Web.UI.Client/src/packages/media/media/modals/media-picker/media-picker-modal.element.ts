import { UmbMediaCollectionRepository } from '../../collection/repository/media-collection.repository.js';
import type { UmbMediaCollectionFilterModel, UmbMediaCollectionItemModel } from '../../collection/types.js';
import type { UmbMediaPickerModalData, UmbMediaPickerModalValue } from './media-picker-modal.token.js';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { css, html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import type { UUIInputElement, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbId } from '@umbraco-cms/backoffice/id';

@customElement('umb-media-picker-modal')
export class UmbMediaPickerModalElement extends UmbModalBaseElement<UmbMediaPickerModalData, UmbMediaPickerModalValue> {
	#collectionRepository = new UmbMediaCollectionRepository(this);

	@state()
	private _items: Array<UmbMediaCollectionItemModel> = [];

	@state()
	private _selection: Array<string> = [];

	@state()
	private _currentPath = 'media-root';

	@state()
	private _typingNewFolder = false;

	@state()
	private _paths: Array<{ name: string; unique: string }> = [{ name: 'Media', unique: 'media-root' }];

	connectedCallback(): void {
		super.connectedCallback();
		this._selection = this.data?.selection ?? [];
		this.#getCollection();
	}

	async #getCollection() {
		const params: UmbMediaCollectionFilterModel = {
			// TODO whatever this is for...
			userDefinedProperties: [{ alias: 'type', header: '', isSystem: true }],
		};

		const { data } = await this.#collectionRepository.requestCollection(params);
		this._items = data?.items ?? [];

		console.log(data?.items);
	}

	#focusFolderInput() {
		this._typingNewFolder = true;
		requestAnimationFrame(() => {
			const element = this.getHostElement().shadowRoot!.querySelector('#new-folder') as UUIInputElement;
			element.focus();
			element.select();
		});
	}

	#addFolder(e: UUIInputEvent) {
		const name = e.target.value as string;
		if (name) {
			const unique = UmbId.new();
			this._paths = [...this._paths, { name, unique }];
			this._currentPath = unique;
		}
		this._typingNewFolder = false;
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
						placeholder=${this.localize.term('placeholders_search')}>
						<uui-icon slot="prepend" name="icon-search"></uui-icon>
					</uui-input>
					<uui-checkbox label=${this.localize.term('general_excludeFromSubFolders') + '(TODO)'} disabled></uui-checkbox>
				</div>
				<uui-button label=${this.localize.term('general_upload')} look="primary"></uui-button>
			</div>
			${this.#renderPath()}`;
	}

	#goToFolder(unique: string) {
		this._paths = [...this._paths].slice(0, this._paths.findIndex((path) => path.unique === unique) + 1);
		this._currentPath = unique;
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

	#renderCard(item: UmbMediaCollectionItemModel) {
		return html`
			<uui-card-media
				.name=${item.name ?? 'Unnamed Media'}
				selectable
				?select-only=${this._selection && this._selection.length > 0}
				file-ext=${item.values.find((value) => value.alias === 'type')?.value ?? item.icon}>
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
