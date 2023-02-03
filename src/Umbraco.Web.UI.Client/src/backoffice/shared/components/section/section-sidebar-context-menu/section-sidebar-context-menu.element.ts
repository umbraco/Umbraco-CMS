import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import {
	UmbSectionSidebarContext,
	UMB_SECTION_SIDEBAR_CONTEXT_TOKEN,
} from '../section-sidebar/section-sidebar.context';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-section-sidebar-context-menu')
export class UmbSectionSidebarContextMenu extends UmbLitElement {
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

	connectedCallback(): void {
		super.connectedCallback();

		this.consumeContext(UMB_SECTION_SIDEBAR_CONTEXT_TOKEN, (instance) => {
			this.#sectionSidebarContext = instance;

			this.observe(this.#sectionSidebarContext.contextMenuIsOpen, (isOpen) => {
				this._isOpen = isOpen;
				this._entityType = this.#sectionSidebarContext?.getEntityType();
				this._unique = this.#sectionSidebarContext?.getUnique();
			});
		});
	}

	#closeContextMenu() {
		this.#sectionSidebarContext?.closeContextMenu();
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
					<umb-entity-action-list
						entity-type=${ifDefined(this._entityType)}
						unique=${ifDefined(this._unique)}></umb-entity-action-list>
			  </div>`
			: nothing;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-sidebar-context-menu': UmbSectionSidebarContextMenu;
	}
}
