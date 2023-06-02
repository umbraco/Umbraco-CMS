import {
	css,
	html,
	LitElement,
	nothing,
	customElement,
	property,
	state,
	query,
} from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';

/**
 * @element umb-body-layout
 * @description Layout element to arrange elements in a body layout. A general layout for most views.
 * @slot icon - Slot for icon
 * @slot name - Slot for name
 * @slot footer - Slot for footer element
 * @slot footer-info - Slot for info in the footer
 * @slot actions - Slot for actions in the footer
 * @slot default - slot for main content
 * @export
 * @class UmbBodyLayout
 * @extends {UmbLitElement}
 */
@customElement('umb-body-layout')
export class UmbBodyLayoutElement extends LitElement {
	/**
	 * Renders a headline in the header.
	 * @public
	 * @type {string}
	 * @attr {string} clear-header - renders the header without background and borders.
	 * @default ''
	 */

	@query('#main') scrollContainer?: HTMLElement;

	@property()
	public headline = '';

	@property({ type: Boolean, reflect: true, attribute: 'scroll-shadow' })
	public scrollShadow = false;

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

	connectedCallback(): void {
		super.connectedCallback();
		if (this.scrollShadow) {
			requestAnimationFrame(() => {
				this.scrollContainer?.addEventListener('scroll', this.#onScroll);
			});
		}
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this.scrollContainer?.removeEventListener('scroll', this.#onScroll);
	}

	#onScroll = () => {
		if (!this.scrollContainer) return;
		this.toggleAttribute('scrolling', this.scrollContainer.scrollTop > 0);
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

			<!-- This div should be changed for the uui-scroll-container when it gets updated -->
			<div id="main">
				<slot></slot>
			</div>

			<slot name="footer"></slot>
			<umb-footer-layout style="display:${this._footerSlotHasChildren || this._actionsSlotHasChildren ? '' : 'none'}">
				<slot
					name="footer-info"
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
				z-index: 1;
			}

			:host([scroll-shadow]) #header {
				transition: box-shadow 150ms ease-in-out;
				box-shadow: 0 -1px 0px 0px rgba(0, 0, 0, 0.8);
			}

			:host([clear-header]) #header {
				background-color: transparent;
				border-color: transparent;
			}

			:host([scroll-shadow][scrolling]) #header {
				/* This should be using the uui-shadows but for now they are too drastic for this use case */
				box-shadow: 0 1px 15px 0 rgba(0, 0, 0, 0.3);
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
				overflow-y: auto;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-body-layout': UmbBodyLayoutElement;
	}
}
