import { css, html, LitElement, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';

/**
 * @element umb-body-layout
 * @description Layout element to arrange elements in a body layout. A general layout for most views.
 * @slot icon - Slot for icon
 * @slot name - Slot for name
 * @slot footer - Slot for workspace footer
 * @slot actions - Slot for workspace footer actions
 * @slot default - slot for main content
 * @export
 * @class UmbBodyLayout
 * @extends {UmbLitElement}
 */
@customElement('umb-body-layout')
export class UmbBodyLayoutElement extends LitElement {
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
				height: var(--umb-header-layout-height);
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
		`,
	];

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
	private _actionsMenuSlotHasChildren = false;

	@state()
	private _footerSlotHasChildren = false;

	@state()
	private _actionsSlotHasChildren = false;

	#hasNodes = (e: Event) => {
		return (e.target as HTMLSlotElement).assignedNodes({ flatten: true }).length > 0;
	};

	render() {
		return html`
			<div
				id="header"
				style="display:${this.headline ||
				this._headerSlotHasChildren ||
				this._tabsSlotHasChildren ||
				this._actionsMenuSlotHasChildren
					? ''
					: 'none'}">
				${this.headline ? html`<h3 id="headline">${this.headline}</h3>` : nothing}

				<slot
					name="header"
					@slotchange=${(e: Event) => {
						this._headerSlotHasChildren = this.#hasNodes(e);
					}}></slot>
				<slot
					id="tabs"
					name="tabs"
					@slotchange=${(e: Event) => {
						this._tabsSlotHasChildren = this.#hasNodes(e);
					}}></slot>
				<slot
					id="action-menu"
					name="action-menu"
					@slotchange=${(e: Event) => {
						this._actionsMenuSlotHasChildren = this.#hasNodes(e);
					}}></slot>
			</div>
			<uui-scroll-container id="main">
				<slot></slot>
			</uui-scroll-container>
			<umb-footer-layout style="display:${this._footerSlotHasChildren || this._actionsSlotHasChildren ? '' : 'none'}">
				<slot
					name="footer"
					@slotchange=${(e: Event) => {
						this._footerSlotHasChildren = this.#hasNodes(e);
					}}></slot>
				<slot
					name="actions"
					slot="actions"
					@slotchange=${(e: Event) => {
						this._actionsSlotHasChildren = this.#hasNodes(e);
					}}></slot>
			</umb-footer-layout>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-body-layout': UmbBodyLayoutElement;
	}
}
