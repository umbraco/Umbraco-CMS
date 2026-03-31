import type { UmbNotificationHandler } from '../../notification-handler.js';
import type { UmbPeekErrorArgs } from '../../types.js';
import { css, customElement, html, ifDefined, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import {
	UMB_ERROR_VIEWER_MODAL,
	UMB_MODAL_MANAGER_CONTEXT,
	type UmbErrorViewerModalData,
} from '@umbraco-cms/backoffice/modal';

const DETAIL_MAX_LENGTH = 250;

@customElement('umb-peek-error-notification')
export class UmbPeekErrorNotificationElement extends UmbLitElement {
	@property({ attribute: false })
	public data?: UmbPeekErrorArgs;

	public notificationHandler!: UmbNotificationHandler;

	async #openErrorViewer(data: UmbErrorViewerModalData) {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		if (!modalManager) {
			throw new Error('Modal manager not found.');
		}

		modalManager.open(this, UMB_ERROR_VIEWER_MODAL, { data });
		this.notificationHandler.close();
	}

	get #message() {
		const detail = this.data?.detail;
		if (detail && detail.length <= DETAIL_MAX_LENGTH) {
			return html`${this.data?.message}
				<p class="detail">${detail}</p>`;
		}
		return this.data?.message;
	}

	get #validationErrors() {
		return this.data?.errors ?? this.data?.details;
	}

	get #actions() {
		const hasLongDetail = !!this.data?.detail && this.data.detail.length > DETAIL_MAX_LENGTH;
		if (!this.#validationErrors && !hasLongDetail) return nothing;

		return html`
			${this.#validationErrors
				? html`<uui-button
						slot="actions"
						look="primary"
						color="danger"
						label=${this.localize.term('defaultdialogs_seeErrorAction')}
						@click=${() => this.#openErrorViewer(this.#validationErrors!)}></uui-button>`
				: nothing}
			${hasLongDetail
				? html`<uui-button
						slot="actions"
						look="primary"
						color="danger"
						label=${this.localize.term('defaultdialogs_exceptionDetail')}
						@click=${() => {
							// Cast: the error viewer modal handles strings at runtime, but UmbModalToken constrains data to object types.
							this.#openErrorViewer(this.data!.detail! as unknown as UmbErrorViewerModalData);
						}}></uui-button>`
				: nothing}
		`;
	}

	protected override render() {
		if (!this.data) return nothing;
		return html`<uui-toast-notification-layout headline=${ifDefined(this.data.headline)}>
			${this.#message} ${this.#actions}
		</uui-toast-notification-layout>`;
	}

	static override readonly styles = [
		css`
			.detail {
				overflow: hidden;
				text-overflow: ellipsis;
				display: -webkit-box;
				-webkit-line-clamp: 3;
				-webkit-box-orient: vertical;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-peek-error-notification': UmbPeekErrorNotificationElement;
	}
}
