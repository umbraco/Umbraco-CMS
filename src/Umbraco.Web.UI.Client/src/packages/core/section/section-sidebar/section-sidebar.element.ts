import { UmbSectionSidebarContext, UMB_SECTION_SIDEBAR_CONTEXT_TOKEN } from './section-sidebar.context.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html , customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-section-sidebar')
export class UmbSectionSidebarElement extends UmbLitElement {
	#sectionSidebarContext = new UmbSectionSidebarContext(this);

	constructor() {
		super();
		this.provideContext(UMB_SECTION_SIDEBAR_CONTEXT_TOKEN, this.#sectionSidebarContext);
	}

	render() {
		return html`
			<umb-section-sidebar-context-menu>
				<uui-scroll-container id="scroll-container">
					<slot></slot>
				</uui-scroll-container>
			</umb-section-sidebar-context-menu>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				flex: 0 0 var(--umb-section-sidebar-layout-width);
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
