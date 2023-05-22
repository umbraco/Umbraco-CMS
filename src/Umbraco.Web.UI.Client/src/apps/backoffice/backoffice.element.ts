import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { customElement, property } from 'lit/decorators.js';
import { UmbExtensionInitializer } from './extension.controller';
import { UmbBackofficeContext, UMB_BACKOFFICE_CONTEXT_TOKEN } from './backoffice.context';
import { UmbEntryPointExtensionInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './components';

@customElement('umb-backoffice')
export class UmbBackofficeElement extends UmbLitElement {
	@property()
	set localPackages(value: Array<Promise<any>>) {
		this.#extensionInitializer.setLocalPackages(value);
	}

	#extensionInitializer = new UmbExtensionInitializer(this, umbExtensionsRegistry);

	constructor() {
		super();
		this.provideContext(UMB_BACKOFFICE_CONTEXT_TOKEN, new UmbBackofficeContext());
		new UmbEntryPointExtensionInitializer(this, umbExtensionsRegistry);
		new UmbExtensionInitializer(this, umbExtensionsRegistry);
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
