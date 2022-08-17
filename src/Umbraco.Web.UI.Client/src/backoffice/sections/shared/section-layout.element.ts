import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-section-layout')
export class UmbSectionLayout extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				width: 100%;
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
		'umb-section-layout': UmbSectionLayout;
	}
}
