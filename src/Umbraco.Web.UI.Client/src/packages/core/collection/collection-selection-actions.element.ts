import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, nothing, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { map } from '@umbraco-cms/backoffice/external/rxjs';
import { UMB_COLLECTION_CONTEXT_TOKEN, UmbCollectionContext } from '@umbraco-cms/backoffice/collection';
import { ManifestEntityBulkAction, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbExecutedEvent } from '@umbraco-cms/backoffice/events';

@customElement('umb-collection-selection-actions')
export class UmbCollectionSelectionActionsElement extends UmbLitElement {
	#entityType?: string;

	@state()
	private _nodesLength = 0;

	@state()
	private _selectionLength = 0;

	@state()
	private _entityBulkActions: Array<ManifestEntityBulkAction> = [];

	private _collectionContext?: UmbCollectionContext<any, any>;
	private _selection: Array<string> = [];

	constructor() {
		super();
		this.consumeContext(UMB_COLLECTION_CONTEXT_TOKEN, (instance) => {
			this._collectionContext = instance;
			this._observeCollectionContext();

			if (instance.getEntityType()) {
				this.#entityType = instance.getEntityType() ?? undefined;
				this.#observeEntityBulkActions();
			}
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
		this.observe(this._collectionContext.items, (mediaItems) => {
			this._nodesLength = mediaItems.length;
		}, 'observeItem');

		this.observe(this._collectionContext.selection, (selection) => {
			this._selectionLength = selection.length;
			this._selection = selection;
		}, 'observeSelection');
	}

	private _renderSelectionCount() {
		return html`<div>${this._selectionLength} of ${this._nodesLength} selected</div>`;
	}

	// TODO: find a solution to use extension slot
	#observeEntityBulkActions() {
		this.observe(
			umbExtensionsRegistry.extensionsOfType('entityBulkAction').pipe(
				map((extensions) => {
					return extensions.filter((extension) => extension.conditions.entityType === this.#entityType);
				})
			),
			(bulkActions) => {
				this._entityBulkActions = bulkActions;
			}
			, 'observeEntityBulkActions'
		);
	}

	#onActionExecuted(event: UmbExecutedEvent) {
		event.stopPropagation();
		this._collectionContext?.clearSelection();
	}

	render() {
		if (this._selectionLength === 0) return nothing;

		return html`
			<div id="container">
				<div id="selection-info">
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
				</div>
			</div>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			#container {
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
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-selection-actions': UmbCollectionSelectionActionsElement;
	}
}
