import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { UmbCollectionContext, UMB_COLLECTION_CONTEXT_TOKEN } from '../../../shared/collection/collection.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
// TODO: this should be a lib import

@customElement('umb-media-grid-collection-view')
export class UmbMediaGridCollectionViewElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				box-sizing: border-box;
				position: relative;
				height: 100%;
				width: 100%;
				padding: var(--uui-size-space-3) var(--uui-size-space-6);
			}
			:host([dragging]) #dropzone {
				opacity: 1;
				pointer-events: all;
			}
			[dropzone] {
				opacity: 0;
			}
			#dropzone {
				opacity: 0;
				pointer-events: none;
				display: block;
				position: absolute;
				inset: 0px;
				z-index: 100;
				backdrop-filter: opacity(1); /* Removes the built in blur effect */
				border-radius: var(--uui-border-radius);
				overflow: clip;
				border: 1px solid var(--uui-color-focus);
			}
			#dropzone:after {
				content: '';
				display: block;
				position: absolute;
				inset: 0;
				border-radius: var(--uui-border-radius);
				background-color: var(--uui-color-focus);
				opacity: 0.2;
			}
			#media-folders {
				margin-bottom: var(--uui-size-space-5);
			}
			#media-folders,
			#media-files {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
				grid-template-rows: repeat(auto-fill, 200px);
				gap: var(--uui-size-space-5);
			}
			.media-item img {
				object-fit: contain;
			}
		`,
	];

	@state()
	private _mediaItems?: Array<EntityTreeItemResponseModel>;

	@state()
	private _selection: Array<string> = [];

	private _collectionContext?: UmbCollectionContext<EntityTreeItemResponseModel>;

	constructor() {
		super();
		document.addEventListener('dragenter', this._handleDragEnter.bind(this));
		document.addEventListener('dragleave', this._handleDragLeave.bind(this));
		document.addEventListener('drop', this._handleDrop.bind(this));
		this.consumeContext(UMB_COLLECTION_CONTEXT_TOKEN, (instance) => {
			this._collectionContext = instance;
			this._observeCollectionContext();
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		document.removeEventListener('dragenter', this._handleDragEnter.bind(this));
		document.removeEventListener('dragleave', this._handleDragLeave.bind(this));
		document.removeEventListener('drop', this._handleDrop.bind(this));
	}

	private _handleDragEnter() {
		this.toggleAttribute('dragging', true);
	}

	private _handleDragLeave() {
		this.toggleAttribute('dragging', false);
	}

	private _handleDrop(e: DragEvent) {
		e.preventDefault();
		this.toggleAttribute('dragging', false);
	}

	private _observeCollectionContext() {
		if (!this._collectionContext) return;

		this.observe(this._collectionContext.data, (mediaItems) => {
			this._mediaItems = [...mediaItems].sort((a, b) => (a.hasChildren === b.hasChildren ? 0 : a ? -1 : 1));
		});

		this.observe(this._collectionContext.selection, (selection) => {
			this._selection = selection;
		});
	}

	private _handleOpenItem(mediaItem: EntityTreeItemResponseModel) {
		//TODO: Fix when we have dynamic routing
		history.pushState(null, '', 'section/media/media/edit/' + mediaItem.key);
	}

	private _handleSelect(mediaItem: EntityTreeItemResponseModel) {
		if (mediaItem.key) {
			this._collectionContext?.select(mediaItem.key);
		}
	}

	private _handleDeselect(mediaItem: EntityTreeItemResponseModel) {
		if (mediaItem.key) {
			this._collectionContext?.deselect(mediaItem.key);
		}
	}

	private _isSelected(mediaItem: EntityTreeItemResponseModel) {
		if (mediaItem.key) {
			return this._selection.includes(mediaItem.key);
		}
		return false;
	}

	private _renderMediaItem(item: EntityTreeItemResponseModel) {
		const name = item.name || '';
		//TODO: fix the file extension when media items have a file extension.
		return html`<uui-card-media
			selectable
			?select-only=${this._selection && this._selection.length > 0}
			?selected=${this._isSelected(item)}
			@open=${() => this._handleOpenItem(item)}
			@selected=${() => this._handleSelect(item)}
			@unselected=${() => this._handleDeselect(item)}
			class="media-item"
			.fileExt=${item.hasChildren ? '' : 'image'}
			name=${name}></uui-card-media>`;
	}

	render() {
		return html`
			<uui-file-dropzone
				id="dropzone"
				multiple
				@file-change=${(e: any) => console.log(e)}
				label="Drop files here"
				accept=""></uui-file-dropzone>
			<div id="media-files">
				${this._mediaItems
					? repeat(
							this._mediaItems,
							(file, index) => (file.key || '') + index,
							(file) => this._renderMediaItem(file)
					  )
					: ''}
			</div>
		`;
	}
}

export default UmbMediaGridCollectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-grid-collection-view': UmbMediaGridCollectionViewElement;
	}
}
