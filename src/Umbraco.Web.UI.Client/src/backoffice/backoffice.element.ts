import { defineElement } from '@umbraco-ui/uui-base/lib/registration';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';

import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_ALIAS } from '../core/modal';
import { UmbNotificationService, UMB_NOTIFICATION_SERVICE_CONTEXT_ALIAS } from '../core/notification';
import { UmbUserStore, UMB_USER_STORE_CONTEXT_ALIAS } from './users/users/user.store';
import { UmbUserGroupStore } from './users/user-groups/user-group.store';
import { UmbCurrentUserStore, UMB_CURRENT_USER_STORE_CONTEXT_ALIAS } from './users/current-user/current-user.store';
import { UmbCurrentUserHistoryStore } from './users/current-user/current-user-history.store';

import {
	UmbDocumentTypeStore,
	UMB_DOCUMENT_TYPE_STORE_CONTEXT_ALIAS,
} from './documents/document-types/document-type.store';
import { UmbMediaTypeStore, UMB_MEDIA_TYPE_STORE_CONTEXT_ALIAS } from './media/media-types/media-type.store';
import { UmbMemberTypeStore, UMB_MEMBER_TYPE_STORE_CONTEXT_ALIAS } from './members/member-types/member-type.store';
import { UmbDocumentStore, UMB_DOCUMENT_STORE_CONTEXT_ALIAS } from './documents/documents/document.store';
import { UmbMediaStore, UMB_MEDIA_STORE_CONTEXT_ALIAS } from './media/media/media.store';
import { UmbMemberGroupStore } from './members/member-groups/member-group.store';
import { UmbDictionaryStore } from './translation/dictionary/dictionary.store';
import { UmbDocumentBlueprintStore } from './documents/document-blueprints/document-blueprint.store';

import { UmbSectionStore } from './shared/components/section/section.store';
import { UmbDataTypeStore, UMB_DATA_TYPE_STORE_CONTEXT_ALIAS } from './settings/data-types/data-type.store';
import { UmbLitElement } from '@umbraco-cms/element';

// Domains
import './settings';
import './documents';
import './media';
import './members';
import './translation';
import './users';
import './packages';
import './search';
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

		this.provideContext(UMB_MODAL_SERVICE_CONTEXT_ALIAS, new UmbModalService());
		this.provideContext(UMB_NOTIFICATION_SERVICE_CONTEXT_ALIAS, new UmbNotificationService());

		// TODO: find a way this is possible outside this element. It needs to be possible to register stores in extensions
		this.provideContext(UMB_CURRENT_USER_STORE_CONTEXT_ALIAS, new UmbCurrentUserStore());
		this.provideContext(UMB_DOCUMENT_STORE_CONTEXT_ALIAS, new UmbDocumentStore(this));
		this.provideContext(UMB_MEDIA_STORE_CONTEXT_ALIAS, new UmbMediaStore(this));
		this.provideContext(UMB_DATA_TYPE_STORE_CONTEXT_ALIAS, new UmbDataTypeStore(this));
		this.provideContext(UMB_DOCUMENT_TYPE_STORE_CONTEXT_ALIAS, new UmbDocumentTypeStore(this));
		this.provideContext(UMB_MEDIA_TYPE_STORE_CONTEXT_ALIAS, new UmbMediaTypeStore(this));
		this.provideContext(UMB_MEMBER_TYPE_STORE_CONTEXT_ALIAS, new UmbMemberTypeStore(this));
		this.provideContext(UMB_USER_STORE_CONTEXT_ALIAS, new UmbUserStore(this));
		this.provideContext('umbUserGroupStore', new UmbUserGroupStore(this));
		this.provideContext('umbMemberGroupStore', new UmbMemberGroupStore(this));
		this.provideContext('umbSectionStore', new UmbSectionStore());
		this.provideContext('umbCurrentUserHistoryStore', new UmbCurrentUserHistoryStore());
		this.provideContext('umbDictionaryStore', new UmbDictionaryStore(this));
		this.provideContext('umbDocumentBlueprintStore', new UmbDocumentBlueprintStore(this));
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
