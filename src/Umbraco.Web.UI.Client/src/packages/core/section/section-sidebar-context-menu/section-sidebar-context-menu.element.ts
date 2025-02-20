import type { UmbSectionSidebarContext } from '../section-sidebar/index.js';
import { UMB_SECTION_SIDEBAR_CONTEXT } from '../section-sidebar/index.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, nothing, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import type { UmbContextRequestEvent } from '@umbraco-cms/backoffice/context-api';

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

		this.consumeContext(UMB_SECTION_SIDEBAR_CONTEXT, (instance) => {
			this.#sectionSidebarContext = instance;
			this.#observeEntityModel();

			if (this.#sectionSidebarContext) {
				// make prettier not break the lines on the next 4 lines:
				// prettier-ignore
				this.observe( this.#sectionSidebarContext.contextMenuIsOpen, (value) => (this._isOpen = value), '_observeContextMenuIsOpen');
				// prettier-ignore
				this.observe(this.#sectionSidebarContext.headline, (value) => (this._headline = value), '_observeHeadline');
			} else {
				this.removeUmbControllerByAlias('_observeContextMenuIsOpen');
				this.removeUmbControllerByAlias('_observeHeadline');
			}
		});
	}

	#observeEntityModel() {
		if (!this.#sectionSidebarContext) {
			this.removeUmbControllerByAlias('_observeEntityModel');
			return;
		}

		this.observe(
			observeMultiple([this.#sectionSidebarContext.unique, this.#sectionSidebarContext.entityType]),
			(values) => {
				this._unique = values[0];
				this._entityType = values[1];
			},
			'_observeEntityModel',
		);
	}

	#closeContextMenu() {
		this.#sectionSidebarContext?.closeContextMenu();
	}

	#onActionExecuted(event: CustomEvent) {
		event.stopPropagation();
		this.#closeContextMenu();
	}

	#proxyContextRequests(event: UmbContextRequestEvent) {
		if (!this.#sectionSidebarContext) return;
		// Note for this hack (The if-sentence):  [NL]
		// We do not currently have a good enough control to ensure that the proxy is last, meaning if another context is provided at this element, it might respond after the proxy event has been dispatched.
		// To avoid such this hack just prevents proxying the event if its a request for its own context.
		if (event.contextAlias !== UMB_SECTION_SIDEBAR_CONTEXT.contextAlias) {
			event.stopImmediatePropagation();
			this.#sectionSidebarContext.getContextElement()?.dispatchEvent(event.clone());
		}
	}

	override render() {
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

		return this._isOpen ? html`<div id="backdrop" @click=${this.#closeContextMenu}></div>` : nothing;
	}

	#renderModal() {
		return this._isOpen && this._unique !== undefined && this._entityType
			? html`<uui-scroll-container id="action-modal" @umb:context-request=${this.#proxyContextRequests}>
					${this._headline ? html`<h3>${this.localize.string(this._headline)}</h3>` : nothing}
					<umb-entity-action-list
						@action-executed=${this.#onActionExecuted}
						.entityType=${this._entityType}
						.unique=${this._unique}></umb-entity-action-list>
				</uui-scroll-container>`
			: nothing;
	}

	static override styles = [
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
				height: 100%;
				z-index: 1;
				top: 0;
				right: calc(var(--umb-section-sidebar-width) * -1);
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
			#action-modal umb-entity-action-list {
				--uui-menu-item-flat-structure: 0;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-sidebar-context-menu': UmbSectionSidebarContextMenuElement;
	}
}
