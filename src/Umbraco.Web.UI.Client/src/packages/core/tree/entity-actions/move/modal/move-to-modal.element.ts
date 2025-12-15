import type { UmbTreeElement } from '../../../tree.element.js';
import type { UmbTreeItemModel } from '../../../types.js';
import type { UmbMoveToModalData, UmbMoveToModalValue } from './move-to-modal.token.js';
import { css, customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/event';

const elementName = 'umb-move-to-modal';
@customElement(elementName)
export class UmbMoveToModalElement extends UmbModalBaseElement<UmbMoveToModalData, UmbMoveToModalValue> {
	@state()
	private _destinationUnique?: string | null;

	@state()
	private _selectionError?: string;

	@state()
	private _submitError?: string;

	@state()
	private _isSubmitting = false;

	@state()
	private _disabledItems: Set<string | null> = new Set();

	/**
	 * Combined selectable filter that checks both the original pickableFilter
	 * and the dynamically disabled items
	 * @returns A filter function or undefined if no filtering needed
	 */
	#getSelectableFilter(): ((item: UmbTreeItemModel) => boolean) | undefined {
		const originalFilter = this.data?.pickableFilter;
		const disabledItems = this._disabledItems;

		// If no original filter and no disabled items, return undefined (all selectable)
		if (!originalFilter && disabledItems.size === 0) {
			return undefined;
		}

		return (item: UmbTreeItemModel) => {
			// Check if item is in disabled set
			if (disabledItems.has(item.unique)) {
				return false;
			}
			// Check original filter if provided
			if (originalFilter && !originalFilter(item)) {
				return false;
			}
			return true;
		};
	}

	async #onTreeSelectionChange(event: UmbSelectionChangeEvent) {
		const target = event.target as UmbTreeElement;
		const selection = target.getSelection();
		this._destinationUnique = selection[0];
		this._selectionError = undefined;
		this._submitError = undefined;

		if (this._destinationUnique === undefined) return;

		this.updateValue({ destination: { unique: this._destinationUnique } });

		// Lazy validation via onSelection callback
		if (this.data?.onSelection) {
			const result = await this.data.onSelection(this._destinationUnique);
			if (!result.valid) {
				this._selectionError = result.error;
				// Add to disabled items so it can't be selected again
				if (this._destinationUnique !== null) {
					this._disabledItems = new Set([...this._disabledItems, this._destinationUnique]);
				}
			}
		}
	}

	async #onSubmit() {
		if (this._destinationUnique === undefined) return;

		// If no onBeforeSubmit provided, just close the modal (legacy behavior)
		if (!this.data?.onBeforeSubmit) {
			this._submitModal();
			return;
		}

		this._isSubmitting = true;
		this._submitError = undefined;

		try {
			const result = await this.data.onBeforeSubmit(this._destinationUnique);
			if (result.success) {
				this._submitModal();
			} else {
				// Stay open and show error
				this._submitError = result.error?.message ?? this.localize.term('general_unknownError');
			}
		} finally {
			this._isSubmitting = false;
		}
	}

	override render() {
		if (!this.data) return nothing;

		return html`
			<umb-body-layout headline=${this.localize.term('actions_move')}>
				<uui-box>
					<umb-tree
						alias=${this.data.treeAlias}
						.props=${{
							hideTreeItemActions: true,
							foldersOnly: this.data?.foldersOnly,
							expandTreeRoot: true,
							selectableFilter: this.#getSelectableFilter(),
						}}
						@selection-change=${this.#onTreeSelectionChange}></umb-tree>
				</uui-box>
				${this.#renderError()} ${this.#renderActions()}
			</umb-body-layout>
		`;
	}

	#renderError() {
		const error = this._submitError ?? this._selectionError;
		if (!error) return nothing;

		return html`
			<div id="error">
				<uui-icon name="icon-alert"></uui-icon>
				<span>${error}</span>
			</div>
		`;
	}

	#renderActions() {
		const canSubmit = this._destinationUnique !== undefined && !this._selectionError && !this._isSubmitting;

		return html`
			<uui-button
				slot="actions"
				label=${this.localize.term('general_cancel')}
				@click=${this._rejectModal}
				?disabled=${this._isSubmitting}></uui-button>
			<uui-button
				slot="actions"
				color="positive"
				look="primary"
				label=${this.localize.term('actions_move')}
				@click=${this.#onSubmit}
				?disabled=${!canSubmit}
				.state=${this._isSubmitting ? 'waiting' : undefined}></uui-button>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#error {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-2);
				color: var(--uui-color-danger);
				padding: var(--uui-size-space-4);
				margin-top: var(--uui-size-space-4);
			}
		`,
	];
}

export { UmbMoveToModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbMoveToModalElement;
	}
}
