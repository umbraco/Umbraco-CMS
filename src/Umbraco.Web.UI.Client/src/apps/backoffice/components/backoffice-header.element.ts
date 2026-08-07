import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_BACKOFFICE_CONTEXT } from '../backoffice.context.js';
import type { UmbBackofficeContext } from '../backoffice.context.js';
import { UMB_MOBILE_BREAKPOINT } from '@umbraco-cms/backoffice/const';

@customElement('umb-backoffice-header')
export class UmbBackofficeHeaderElement extends UmbLitElement {
	#backofficeContext?: UmbBackofficeContext;

	@state()
	private _sidebarOpen = false;

	constructor() {
		super();
		this.consumeContext(UMB_BACKOFFICE_CONTEXT, (ctx) => {
			this.#backofficeContext = ctx;
			this.observe(
				ctx?.mobileSidebarOpen,
				(open) => {
					this._sidebarOpen = open === true;
				},
				'_observeMobileSidebarOpen',
			);
		});
	}
	#onBurgerClick() {
		this.#backofficeContext?.toggleMobileSidebar();
	}
	override render() {
		return html`
			<div id="appHeader">
				<umb-backoffice-header-logo></umb-backoffice-header-logo>
				<button
					id="burger-button"
					@click=${this.#onBurgerClick}
					aria-label="Toggle navigation"
					aria-expanded=${this._sidebarOpen}>
					<uui-icon name="icon-list"></uui-icon>
				</button>
				<umb-backoffice-header-sections></umb-backoffice-header-sections>
				<umb-backoffice-header-apps></umb-backoffice-header-apps>
			</div>
		`;
	}

	static override readonly styles = [
		css`
			:host {
				width: 100%;
			}

			#appHeader {
				--uui-focus-outline-color: var(--uui-color-header-contrast-emphasis);
				background-color: var(--umb-header-background-color, var(--uui-color-header-surface));
				display: flex;
				align-items: center;
				justify-content: space-between;
				padding: 0 var(--uui-size-space-5);
			}

			umb-backoffice-header-sections {
				flex: 1 1 auto;
			}

			#burger-button {
				display: none;
				background: transparent;
				border: none;
				color: var(--uui-color-header-contrast);
				cursor: pointer;
				padding: var(--uui-size-space-2);
				font-size: 20px;
				align-items: center;
				justify-content: center;
			}

			@media (max-width: ${UMB_MOBILE_BREAKPOINT}px) {
				#burger-button {
					display: flex;
				}
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-header': UmbBackofficeHeaderElement;
	}
}
