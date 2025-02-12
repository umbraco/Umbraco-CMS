import { UmbSectionSidebarContext } from './section-sidebar.context.js';
import { UMB_MARK_ATTRIBUTE_NAME } from '@umbraco-cms/backoffice/const';
import { css, html, customElement, type PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-section-sidebar')
export class UmbSectionSidebarElement extends UmbLitElement {
	constructor() {
		super();
		new UmbSectionSidebarContext(this);
	}

	protected override firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.firstUpdated(_changedProperties);
		this.setAttribute(UMB_MARK_ATTRIBUTE_NAME, 'section-sidebar');
	}

	override render() {
		return html`
			<umb-section-sidebar-context-menu>
				<uui-scroll-container id="scroll-container">
					<slot></slot>
				</uui-scroll-container>
			</umb-section-sidebar-context-menu>
		`;
	}

	static override styles = [
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
				position: relative;
				box-sizing: border-box;
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
