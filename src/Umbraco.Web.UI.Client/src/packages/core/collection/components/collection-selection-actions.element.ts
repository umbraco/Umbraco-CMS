import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, nothing, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UMB_COLLECTION_CONTEXT, UmbCollectionContext } from '@umbraco-cms/backoffice/collection';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-collection-selection-actions')
export class UmbCollectionSelectionActionsElement extends UmbLitElement {
	@state()
	private _nodesLength = 0;

	@state()
	private _selectionLength = 0;

	@state()
	private _extensionProps = {};

	private _selection: Array<string | null> = [];

	private _collectionContext?: UmbCollectionContext<any, any>;

	constructor() {
		super();
		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
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
		this.observe(
			this._collectionContext.items,
			(mediaItems) => {
				this._nodesLength = mediaItems.length;
			},
			'umbItemsLengthObserver',
		);

		this.observe(
			this._collectionContext.selection,
			(selection) => {
				this._selectionLength = selection.length;
				this._selection = selection;
				this._extensionProps = { selection: this._selection };
			},
			'umbSelectionObserver',
		);
	}

	private _renderSelectionCount() {
		return html`<div>${this._selectionLength} of ${this._nodesLength} selected</div>`;
	}

	#onActionExecuted(event: UmbActionExecutedEvent) {
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

				<umb-extension-slot
					id="actions"
					type="entityBulkAction"
					default-element="umb-entity-bulk-action"
					.props=${this._extensionProps}
					@action-executed=${this.#onActionExecuted}>
				</umb-extension-slot>
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
