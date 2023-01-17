import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbCollectionContext, UMB_COLLECTION_CONTEXT_ALIAS } from './collection.context';
import type { MediaDetails } from '@umbraco-cms/models';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-collection-selection-actions')
export class UmbCollectionSelectionActionsElement extends UmbLitElement {
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
	private _nodesLength = 0;

	@state()
	private _selectionLength = 0;

	private _collectionContext?: UmbCollectionContext<MediaDetails>;

	constructor() {
		super();
		this.consumeContext(UMB_COLLECTION_CONTEXT_ALIAS, (instance) => {
			this._collectionContext = instance;
			this._observeCollectionContext();
		});
	}

	private _handleKeyDown(event: KeyboardEvent) {
		if (event.key === 'Enter') {
			this._handleClearSelection();
		}
	}

	private _handleClearSelection() {
		this._collectionContext?.clearSelection();
	}

	private _observeCollectionContext() {
		if (!this._collectionContext) return;

		// TODO: Make sure it only updates on length change.
		this.observe(this._collectionContext.data, (mediaItems) => {
			this._nodesLength = mediaItems.length;
		});

		this.observe(this._collectionContext.selection, (selection) => {
			this._selectionLength = selection.length;
		});
	}

	private _renderSelectionCount() {
		return html`<div>${this._selectionLength} of ${this._nodesLength} selected</div>`;
	}

	render() {
		if (this._selectionLength === 0) return nothing;

		return html`<uui-button
				@click=${this._handleClearSelection}
				@keydown=${this._handleKeyDown}
				label="Clear"
				look="secondary"></uui-button>
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
