import { UMB_DOCUMENT_TREE_ALIAS } from '../../../tree/manifests.js';
import type {
	UmbDuplicateDocumentModalData,
	UmbDuplicateDocumentModalValue,
} from './duplicate-document-modal.token.js';
import { html, customElement, nothing, css, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

import type { UUIBooleanInputEvent } from '@umbraco-cms/backoffice/external/uui';
import type { UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbTreeElement } from '@umbraco-cms/backoffice/tree';

const elementName = 'umb-document-duplicate-to-modal';
@customElement(elementName)
export class UmbDocumentDuplicateToModalElement extends UmbModalBaseElement<
	UmbDuplicateDocumentModalData,
	UmbDuplicateDocumentModalValue
> {
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

	#onRelateToOriginalChange(event: UUIBooleanInputEvent) {
		const target = event.target;
		this.updateValue({ relateToOriginal: target.checked });
	}

	#onIncludeDescendantsChange(event: UUIBooleanInputEvent) {
		const target = event.target;
		this.updateValue({ includeDescendants: target.checked });
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
			const result = await this.data.onBeforeSubmit(this._destinationUnique, {
				relateToOriginal: this.value?.relateToOriginal ?? false,
				includeDescendants: this.value?.includeDescendants ?? false,
			});
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
				<uui-box id="tree-box" headline=${this.localize.term('moveOrCopy_copyTo')}>
					<umb-tree
						alias=${UMB_DOCUMENT_TREE_ALIAS}
						.props=${{
							expandTreeRoot: true,
							hideTreeItemActions: true,
						}}
						@selection-change=${this.#onTreeSelectionChange}></umb-tree>
				</uui-box>
				<uui-box headline=${this.localize.term('general_options')}>
					<umb-property-layout label=${this.localize.term('moveOrCopy_relateToOriginal')} orientation="vertical"
						><div slot="editor">
							<uui-toggle
								@change=${this.#onRelateToOriginalChange}
								.checked=${this.value?.relateToOriginal}></uui-toggle>
						</div>
					</umb-property-layout>

					<umb-property-layout label=${this.localize.term('moveOrCopy_includeDescendants')} orientation="vertical"
						><div slot="editor">
							<uui-toggle
								@change=${this.#onIncludeDescendantsChange}
								.checked=${this.value?.includeDescendants}></uui-toggle>
						</div>
					</umb-property-layout>
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
				@click="${this._rejectModal}"
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
			#tree-box {
				margin-bottom: var(--uui-size-layout-1);
			}

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

export { UmbDocumentDuplicateToModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbDocumentDuplicateToModalElement;
	}
}
