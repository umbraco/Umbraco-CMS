import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbSectionSidebarContext, UMB_SECTION_SIDEBAR_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/section';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import '../../tree/context-menu/tree-context-menu.service';
import '../section-sidebar-context-menu/section-sidebar-context-menu.element';

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
				flex: 0 0 300px;
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
