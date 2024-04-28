import { UmbSectionSidebarContext } from './section-sidebar.context.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-section-sidebar')
export class UmbSectionSidebarElement extends UmbLitElement {
	@state()
	private _isOpen = false;

	#sectionSidebarContext = new UmbSectionSidebarContext(this);

	constructor() {
		super();

		this.observe(this.#sectionSidebarContext.contextMenuIsOpen, (value) => {
			this._isOpen = value;
		});
	}

	render() {
		return this._isOpen
			? html` <umb-section-sidebar-context-menu> ${this.#renderScrollContainer()} </umb-section-sidebar-context-menu> `
			: this.#renderScrollContainer();
	}

	#renderScrollContainer() {
		return html` <uui-scroll-container id="scroll-container"> <slot></slot> </uui-scroll-container> `;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				flex: 0 0 var(--umb-section-sidebar-width);
				background-color: var(--uui-color-surface);
				height: 100%;
				border-right: 1px solid var(--uui-color-border);
				font-weight: 500;
				display: flex;
				flex-direction: column;
				z-index: 10;
			}

			#scroll-container {
				height: 100%;
				overflow-y: auto;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-sidebar': UmbSectionSidebarElement;
	}
}
