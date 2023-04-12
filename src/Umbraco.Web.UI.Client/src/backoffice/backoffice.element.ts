import { defineElement } from '@umbraco-ui/uui-base/lib/registration';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';

import { UmbCurrentUserStore, UMB_CURRENT_USER_STORE_CONTEXT_TOKEN } from './users/current-user/current-user.store';
import {
	UmbCurrentUserHistoryStore,
	UMB_CURRENT_USER_HISTORY_STORE_CONTEXT_TOKEN,
} from './users/current-user/current-user-history.store';

import {
	UmbBackofficeContext,
	UMB_BACKOFFICE_CONTEXT_TOKEN,
} from './shared/components/backoffice-frame/backoffice.context';
import { UmbThemeContext } from './themes/theme.context';
import {
	UMB_APP_LANGUAGE_CONTEXT_TOKEN,
	UmbAppLanguageContext,
} from './settings/languages/app-language-select/app-language.context';
import { UmbServerExtensionController } from './packages/repository/server-extension.controller';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/modal';
import { createExtensionClass, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-api';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';
import { UmbEntryPointExtensionInitializer } from '@umbraco-cms/backoffice/extensions-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

// Domains
import './settings';
import './documents';
import './media';
import './members';
import './translation';
import './users';
import './packages';
import './search';
import './templating';
import './shared';

@defineElement('umb-backoffice')
export class UmbBackofficeElement extends UmbLitElement {
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
			umb-backoffice-modal-container {
				z-index: 1000;
			}
		`,
	];

	constructor() {
		super();

		new UmbEntryPointExtensionInitializer(this, umbExtensionsRegistry);

		this.provideContext(UMB_MODAL_CONTEXT_TOKEN, new UmbModalContext(this));
		this.provideContext(UMB_NOTIFICATION_CONTEXT_TOKEN, new UmbNotificationContext());
		this.provideContext(UMB_CURRENT_USER_STORE_CONTEXT_TOKEN, new UmbCurrentUserStore());
		this.provideContext(UMB_APP_LANGUAGE_CONTEXT_TOKEN, new UmbAppLanguageContext(this));
		this.provideContext(UMB_BACKOFFICE_CONTEXT_TOKEN, new UmbBackofficeContext());
		new UmbThemeContext(this);
		new UmbServerExtensionController(this, umbExtensionsRegistry);
		this.provideContext(UMB_CURRENT_USER_HISTORY_STORE_CONTEXT_TOKEN, new UmbCurrentUserHistoryStore());

		// Register All Stores
		this.observe(umbExtensionsRegistry.extensionsOfTypes(['store', 'treeStore']), (stores) => {
			stores.forEach((store) => createExtensionClass(store, [this]));
		});
	}

	render() {
		return html`
			<umb-backoffice-header></umb-backoffice-header>
			<umb-backoffice-main></umb-backoffice-main>
			<umb-backoffice-notification-container></umb-backoffice-notification-container>
			<umb-backoffice-modal-container></umb-backoffice-modal-container>
		`;
	}
}

export default UmbBackofficeElement;
declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice': UmbBackofficeElement;
	}
}
