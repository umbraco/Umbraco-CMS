import { css, html, LitElement, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';

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
				gap: var(--uui-size-space-2);
				margin: 0 var(--uui-size-layout-1);
				margin-left: auto;
			}
		`,
	];

	private hasNodes = (e: Event) => {
		return (e.target as HTMLSlotElement).assignedNodes({ flatten: true }).length > 0;
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

	@state()
	private _headerSlotHasChildren = false;
	@state()
	private _tabsSlotHasChildren = false;
	@state()
	private _footerSlotHasChildren = false;
	@state()
	private _actionsSlotHasChildren = false;

	render() {
		return html`
			<div
				id="header"
				style="display:${this.headline || this._headerSlotHasChildren || this._tabsSlotHasChildren ? '' : 'none'}">
				${this.headline ? html`<h3 id="headline">${this.headline}</h3>` : nothing}

				<slot
					name="header"
					@slotchange=${(e: Event) => {
						this._headerSlotHasChildren = this.hasNodes(e);
					}}></slot>
				<slot
					id="tabs"
					name="tabs"
					@slotchange=${(e: Event) => {
						this._tabsSlotHasChildren = this.hasNodes(e);
					}}></slot>
			</div>
			<uui-scroll-container id="main">
				<slot></slot>
			</uui-scroll-container>
			<div id="footer" style="display:${this._footerSlotHasChildren || this._actionsSlotHasChildren ? '' : 'none'}">
				<slot
					name="footer"
					@slotchange=${(e: Event) => {
						this._footerSlotHasChildren = this.hasNodes(e);
					}}></slot>
				<slot
					id="actions"
					name="actions"
					style="display:${this._actionsSlotHasChildren ? '' : 'none'}"
					@slotchange=${(e: Event) => {
						this._actionsSlotHasChildren = this.hasNodes(e);
					}}></slot>
			</div>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-body-layout': UmbBodyLayout;
	}
}
