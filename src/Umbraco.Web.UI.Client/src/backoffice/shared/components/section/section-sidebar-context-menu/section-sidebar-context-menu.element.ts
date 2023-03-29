import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import {
	UmbSectionSidebarContext,
	UMB_SECTION_SIDEBAR_CONTEXT_TOKEN,
} from '../section-sidebar/section-sidebar.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-section-sidebar-context-menu')
export class UmbSectionSidebarContextMenuElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
				z-index: 1;
			}
			#backdrop {
				content: '';
				position: absolute;
				inset: 0px;
				background-color: black;
				opacity: 0.5;
				width: 100vw;
				height: 100vh;
				z-index: -1;
			}
			#relative-wrapper {
				background-color: var(--uui-color-surface);
				position: relative;
				display: flex;
				flex-direction: column;
				width: 100%;
				height: 100%;
			}
			#action-modal {
				position: absolute;
				left: 300px;
				height: 100%;
				z-index: 1;
				top: 0;
				width: 300px;
				border: none;
				border-left: 1px solid var(--uui-color-border);
				border-right: 1px solid var(--uui-color-border);
				background-color: var(--uui-color-surface);
			}
		`,
	];

	#sectionSidebarContext?: UmbSectionSidebarContext;

	@state()
	private _isOpen = false;

	@state()
	private _entityType?: string;

	@state()
	private _unique?: string;

	@state()
	private _headline?: string;

	constructor() {
		super();

		this.consumeContext(UMB_SECTION_SIDEBAR_CONTEXT_TOKEN, (instance) => {
			this.#sectionSidebarContext = instance;

			this.observe(this.#sectionSidebarContext.contextMenuIsOpen, (value) => (this._isOpen = value));
			this.observe(this.#sectionSidebarContext.unique, (value) => (this._unique = value));
			this.observe(this.#sectionSidebarContext.entityType, (value) => (this._entityType = value));
			this.observe(this.#sectionSidebarContext.headline, (value) => (this._headline = value));
		});
	}

	#closeContextMenu() {
		this.#sectionSidebarContext?.closeContextMenu();
	}

	#onActionExecuted(event: CustomEvent) {
		event.stopPropagation();
		this.#closeContextMenu();
	}

	render() {
		return html`
			${this.#renderBackdrop()}
			<div id="relative-wrapper">
				<slot></slot>
				${this.#renderModal()}
			</div>
		`;
	}

	#renderBackdrop() {
		// TODO: add keyboard support (close on escape)
		// eslint-disable-next-line lit-a11y/click-events-have-key-events
		return this._isOpen ? html`<div id="backdrop" @click=${this.#closeContextMenu}></div>` : nothing;
	}

	// TODO: allow different views depending on left or right click
	#renderModal() {
		return this._isOpen
			? html`<div id="action-modal">
					<h3>${this._headline}</h3>
					<umb-entity-action-list
						@executed=${this.#onActionExecuted}
						entity-type=${ifDefined(this._entityType)}
						unique=${ifDefined(this._unique)}></umb-entity-action-list>
			  </div>`
			: nothing;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-sidebar-context-menu': UmbSectionSidebarContextMenuElement;
	}
}
