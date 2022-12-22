import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import type { UmbCollectionContext } from '../collection.context';
import type { MediaDetails } from '@umbraco-cms/models';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';

@customElement('umb-collection-view-media-grid')
export class UmbCollectionViewsMediaGridElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
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
	private _mediaItems: Array<MediaDetails> = [];

	@state()
	private _selection: Array<string> = [];

	private _collectionContext?: UmbCollectionContext<MediaDetails>;

	constructor() {
		super();
		document.addEventListener('dragenter', (e) => {
			this.toggleAttribute('dragging', true);
		});
		document.addEventListener('dragleave', (e) => {
			this.toggleAttribute('dragging', false);
		});
		document.addEventListener('drop', (e) => {
			e.preventDefault();
			this.toggleAttribute('dragging', false);
		});
		this.consumeContext('umbCollectionContext', (instance) => {
			console.log("umbCollectionContext", instance)
			this._collectionContext = instance;
			this._observeCollectionContext();
		});
	}

	private _observeCollectionContext() {
		if (!this._collectionContext) return;

		this.observe<Array<MediaDetails>>(this._collectionContext.data, (mediaItems) => {
			this._mediaItems = mediaItems.sort((a, b) => (a.hasChildren === b.hasChildren ? 0 : a ? -1 : 1));
		});

		this.observe<Array<string>>(this._collectionContext.selection, (selection) => {
			this._selection = selection;
		});
	}

	private _handleOpenItem(mediaItem: MediaDetails) {
		//TODO: Fix when we have dynamic routing
		history.pushState(null, '', 'section/media/media/' + mediaItem.key);
	}

	private _handleSelect(mediaItem: MediaDetails) {
		this._collectionContext?.select(mediaItem.key);
	}

	private _handleDeselect(mediaItem: MediaDetails) {
		this._collectionContext?.deselect(mediaItem.key);
	}

	private _isSelected(mediaItem: MediaDetails) {
		return this._selection.includes(mediaItem.key);
	}

	private _renderMediaItem(item: MediaDetails) {
		const name = item.name || '';
		//TODO: fix the file extension when media items have a file extension.
		return html`<uui-card-media
			selectable
			?select-only=${this._selection.length > 0}
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
				${repeat(
					this._mediaItems,
					(file, index) => file.key + index,
					(file) => this._renderMediaItem(file)
				)}
			</div>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-view-media-grid': UmbCollectionViewsMediaGridElement;
	}
}
