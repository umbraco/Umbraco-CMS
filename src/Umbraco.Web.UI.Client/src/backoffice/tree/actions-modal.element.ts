import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';

@customElement('umb-actions-modal')
export class UmbActionsModal extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			#title {
				display: flex;
				flex-direction: column;
				justify-content: center;
				padding: 0 var(--uui-size-4);
				height: 70px;
				box-sizing: border-box;
				border-bottom: 1px solid var(--uui-color-divider-standalone);
			}
			#title > * {
				margin: 0;
			}

			.action {
				padding: var(--uui-size-2) var(--uui-size-4);
				border-bottom: 1px solid var(--uui-color-divider);
				cursor: pointer;
			}
			.action:hover {
				background-color: var(--uui-color-surface-alt);
			}
		`,
	];

	@property()
	name = '';

	render() {
		return html`
			<div id="title">
				<h3>${this.name}</h3>
			</div>
			<div id="action-list">
				<div class="action">action 1</div>
				<div class="action">action 2</div>
				<div class="action">action 3</div>
			</div>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-actions-modal': UmbActionsModal;
	}
}
