import type {
	UmbEntityBulkActionProgressModalData,
	UmbEntityBulkActionProgressModalValue,
} from './entity-bulk-action-progress-modal.token.js';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-entity-bulk-action-progress-modal')
export class UmbEntityBulkActionProgressModalElement extends UmbModalBaseElement<
	UmbEntityBulkActionProgressModalData,
	UmbEntityBulkActionProgressModalValue
> {
	#onCancel() {
		// Rejecting resolves the modal; the running operation observes this via modal.isResolved() and stops.
		this._rejectModal();
	}

	override render() {
		return html`
			<uui-dialog-layout class="uui-text" .headline=${this.data?.headline ?? ''}>
				${this.data?.mode === 'indeterminate' ? this.#renderIndeterminate() : this.#renderDeterminate()}
			</uui-dialog-layout>
		`;
	}

	#renderIndeterminate() {
		// Leaving progress unset makes uui-loader-bar run its looped (indeterminate) animation.
		return html`<uui-loader-bar></uui-loader-bar>`;
	}

	#renderDeterminate() {
		const total = this.value?.total ?? 0;
		const completed = this.value?.completed ?? 0;
		const progress = total > 0 ? Math.round((completed / total) * 100) : 0;

		return html`
			<div id="progress">
				<uui-loader-bar .progress=${progress}></uui-loader-bar>
				<span>${completed} / ${total}</span>
			</div>
			<uui-button slot="actions" label=${this.localize.term('general_cancel')} @click=${this.#onCancel}></uui-button>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#progress {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-2);
			}
			uui-loader-bar {
				width: 100%;
			}
		`,
	];
}

export { UmbEntityBulkActionProgressModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-bulk-action-progress-modal': UmbEntityBulkActionProgressModalElement;
	}
}
