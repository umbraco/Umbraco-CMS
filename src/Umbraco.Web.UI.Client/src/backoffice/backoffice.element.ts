import { defineElement } from '@umbraco-ui/uui-base/lib/registration';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';

import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '../core/modal';
import { UmbUserStore } from './users/users/user.store';
import { UmbUserGroupStore } from './users/user-groups/user-group.store';
import { UmbCurrentUserStore, UMB_CURRENT_USER_STORE_CONTEXT_TOKEN } from './users/current-user/current-user.store';
import {
	UmbCurrentUserHistoryStore,
	UMB_CURRENT_USER_HISTORY_STORE_CONTEXT_TOKEN,
} from './users/current-user/current-user-history.store';

import {
	UmbBackofficeContext,
	UMB_BACKOFFICE_CONTEXT_TOKEN,
} from './shared/components/backoffice-frame/backoffice.context';
import { UmbDocumentTypeStore } from './documents/document-types/repository/document-type.store';
import { UmbDocumentTypeTreeStore } from './documents/document-types/repository/document-type.tree.store';
import { UmbMediaTypeDetailStore } from './media/media-types/repository/media-type.detail.store';
import { UmbMediaTypeTreeStore } from './media/media-types/repository/media-type.tree.store';
import { UmbDocumentStore } from './documents/documents/repository/document.store';
import { UmbDocumentTreeStore } from './documents/documents/repository/document.tree.store';
import { UmbMediaDetailStore } from './media/media/repository/media.detail.store';
import { UmbMediaTreeStore } from './media/media/repository/media.tree.store';
import { UmbMemberTypeDetailStore } from './members/member-types/repository/member-type.detail.store';
import { UmbMemberTypeTreeStore } from './members/member-types/repository/member-type.tree.store';
import { UmbMemberGroupDetailStore } from './members/member-groups/repository/member-group.detail.store';
import { UmbMemberGroupTreeStore } from './members/member-groups/repository/member-group.tree.store';
import { UmbMemberDetailStore } from './members/members/member.detail.store';
import { UmbMemberTreeStore } from './members/members/repository/member.tree.store';
import { UmbDictionaryDetailStore } from './translation/dictionary/repository/dictionary.detail.store';
import { UmbDictionaryTreeStore } from './translation/dictionary/repository/dictionary.tree.store';
import { UmbDocumentBlueprintDetailStore } from './documents/document-blueprints/document-blueprint.detail.store';
import { UmbDocumentBlueprintTreeStore } from './documents/document-blueprints/document-blueprint.tree.store';
import { UmbDataTypeStore } from './settings/data-types/repository/data-type.store';
import { UmbDataTypeTreeStore } from './settings/data-types/repository/data-type.tree.store';
import { UmbTemplateTreeStore } from './templating/templates/tree/data/template.tree.store';
import { UmbTemplateDetailStore } from './templating/templates/workspace/data/template.detail.store';
import { UmbThemeContext } from './themes/theme.context';
import { UmbLanguageStore } from './settings/languages/repository/language.store';
import {
	UMB_APP_LANGUAGE_CONTEXT_TOKEN,
	UmbAppLanguageContext,
} from './settings/languages/app-language-select/app-language.context';
import { UmbPackageStore } from './packages/repository/package.store';
import { UmbServerExtensionController } from './packages/repository/server-extension.controller';
import { UmbNotificationService, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/notification';
import { UmbLitElement } from '@umbraco-cms/element';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-api';

import '@umbraco-cms/router';

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
		`,
	];

	constructor() {
		super();

		this.provideContext(UMB_MODAL_SERVICE_CONTEXT_TOKEN, new UmbModalService());
		this.provideContext(UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN, new UmbNotificationService());

		// TODO: find a way this is possible outside this element. It needs to be possible to register stores in extensions
		this.provideContext(UMB_CURRENT_USER_STORE_CONTEXT_TOKEN, new UmbCurrentUserStore());

		new UmbDocumentStore(this);
		new UmbDocumentTreeStore(this);
		new UmbMediaDetailStore(this);
		new UmbMediaTreeStore(this);
		new UmbDataTypeStore(this);
		new UmbDataTypeTreeStore(this);
		new UmbUserStore(this);
		new UmbMediaTypeDetailStore(this);
		new UmbMediaTypeTreeStore(this);
		new UmbDocumentTypeStore(this);
		new UmbDocumentTypeTreeStore(this);
		new UmbMemberTypeDetailStore(this);
		new UmbMemberTypeTreeStore(this);
		new UmbUserGroupStore(this);
		new UmbMemberGroupDetailStore(this);
		new UmbMemberGroupTreeStore(this);
		new UmbMemberDetailStore(this);
		new UmbMemberTreeStore(this);
		new UmbDictionaryDetailStore(this);
		new UmbDictionaryTreeStore(this);
		new UmbDocumentBlueprintDetailStore(this);
		new UmbDocumentBlueprintTreeStore(this);
		new UmbTemplateTreeStore(this);
		new UmbTemplateDetailStore(this);
		new UmbLanguageStore(this);

		this.provideContext(UMB_APP_LANGUAGE_CONTEXT_TOKEN, new UmbAppLanguageContext(this));
		this.provideContext(UMB_BACKOFFICE_CONTEXT_TOKEN, new UmbBackofficeContext());
		this.provideContext(UMB_CURRENT_USER_HISTORY_STORE_CONTEXT_TOKEN, new UmbCurrentUserHistoryStore());
		new UmbThemeContext(this);

		new UmbPackageStore(this);
		new UmbServerExtensionController(this, umbExtensionsRegistry);
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
