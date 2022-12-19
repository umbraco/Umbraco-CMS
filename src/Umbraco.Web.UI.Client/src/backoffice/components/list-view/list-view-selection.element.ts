import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';

@customElement('umb-list-view-selection')
export class UmbListViewSelectionElement extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				gap: var(--uui-size-3);
				width: 100%;
				padding: var(--uui-size-space-4) var(--uui-size-space-6);
				background-color: var(--uui-color-selected);
				color: var(--uui-color-selected-contrast);
				align-items: center;
				box-sizing: border-box;
			}
		`,
	];

	@state()
	private _selection: Array<string> = [];

	private _renderSelectionCount() {
		return html`<div>${this._selection.length} of ${4} selected</div>`;
	}

	render() {
		return html`<uui-button label="Clear" look="secondary"></uui-button>
			${this._renderSelectionCount()}
			<uui-button style="margin-left: auto" label="Set group" look="secondary"></uui-button>
			<uui-button label="Enable" look="secondary"></uui-button>
			<uui-button label="Unlock" disabled look="secondary"></uui-button>
			<uui-button label="Disable" look="secondary"></uui-button> `;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-list-view-selection': UmbListViewSelectionElement;
	}
}
