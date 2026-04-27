import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_DRAWER_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/drawer';

@customElement('example-drawer')
export class UmbExampleDrawerElement extends UmbLitElement {
	#drawerCtx?: typeof UMB_DRAWER_MANAGER_CONTEXT.TYPE;

	constructor() {
		super();
		this.consumeContext(UMB_DRAWER_MANAGER_CONTEXT, (ctx) => {
			this.#drawerCtx = ctx;
		});
	}

	#close() {
		this.#drawerCtx?.close();
	}

	override render() {
		return html`
			<header>
				<h2>Example Drawer</h2>
				<uui-button label="Close" compact @click=${this.#close}>×</uui-button>
			</header>
			<div class="body">
				<p>Hello from the drawer.</p>
				<p>Open a modal from the dashboard — this drawer should stay on top.</p>
			</div>
		`;
	}

	static override styles = [
		css`
			:host {
				display: flex;
				flex-direction: column;
				height: 100%;
			}
			header {
				display: flex;
				align-items: center;
				justify-content: space-between;
				padding: 16px;
				border-bottom: 1px solid var(--uui-color-divider);
			}
			.body {
				padding: 16px;
			}
		`,
	];
}

export default UmbExampleDrawerElement;

declare global {
	interface HTMLElementTagNameMap {
		'example-drawer': UmbExampleDrawerElement;
	}
}
