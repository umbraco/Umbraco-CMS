import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-history-list')
export class UmbHistoryListElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				--avatar-size: calc(2em + 4px);
			}

			::slotted(*) {
				position: relative;
			}

			::slotted(*:not(:last-child)) {
				margin-bottom: calc(2 * var(--uui-size-space-3));
			}

			::slotted(*:not(:last-child))::before {
				content: '';
				border: 1px solid var(--uui-color-border);
				position: absolute;
				display: block;
				height: calc(1.5 * var(--avatar-size));
				top: var(--avatar-size);
				left: calc(-1px + var(--avatar-size) / 2);
			}
		`,
	];

	render() {
		return html`<div>
			<slot></slot>
		</div>`;
	}
}

export default UmbHistoryListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-history-list': UmbHistoryListElement;
	}
}
