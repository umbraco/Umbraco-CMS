import { defineElement } from '@umbraco-ui/uui-base/lib/registration';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { UmbExtensionInitializer } from '../../packages/packages/repository/server-extension.controller';
import { UmbBackofficeContext, UMB_BACKOFFICE_CONTEXT_TOKEN } from './backoffice.context';
import { UmbEntryPointExtensionInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

const CORE_PACKAGES = [
	import('../../packages/core/umbraco-package'),
	import('../../packages/settings/umbraco-package'),
	import('../../packages/documents/umbraco-package'),
	import('../../packages/media/umbraco-package'),
	import('../../packages/members/umbraco-package'),
	import('../../packages/translation/umbraco-package'),
	import('../../packages/users/umbraco-package'),
	import('../../packages/packages/umbraco-package'),
	import('../../packages/search/umbraco-package'),
	import('../../packages/templating/umbraco-package'),
	import('../../packages/umbraco-news/umbraco-package'),
	import('../../packages/tags/umbraco-package'),
];

@defineElement('umb-backoffice')
export class UmbBackofficeElement extends UmbLitElement {
	constructor() {
		super();
		this.provideContext(UMB_BACKOFFICE_CONTEXT_TOKEN, new UmbBackofficeContext());
		new UmbEntryPointExtensionInitializer(this, umbExtensionsRegistry);
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
