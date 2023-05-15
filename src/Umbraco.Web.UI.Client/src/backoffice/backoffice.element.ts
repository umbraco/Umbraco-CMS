import { defineElement } from '@umbraco-ui/uui-base/lib/registration';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { UmbStoreExtensionInitializer } from '../core/store-extension-initializer';
import {
	UmbBackofficeContext,
	UMB_BACKOFFICE_CONTEXT_TOKEN,
} from './core/components/backoffice-frame/backoffice.context';
import { UmbExtensionInitializer } from './packages/repository/server-extension.controller';
import { UmbEntryPointExtensionInitializer, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

const CORE_PACKAGES = [
	import('./core/umbraco-package'),
	import('./settings/umbraco-package'),
	import('./documents/umbraco-package'),
	import('./media/umbraco-package'),
	import('./members/umbraco-package'),
	import('./translation/umbraco-package'),
	import('./users/umbraco-package'),
	import('./packages/umbraco-package'),
	import('./search/umbraco-package'),
	import('./templating/umbraco-package'),
	import('./umbraco-news/umbraco-package'),
	import('./tags/umbraco-package'),
];

@defineElement('umb-backoffice')
export class UmbBackofficeElement extends UmbLitElement {
	constructor() {
		super();
		this.provideContext(UMB_BACKOFFICE_CONTEXT_TOKEN, new UmbBackofficeContext());
		new UmbEntryPointExtensionInitializer(this, umbExtensionsRegistry);
		new UmbStoreExtensionInitializer(this);
		new UmbExtensionInitializer(this, umbExtensionsRegistry, CORE_PACKAGES);
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
