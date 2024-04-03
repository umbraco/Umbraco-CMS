import type { UmbRollbackModalData, UmbRollbackModalValue } from './rollback-modal.token.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

import '../shared/document-variant-language-picker.element.js';

@customElement('umb-rollback-modal')
export class UmbRollbackModalElement extends UmbModalBaseElement<UmbRollbackModalData, UmbRollbackModalValue> {
	#onRollback() {
		console.log('Rollback');
		return;
		this.modalContext?.submit();
	}

	#onCancel() {
		this.modalContext?.reject();
	}

	#renderRollbackItem() {
		return html`
			<div>
				<uui-card class="rollback-item">
					<div>
						<p class="rollback-item-date">April 2, 2024 6:05 PM</p>
						<p>Jesper</p>
						<p>Current published version</p>
					</div>
					<uui-button look="secondary" @click=${this.#onRollback}>Prevent cleanup</uui-button>
				</uui-card>
			</div>
		`;
	}

	render() {
		return html`
			<umb-body-layout headline="Rollback">
				<div id="main">
					<uui-box headline="Versions" id="box-left"> ${this.#renderRollbackItem()} </uui-box>
					<uui-box headline="DATE HERE" id="box-right"></uui-box>
				</div>
			</umb-body-layout>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			.rollback-item {
				display: flex;
				justify-content: space-between;
				align-items: center;
				padding: var(--uui-size-space-5);
			}
			.rollback-item p {
				margin: 0;
				opacity: 0.5;
			}
			.rollback-item-date {
				opacity: 1;
			}
			#main {
				display: flex;
				gap: var(--uui-size-space-4);
				width: 100%;
			}

			#box-left {
				max-width: 500px;
				flex: 1;
			}

			#box-right {
				flex: 1;
			}
		`,
	];
}

export default UmbRollbackModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-rollback-modal': UmbRollbackModalElement;
	}
}
