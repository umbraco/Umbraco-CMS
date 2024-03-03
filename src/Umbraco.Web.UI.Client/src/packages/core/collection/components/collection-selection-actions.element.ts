import { UMB_DEFAULT_COLLECTION_CONTEXT } from '../default/collection-default.context.js';
import type { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, nothing, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-collection-selection-actions')
export class UmbCollectionSelectionActionsElement extends UmbLitElement {
	@state()
	private _totalItems = 0;

	@state()
	private _selectionLength = 0;

	@state()
	private _elementProps = {};

	private _selection: Array<string | null> = [];

	private _collectionContext?: typeof UMB_DEFAULT_COLLECTION_CONTEXT.TYPE;

	constructor() {
		super();
		this.consumeContext(UMB_DEFAULT_COLLECTION_CONTEXT, (instance) => {
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
				this._elementProps = { selection: this._selection };
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

				<umb-extension-with-api-slot
					id="actions"
					type="entityBulkAction"
					default-element="umb-entity-bulk-action"
					.elementProps=${this._elementProps}
					.apiArgs=${[this._elementProps]}
					@action-executed=${this.#onActionExecuted}>
				</umb-extension-with-api-slot>
			</div>
		`;
	}

	static styles = [
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
