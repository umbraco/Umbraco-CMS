import { UmbBackofficeContext } from './backoffice.context.js';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import {
	UmbBackofficeEntryPointExtensionInitializer,
	UmbEntryPointExtensionInitializer,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import './components/index.js';

@customElement('umb-backoffice')
export class UmbBackofficeElement extends UmbLitElement {
	/**
	 * Backoffice extension registry.
	 * This enables to register and unregister extensions via DevTools, or just via querying this element via the DOM.
	 */
	public extensionRegistry = umbExtensionsRegistry;

	constructor() {
		super();

		new UmbBackofficeContext(this);

		new UmbBackofficeEntryPointExtensionInitializer(this, umbExtensionsRegistry);
		new UmbEntryPointExtensionInitializer(this, umbExtensionsRegistry);
	}

	override render() {
		return html`
			<umb-backoffice-header></umb-backoffice-header>
			<umb-backoffice-main></umb-backoffice-main>
			<slot></slot>
		`;
	}

	static override readonly styles = [
		css`
			:host {
				display: flex;
				flex-direction: column;
				height: 100%;
				width: 100%;
				color: var(--uui-color-text);
				font-size: 14px;
				box-sizing: border-box;
			}
		`,
	];
}

export default UmbBackofficeElement;
declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice': UmbBackofficeElement;
	}
}
