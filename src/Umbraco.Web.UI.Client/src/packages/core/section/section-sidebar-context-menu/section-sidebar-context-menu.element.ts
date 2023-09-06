import { UmbSectionSidebarContext, UMB_SECTION_SIDEBAR_CONTEXT_TOKEN } from '../section-sidebar/index.js';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { css, html, nothing, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-section-sidebar-context-menu')
export class UmbSectionSidebarContextMenuElement extends UmbLitElement {
	#sectionSidebarContext?: UmbSectionSidebarContext;

	@state()
	private _isOpen = false;

	@state()
	private _entityType?: string;

	@state()
	private _unique?: string | null;

	@state()
	private _headline?: string;

	constructor() {
		super();

		this.consumeContext(UMB_SECTION_SIDEBAR_CONTEXT_TOKEN, (instance) => {
			this.#sectionSidebarContext = instance;

			if (this.#sectionSidebarContext) {
				// make prettier not break the lines on the next 4 lines:
				// prettier-ignore
				this.observe( this.#sectionSidebarContext.contextMenuIsOpen, (value) => (this._isOpen = value), '_observeContextMenuIsOpen');
				// prettier-ignore
				this.observe(this.#sectionSidebarContext.unique, (value) => (this._unique = value), '_observeUnique');
				// prettier-ignore
				this.observe(this.#sectionSidebarContext.entityType, (value) => (this._entityType = value), '_observeEntityType');
				// prettier-ignore
				this.observe(this.#sectionSidebarContext.headline, (value) => (this._headline = value), '_observeHeadline');
			} else {
				this.removeControllerByAlias('_observeContextMenuIsOpen');
				this.removeControllerByAlias('_observeUnique');
				this.removeControllerByAlias('_observeEntityType');
				this.removeControllerByAlias('_observeHeadline');
			}
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
		return this._isOpen && this._unique !== undefined && this._entityType
			? html`<div id="action-modal">
					<h3>${this._headline}</h3>
					<umb-entity-action-list
						@executed=${this.#onActionExecuted}
						.entityType=${this._entityType}
						.unique=${this._unique}></umb-entity-action-list>
			  </div>`
			: nothing;
	}

	static styles = [
		UmbTextStyles,
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
				left: var(--umb-section-sidebar-width);
				height: 100%;
				z-index: 1;
				top: 0;
				width: var(--umb-section-sidebar-width);
				border: none;
				border-left: 1px solid var(--uui-color-border);
				border-right: 1px solid var(--uui-color-border);
				background-color: var(--uui-color-surface);
			}

			#action-modal h3 {
				padding: var(--uui-size-4) var(--uui-size-8);
				margin: 0;
				min-height: var(--umb-header-layout-height);
				box-sizing: border-box;
				display: flex;
				align-items: center;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-sidebar-context-menu': UmbSectionSidebarContextMenuElement;
	}
}
