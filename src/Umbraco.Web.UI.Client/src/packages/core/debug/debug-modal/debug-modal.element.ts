import type { UmbContextDebuggerModalData } from './debug-modal.token.js';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-context-debugger-modal')
export default class UmbContextDebuggerModalElement extends UmbModalBaseElement<UmbContextDebuggerModalData> {
	#close() {
		this.modalContext?.reject();
	}

	override render() {
		return html`
			<umb-body-layout headline="Debug: Contexts">
				<div id="main">${this.data?.content}</div>
				<div slot="actions">
					<uui-button @click=${this.#close} label=${this.localize.term('general_close')}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			summary {
				cursor: pointer;
			}

			details > details {
				margin-left: 1rem;
			}

			ul {
				margin-top: 0;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-context-debugger-modal': UmbContextDebuggerModalElement;
	}
}
