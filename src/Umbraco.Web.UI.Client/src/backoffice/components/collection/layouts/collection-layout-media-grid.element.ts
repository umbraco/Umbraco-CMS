import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import type { MediaDetails } from '@umbraco-cms/models';
import { UmbMediaStore } from '@umbraco-cms/stores/media/media.store';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';

@customElement('umb-collection-layout-media-grid')
export class UmbCollectionLayoutMediaGridElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
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
				padding: 0 var(--uui-size-space-6);
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

	private _mediaStore?: UmbMediaStore;
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
		this.consumeContext('umbMediaStore', (store: UmbMediaStore) => {
			this._mediaStore = store;
			this._observeMediaItems();
		});
	}

	private _observeMediaItems() {
		if (!this._mediaStore) return;

		this.observe<Array<MediaDetails>>(this._mediaStore?.getTreeRoot(), (items) => {
			this._mediaItems = items;
			console.log('media store', this._mediaStore);
		});
	}

	private _handleOpenItem(key: string) {
		//TODO: Fix when we have dynamic routing
		history.pushState(null, '', 'section/media/media/' + key);
	}

	private _renderMediaItem(item: MediaDetails) {
		const name = item.name || '';
		return html`<uui-card-media
			@open=${() => this._handleOpenItem(item.key)}
			class="media-item"
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
			<div id="media-files">${repeat(this._mediaItems, (file) => this._renderMediaItem(file))}</div>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-layout-media-grid': UmbCollectionLayoutMediaGridElement;
	}
}
