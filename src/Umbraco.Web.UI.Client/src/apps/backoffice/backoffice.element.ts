import { UMB_LOCALIZATION_CONTEXT, UmbLocalizationContext } from '@umbraco-cms/backoffice/localization-api';
import { UmbBackofficeContext, UMB_BACKOFFICE_CONTEXT_TOKEN } from './backoffice.context.js';
import { UmbExtensionInitializer } from './extension.controller.js';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import {
	UmbBundleExtensionInitializer,
	UmbEntryPointExtensionInitializer,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './components/index.js';
import { UMB_AUTH } from '@umbraco-cms/backoffice/auth';

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
	constructor() {
		super();
		this.provideContext(UMB_BACKOFFICE_CONTEXT_TOKEN, new UmbBackofficeContext());
		new UmbBundleExtensionInitializer(this, umbExtensionsRegistry);
		new UmbEntryPointExtensionInitializer(this, umbExtensionsRegistry);

		const extensionInitializer = new UmbExtensionInitializer(this, umbExtensionsRegistry);
		extensionInitializer.setLocalPackages(CORE_PACKAGES);

		const localizationContext = new UmbLocalizationContext(umbExtensionsRegistry);
		this.provideContext(UMB_LOCALIZATION_CONTEXT, localizationContext);

		this.consumeContext(UMB_AUTH, (auth) => {
			this.observe(auth.currentUser, (user) => {
				if (user) {
					const languageIsoCode = user.languageIsoCode ?? 'en';
					localizationContext.setLanguage(languageIsoCode);
				}
			});
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
