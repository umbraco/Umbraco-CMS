import { UmbBackofficeContext, UMB_BACKOFFICE_CONTEXT_TOKEN } from './backoffice.context.js';
import { UmbServerExtensionRegistrator } from './server-extension-registrator.controller.js';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import {
	UmbBundleExtensionInitializer,
	UmbEntryPointExtensionInitializer,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './components/index.js';

// TODO: temp solution to load core packages
const CORE_PACKAGES = [
	import('../../packages/core/umbraco-package.js'),
	import('../../packages/settings/umbraco-package.js'),
	import('../../packages/documents/umbraco-package.js'),
	import('../../packages/media/umbraco-package.js'),
	import('../../packages/members/umbraco-package.js'),
	import('../../packages/dictionary/umbraco-package.js'),
	import('../../packages/users/umbraco-package.js'),
	import('../../packages/packages/umbraco-package.js'),
	import('../../packages/search/umbraco-package.js'),
	import('../../packages/templating/umbraco-package.js'),
	import('../../packages/umbraco-news/umbraco-package.js'),
	import('../../packages/tags/umbraco-package.js'),
];

@customElement('umb-backoffice')
export class UmbBackofficeElement extends UmbLitElement {

	/**
	 * Backoffice extension registry.
	 * This enables to registerer og unregister extensions via DevTools, or just via querying this element via the DOM.
	 * @type {UmbExtensionsRegistry}
	 */
	public registry = umbExtensionsRegistry;

	constructor() {
		super();

		this.provideContext(UMB_BACKOFFICE_CONTEXT_TOKEN, new UmbBackofficeContext(this));
		new UmbBundleExtensionInitializer(this, umbExtensionsRegistry);
		new UmbEntryPointExtensionInitializer(this, umbExtensionsRegistry);
		new UmbServerExtensionRegistrator(this, umbExtensionsRegistry);

		// So far local packages are this simple to registerer, so no need for a manager to do that:
		CORE_PACKAGES.forEach(async (packageImport) => {
			const packageModule = await packageImport;
			umbExtensionsRegistry.registerMany(packageModule.extensions);
		});
	}

	render() {
		return html`
			<umb-backoffice-header></umb-backoffice-header>
			<umb-backoffice-main></umb-backoffice-main>
			<slot></slot>
		`;
	}

	static styles = [
		UUITextStyles,
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
