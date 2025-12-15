import type { UmbTreeElement } from '../../../tree.element.js';
import type { UmbDuplicateToModalData, UmbDuplicateToModalValue } from './duplicate-to-modal.token.js';
import { css, customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/event';

const elementName = 'umb-duplicate-to-modal';
@customElement(elementName)
export class UmbDuplicateToModalElement extends UmbModalBaseElement<UmbDuplicateToModalData, UmbDuplicateToModalValue> {
	@state()
	private _destinationUnique?: string | null;

	@state()
	private _selectionError?: string;

	@state()
	private _submitError?: string;

	@state()
	private _isSubmitting = false;

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
			<umb-body-layout headline=${this.localize.term('actions_copy')}>
				<uui-box headline=${this.localize.term('moveOrCopy_copyTo', this.data.name ?? '')}>
					<umb-tree
						alias=${this.data.treeAlias}
						.props=${{
							hideTreeItemActions: true,
							foldersOnly: this.data?.foldersOnly,
							expandTreeRoot: true,
							selectableFilter: this.data?.pickableFilter,
						}}
						@selection-change=${this.#onTreeSelectionChange}></umb-tree>
				</uui-box>
				${this.#renderActions()}
			</umb-body-layout>
		`;
	}

	#renderActions() {
		const error = this._submitError ?? this._selectionError;
		const canSubmit = this._destinationUnique !== undefined && !this._selectionError && !this._isSubmitting;

		return html`
			${error
				? html`<div id="error" slot="actions">
						<uui-icon name="icon-alert"></uui-icon>
						<span>${error}</span>
					</div>`
				: nothing}
			<uui-button
				slot="actions"
				label=${this.localize.term('general_cancel')}
				@click=${this._rejectModal}
				?disabled=${this._isSubmitting}></uui-button>
			<uui-button
				slot="actions"
				color="positive"
				look="primary"
				label=${this.localize.term('actions_copy')}
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
				padding: var(--uui-size-space-3) 0;
				padding-inline-start: var(--uui-size-space-3);
			}
		`,
	];
}

export { UmbDuplicateToModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbDuplicateToModalElement;
	}
}
