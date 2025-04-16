import { UmbBackofficeContext } from './backoffice.context.js';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import {
	UmbBackofficeEntryPointExtensionInitializer,
	UmbEntryPointExtensionInitializer,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';
import { UmbServerExtensionRegistrator } from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';

import './components/index.js';

const CORE_PACKAGES = [
	import('../../block/umbraco-package.js'),
	import('../../clipboard/umbraco-package.js'),
	import('../../code-editor/umbraco-package.js'),
	import('../../data-type/umbraco-package.js'),
	import('../../dictionary/umbraco-package.js'),
	import('../../documents/umbraco-package.js'),
	import('../../embedded-media/umbraco-package.js'),
	import('../../extension-insights/umbraco-package.js'),
	import('../../health-check/umbraco-package.js'),
	import('../../help/umbraco-package.js'),
	import('../../language/umbraco-package.js'),
	import('../../log-viewer/umbraco-package.js'),
	import('../../markdown-editor/umbraco-package.js'),
	import('../../media/umbraco-package.js'),
	import('../../members/umbraco-package.js'),
	import('../../models-builder/umbraco-package.js'),
	import('../../multi-url-picker/umbraco-package.js'),
	import('../../packages/umbraco-package.js'),
	import('../../performance-profiling/umbraco-package.js'),
	import('../../property-editors/umbraco-package.js'),
	import('../../publish-cache/umbraco-package.js'),
	import('../../relations/umbraco-package.js'),
	import('../../rte/umbraco-package.js'),
	import('../../search/umbraco-package.js'),
	import('../../settings/umbraco-package.js'),
	import('../../static-file/umbraco-package.js'),
	import('../../sysinfo/umbraco-package.js'),
	import('../../tags/umbraco-package.js'),
	import('../../telemetry/umbraco-package.js'),
	import('../../templating/umbraco-package.js'),
	import('../../tiptap/umbraco-package.js'),
	import('../../translation/umbraco-package.js'),
	import('../../ufm/umbraco-package.js'),
	import('../../umbraco-news/umbraco-package.js'),
	import('../../user/umbraco-package.js'),
	import('../../webhook/umbraco-package.js'),
	import('../../content/umbraco-package.js'),
];

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

		// So far local packages are this simple to registerer, so no need for a manager to do that:
		CORE_PACKAGES.forEach(async (packageImport) => {
			const packageModule = await packageImport;
			umbExtensionsRegistry.registerMany(packageModule.extensions);
		});

		const serverExtensions = new UmbServerExtensionRegistrator(this, umbExtensionsRegistry);

		// TODO: We need to ensure this request is called every time the user logs in, but this should be done somewhere across the app and not here [JOV]
		this.consumeContext(UMB_AUTH_CONTEXT, (authContext) => {
			this.observe(authContext.isAuthorized, (isAuthorized) => {
				if (!isAuthorized) return;
				serverExtensions.registerPrivateExtensions();
			});
		});
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
