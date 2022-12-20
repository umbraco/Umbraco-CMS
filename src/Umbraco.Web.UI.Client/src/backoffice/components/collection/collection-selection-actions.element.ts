import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import type { MediaDetails } from '@umbraco-cms/models';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import type { UmbDashboardMediaManagementElement } from 'src/backoffice/dashboards/media-management/dashboard-media-management.element';

@customElement('umb-collection-selection-actions')
export class UmbCollectionSelectionActionsElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				gap: var(--uui-size-3);
				width: 100%;
				padding: var(--uui-size-space-4) var(--uui-size-space-6);
				background-color: var(--uui-color-selected);
				color: var(--uui-color-selected-contrast);
				align-items: center;
				box-sizing: border-box;
			}
		`,
	];

	@property()
	public entityType = 'media';

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

	private _renderSelectionCount() {
		return html`<div>${this._selection.length} of ${this._mediaItems.length} selected</div>`;
	}

	render() {
		if (this._selection.length === 0) return nothing;

		return html`<uui-button label="Clear" look="secondary"></uui-button>
			${this._renderSelectionCount()}
			<umb-extension-slot
				type="collectionBulkAction"
				.filter=${(manifest: any) => manifest.meta.entityType === this.entityType}></umb-extension-slot>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-selection-actions': UmbCollectionSelectionActionsElement;
	}
}
