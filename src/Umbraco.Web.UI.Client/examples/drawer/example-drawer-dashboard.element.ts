import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_DRAWER_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/drawer';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';

@customElement('example-drawer-dashboard')
export class UmbExampleDrawerDashboardElement extends UmbLitElement {
	#drawerCtx?: typeof UMB_DRAWER_MANAGER_CONTEXT.TYPE;

	constructor() {
		super();
		this.consumeContext(UMB_DRAWER_MANAGER_CONTEXT, (ctx) => {
			this.#drawerCtx = ctx;
		});
	}

	#openDrawer() {
		this.#drawerCtx?.open('example.drawer.poc');
	}

	#closeDrawer() {
		this.#drawerCtx?.close();
	}

	async #openModal() {
		await umbConfirmModal(this, {
			headline: 'Modal on top of drawer',
			content: 'When this closes, the drawer should still be on top.',
		});
	}

	override render() {
		return html`
			<uui-box headline="Drawer POC">
				<p>Open the drawer, then open a modal on top to verify it stays interactive and re-promotes itself.</p>
				<uui-button look="primary" @click=${this.#openDrawer}>Open Drawer</uui-button>
				<uui-button @click=${this.#closeDrawer}>Close Drawer</uui-button>
				<uui-button look="secondary" @click=${this.#openModal}>Open Modal on top</uui-button>
			</uui-box>
		`;
	}

	static override styles = [
		css`
			:host {
				display: block;
				padding: 20px;
			}
			uui-button {
				margin-right: 8px;
			}
		`,
	];
}

export default UmbExampleDrawerDashboardElement;

declare global {
	interface HTMLElementTagNameMap {
		'example-drawer-dashboard': UmbExampleDrawerDashboardElement;
	}
}
