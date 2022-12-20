import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import type { MediaDetails } from '@umbraco-cms/models';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import type { UmbDashboardMediaManagementElement } from 'src/backoffice/dashboards/media-management/dashboard-media-management.element';

@customElement('umb-collection-layout-media-table')
export class UmbCollectionLayoutMediaTableElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles = [UUITextStyles, css``];

	@state()
	private _mediaItems: Array<MediaDetails> = [];

	@state()
	private _selection: Array<string> = [];

	private _mediaContext?: UmbDashboardMediaManagementElement;

	constructor() {
		super();
		this.consumeAllContexts(['umbMediaContext'], (instance) => {
			this._mediaContext = instance['umbMediaContext'];
			this._observeMediaContext();
		});
	}

	private _observeMediaContext() {
		if (!this._mediaContext) return;

		this.observe<Array<MediaDetails>>(this._mediaContext.mediaItems, (mediaItems) => {
			this._mediaItems = mediaItems;
		});

		this.observe<Array<string>>(this._mediaContext.selection, (selection) => {
			this._selection = selection;
		});
	}

	private _isSelected(mediaItem: MediaDetails) {
		return this._selection.includes(mediaItem.key);
	}

	render() {
		return html`<h1>umb-collection-layout-media-table</h1>
			<div>
				<h3>Selected Media Items:</h3>
				<ul>
					${this._selection.map((key) => {
						const mediaItem = this._mediaItems.find((item) => item.key === key);
						return html`<li>${mediaItem?.name}</li>`;
					})}
				</ul>
			</div> `;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-layout-media-table': UmbCollectionLayoutMediaTableElement;
	}
}
