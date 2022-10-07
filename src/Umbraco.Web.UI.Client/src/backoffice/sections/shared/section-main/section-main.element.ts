import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-section-main')
export class UmbSectionMainElement extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				flex: 1 1 auto;
				height: 100%;
			}
			slot {
				display: flex;
				flex-direction: column;
				height: 100%;
			}
		`,
	];

	render() {
		return html`<slot></slot>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-main': UmbSectionMainElement;
	}
}
