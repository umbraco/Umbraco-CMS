import { UMB_COLLECTION_CONTEXT } from '../default/index.js';
import { css, customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import type { ManifestEntityBulkAction, MetaEntityBulkAction } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';

/**
 * Generates API arguments for the given manifest.
 * @param {string | null | undefined} entityType - The type of the entity.
 * @param {ManifestEntityBulkAction<MetaEntityBulkAction>} manifest - The manifest object from the extension.
 * @returns {Array<unknown>} An array with the meta object from the manifest.
 */
function apiArgsMethod(
	entityType: string | null | undefined,
	manifest: ManifestEntityBulkAction<MetaEntityBulkAction>,
): Array<unknown> {
	return [{ entityType, meta: manifest.meta }] as Array<unknown>;
}

@customElement('umb-collection-selection-actions')
export class UmbCollectionSelectionActionsElement extends UmbLitElement {
	@state()
	private _entityType?: string | null;

	@state()
	private _totalItems = 0;

	@state()
	private _selectionLength = 0;

	@state()
	private _apiProps = {};

	private _selection: Array<string | null> = [];

	private _collectionContext?: typeof UMB_COLLECTION_CONTEXT.TYPE;

	constructor() {
		super();
		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this._collectionContext = instance;
			this._observeCollectionContext();
		});

		this.consumeContext(UMB_ENTITY_CONTEXT, (entityContext) => {
			this._entityType = entityContext.getEntityType();
		});
	}

	private _handleKeyDown(event: KeyboardEvent) {
		if (event.key === 'Enter') {
			this._handleClearSelection();
		}
	}

	private _handleClearSelection() {
		this._collectionContext?.selection.clearSelection();
	}

	private _observeCollectionContext() {
		if (!this._collectionContext) return;

		this.observe(
			this._collectionContext.totalItems,
			(value) => {
				this._totalItems = value;
			},
			'umbTotalItemsObserver',
		);

		this.observe(
			this._collectionContext.selection.selection,
			(selection) => {
				this._selectionLength = selection.length;
				this._selection = selection;
				this._apiProps = { selection: this._selection };
			},
			'umbSelectionObserver',
		);
	}

	private _renderSelectionCount() {
		return html`<div>${this._selectionLength} of ${this._totalItems} selected</div>`;
	}

	#onActionExecuted(event: UmbActionExecutedEvent) {
		event.stopPropagation();
		this._collectionContext?.selection.clearSelection();
	}

	override render() {
		if (this._selectionLength === 0) return nothing;

		return html`
			<div id="container">
				<div id="selection-info">
					<uui-button
						@click=${this._handleClearSelection}
						@keydown=${this._handleKeyDown}
						label=${this.localize.term('buttons_clearSelection')}
						look="secondary"></uui-button>
					${this._renderSelectionCount()}
				</div>

				<umb-extension-with-api-slot
					id="actions"
					type="entityBulkAction"
					default-element="umb-entity-bulk-action"
					.apiProps=${this._apiProps}
					.apiArgs=${(manifest: ManifestEntityBulkAction) => apiArgsMethod(this._entityType, manifest)}
					@action-executed=${this.#onActionExecuted}>
				</umb-extension-with-api-slot>
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				width: 100%;
				margin-right: calc(
					-1 * var(--uui-size-space-6)
				); // compensate for the padding on the container. TODO: make a better solution
			}

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
