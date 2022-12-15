import { css, html, LitElement, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';

@customElement('umb-body-layout')
export class UmbBodyLayout extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				background-color: var(--uui-color-background);
				width: 100%;
				height: 100%;
				flex-direction: column;
			}

			#header {
				display: flex;
				align-items: center;
				justify-content: space-between;
				width: 100%;
				min-height: 60px;

				background-color: var(--uui-color-surface);
				border-bottom: 1px solid var(--uui-color-border);
				box-sizing: border-box;
			}

			#headline {
				display: block;
				margin: 0 var(--uui-size-layout-1);
			}

			#tabs {
				margin-left: auto;
			}

			#main {
				display: flex;
				flex: 1;
				flex-direction: column;
			}

			#footer {
				display: flex;
				align-items: center;
				justify-content: space-between;
				width: 100%;
				height: 54px; /* TODO: missing var(--uui-size-18);*/
				border-top: 1px solid var(--uui-color-border);
				background-color: var(--uui-color-surface);
				box-sizing: border-box;
			}

			#actions {
				display: flex;
				gap: 6px;
				margin: 0 var(--uui-size-layout-1);
				margin-left: auto;
			}
		`,
	];

	connectedCallback() {
		super.connectedCallback();
		this.shadowRoot?.removeEventListener('slotchange', this._slotChanged);
		this.shadowRoot?.addEventListener('slotchange', this._slotChanged);
	}

	disconnectedCallback() {
		super.disconnectedCallback();
		this.shadowRoot?.removeEventListener('slotchange', this._slotChanged);
	}

	private _slotChanged = (e: Event) => {
		(e.target as any).style.display =
			(e.target as HTMLSlotElement).assignedNodes({ flatten: true }).length > 0 ? '' : 'none';
	};

	/**
	 * Renders a headline in the header.
	 * @public
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	@property()
	public headline = '';

	render() {
		return html`
			<div id="header">
				${this.headline ? html`<h3 id="headline">${this.headline}</h3>` : nothing}

				<slot name="header"></slot>
				<slot id="tabs" name="tabs"></slot>
			</div>
			<uui-scroll-container id="main">
				<slot></slot>
			</uui-scroll-container>
			<div id="footer">
				<slot name="footer"></slot>
				<slot id="actions" name="actions"></slot>
			</div>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-body-layout': UmbBodyLayout;
	}
}
