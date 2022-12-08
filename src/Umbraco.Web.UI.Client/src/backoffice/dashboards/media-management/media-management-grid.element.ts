import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement, nothing } from 'lit';
import { repeat } from 'lit-html/directives/repeat.js';
import { customElement } from 'lit/decorators.js';

export interface UmbMediaItem {
	name: string;
	type: string;
}

@customElement('umb-media-management-grid')
export class UmbMediaManagementGridElement extends LitElement {
	static styles = [
		UUITextStyles,
		css`
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

	private _mediaItems: UmbMediaItem[] = [
		{
			name: 'Image 1',
			type: 'image',
		},
		{
			name: 'Image 2',
			type: 'image',
		},
		{
			name: 'Products',
			type: 'folder',
		},
		{
			name: 'People',
			type: 'folder',
		},
	];

	private get mediaFolders() {
		return this._mediaItems.filter((item) => item.type === 'folder');
	}

	private get mediaFiles() {
		return this._mediaItems.filter((item) => item.type !== 'folder');
	}

	private _renderMediaItem(item: UmbMediaItem) {
		switch (item.type) {
			case 'folder':
				return html`<uui-card-media class="media-item" name="${item.name}"></uui-card-media>`;
			case 'file':
				return html`<uui-card-media class="media-item" name="${item.name}" file-ext="${item.type}"></uui-card-media>`;
			case 'image':
				return html`<uui-card-media class="media-item" name="${item.name}">
					<img src="https://picsum.photos/seed/picsum/200/300" alt="" />
				</uui-card-media>`;

			default:
				return nothing;
		}
	}

	render() {
		return html`
			<div id="media-folders">${repeat(this.mediaFolders, (file) => this._renderMediaItem(file))}</div>
			<div id="media-files">${repeat(this.mediaFiles, (file) => this._renderMediaItem(file))}</div>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-management-grid': UmbMediaManagementGridElement;
	}
}
