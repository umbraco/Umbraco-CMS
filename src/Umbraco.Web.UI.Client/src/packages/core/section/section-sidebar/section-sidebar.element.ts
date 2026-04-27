import { UmbSectionSidebarContext } from './section-sidebar.context.js';
import { UMB_MARK_ATTRIBUTE_NAME } from '@umbraco-cms/backoffice/const';
import { css, html, customElement, type PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-section-sidebar')
export class UmbSectionSidebarElement extends UmbLitElement {
	readonly #mobileQuery = window.matchMedia('(max-width: 920px)');

	constructor() {
		super();
		new UmbSectionSidebarContext(this);
	}

	protected override firstUpdated(_changedProperties: PropertyValueMap<unknown> | Map<PropertyKey, unknown>): void {
		super.firstUpdated(_changedProperties);
		this.setAttribute(UMB_MARK_ATTRIBUTE_NAME, 'section-sidebar');

		// The router already patches history.pushState and dispatches 'changestate' on window.
		// Listening here avoids double-patching history and works for push, replace, and popstate.
		window.addEventListener('changestate', this.#onNavigation);
		window.addEventListener('umb-section-sidebar-toggle', this.#onToggle);
		this.#mobileQuery.addEventListener('change', this.#onMobileChange);
	}

	override disconnectedCallback(): void {
		super.disconnectedCallback();
		window.removeEventListener('changestate', this.#onNavigation);
		window.removeEventListener('umb-section-sidebar-toggle', this.#onToggle);
		this.#mobileQuery.removeEventListener('change', this.#onMobileChange);
	}

	#onToggle = () => {
		if (this.hasAttribute('mobile-expanded')) {
			this.#collapse();
		} else {
			this.setAttribute('mobile-expanded', '');
		}
	};

	#onNavigation = () => {
		if (this.#mobileQuery.matches) this.#collapse();
	};

	#onMobileChange = (e: MediaQueryListEvent) => {
		if (!e.matches) this.#collapse();
	};

	#collapse() {
		this.removeAttribute('mobile-expanded');
	}

	override render() {
		return html`
			<uui-scroll-container id="scroll-container">
				<slot></slot>
			</uui-scroll-container>
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
				position: relative;
				z-index: 2;
				height: 100%;
				overflow-y: auto;
				background-color: var(--uui-color-surface);
			}

			@media (max-width: 920px) {
				:host {
					display: none;
					border-right: none;
					border-bottom: none;
				}

				:host([mobile-expanded]) {
					display: flex;
					height: 100%;
					overflow: visible;
				}
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-sidebar': UmbSectionSidebarElement;
	}
}
