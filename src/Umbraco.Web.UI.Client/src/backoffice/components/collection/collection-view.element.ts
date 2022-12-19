import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-collection-view')
export class UmbCollectionViewElement extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				width: 100%;
				height: 100%;
				box-sizing: border-box;
			}

			#main {
				flex-grow: 1;
				overflow: auto;
			}
			#header,
			#footer {
				flex-basis: content;
			}

			slot {
				display: flex;
				flex-direction: column;
				width: 100%;
				height: 100%;
			}
		`,
	];

	render() {
		return html`
			<slot id="header" name="header"></slot>
			<slot id="main" name="main"></slot>
			<slot id="footer" name="footer"></slot>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-view': UmbCollectionViewElement;
	}
}
