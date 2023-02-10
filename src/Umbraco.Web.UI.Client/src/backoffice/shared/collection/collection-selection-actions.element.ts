import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { map } from 'rxjs';
import { UmbExecutedEvent } from '../../../core/events';
import { UmbCollectionContext, UMB_COLLECTION_CONTEXT_TOKEN } from './collection.context';
import type { ManifestEntityBulkAction, MediaDetails } from '@umbraco-cms/models';
import { UmbLitElement } from '@umbraco-cms/element';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-api';

import '../entity-bulk-actions/entity-bulk-action.element';

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
				justify-content: space-between;
			}
			#selection-info,
			#actions {
				display: flex;
				align-items: center;
				box-sizing: border-box;
				gap: var(--uui-size-3);
			}
		`,
	];

	@property()
	public entityType: string | null = null;

	@state()
	private _nodesLength = 0;

	@state()
	private _selectionLength = 0;

	@state()
	private _entityBulkActions: Array<ManifestEntityBulkAction> = [];

	private _collectionContext?: UmbCollectionContext<MediaDetails>;
	private _selection: Array<string> = [];

	constructor() {
		super();
		this.consumeContext(UMB_COLLECTION_CONTEXT_TOKEN, (instance) => {
			this._collectionContext = instance;
			this.entityType = instance.getEntityType();
			this._observeCollectionContext();
			this.#observeEntityBulkActions();
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
			this._selection = selection;
		});
	}

	private _renderSelectionCount() {
		return html`<div>${this._selectionLength} of ${this._nodesLength} selected</div>`;
	}

	// TODO: find a solution to use extension slot
	#observeEntityBulkActions() {
		this.observe(
			umbExtensionsRegistry.extensionsOfType('entityBulkAction').pipe(
				map((extensions) => {
					return extensions.filter((extension) => extension.meta.entityType === this.entityType);
				})
			),
			(bulkActions) => {
				this._entityBulkActions = bulkActions;
			}
		);
	}

	#onActionExecuted(event: UmbExecutedEvent) {
		event.stopPropagation();
		this._collectionContext?.clearSelection();
	}

	render() {
		if (this._selectionLength === 0) return nothing;

		return html`<div id="selection-info">
				<uui-button
					@click=${this._handleClearSelection}
					@keydown=${this._handleKeyDown}
					label="Clear"
					look="secondary"></uui-button>
				${this._renderSelectionCount()}
			</div>

			<div id="actions">
				${this._entityBulkActions?.map(
					(manifest) =>
						html`<umb-entity-bulk-action
							@executed=${this.#onActionExecuted}
							.selection=${this._selection}
							.manifest=${manifest}></umb-entity-bulk-action>`
				)}
			</div>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-selection-actions': UmbCollectionSelectionActionsElement;
	}
}
