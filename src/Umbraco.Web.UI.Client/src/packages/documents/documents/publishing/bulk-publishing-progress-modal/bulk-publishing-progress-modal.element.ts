import type {
	UmbDocumentBulkPublishingProgressModalData,
	UmbDocumentBulkPublishingProgressModalValue,
} from './bulk-publishing-progress-modal.token.js';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-document-bulk-publishing-progress-modal')
export class UmbDocumentBulkPublishingProgressModalElement extends UmbModalBaseElement<
	UmbDocumentBulkPublishingProgressModalData,
	UmbDocumentBulkPublishingProgressModalValue
> {
	#onCancel() {
		// Closing the dialog stops the operation; the initiator observes this rejection.
		this._rejectModal();
	}

	override render() {
		const total = this.value?.total ?? 0;
		const completed = this.value?.completed ?? 0;
		const progress = total > 0 ? Math.round((completed / total) * 100) : 0;

		return html`
			<uui-dialog-layout class="uui-text" .headline=${this.data?.headline ?? ''}>
				<div id="progress">
					<uui-loader-circle .progress=${progress}></uui-loader-circle>
					<span>${completed} / ${total}</span>
				</div>
				<uui-button slot="actions" label=${this.localize.term('general_cancel')} @click=${this.#onCancel}></uui-button>
			</uui-dialog-layout>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#progress {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-4);
			}
		`,
	];
}

export { UmbDocumentBulkPublishingProgressModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-bulk-publishing-progress-modal': UmbDocumentBulkPublishingProgressModalElement;
	}
}
