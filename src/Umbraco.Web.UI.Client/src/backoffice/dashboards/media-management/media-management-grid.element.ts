import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

export interface UmbMediaItem {
	name: string;
	type: string;
}

@customElement('umb-media-management-grid')
export class UmbMediaManagementGridElement extends LitElement {
	static styles = [UUITextStyles, css``];

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
			case 'file':
				return html`<uui-card-media name="${item.name}" file-ext="${item.type}"></uui-card-media>`;

			default:
				break;
		}
	}

	render() {
		return html`
			<div id="media-folders">
				${this.mediaFolders.map((folder) => html`<uui-card-media name="${folder.name}"></uui-card-media>`)}
			</div>
			<div id="media-files">
				${this.mediaFiles.map((file) => html` <uui-card-media name="File name" file-ext="txt"></uui-card-media>`)}
			</div>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-management-grid': UmbMediaManagementGridElement;
	}
}
