import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-section-main')
export class UmbSectionMainElement extends LitElement {
	

	render() {
		return html`
			<main>
				<slot></slot>
			</main>
		`;
	}
	
	static styles = [
		UUITextStyles,
		css`
			:host {
				flex: 1 1 auto;
				height: 100%;
			}

			main,
			slot {
				display: flex;
				flex-direction: column;
				height: 100%;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-main': UmbSectionMainElement;
	}
}
