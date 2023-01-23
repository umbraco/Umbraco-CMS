import { defineElement } from '@umbraco-ui/uui-base/lib/registration';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';

import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '../core/modal';
import { UmbNotificationService, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN } from '../core/notification';
import { UmbUserStore, UMB_USER_STORE_CONTEXT_TOKEN } from './users/users/user.store';
import { UmbUserGroupStore, UMB_USER_GROUP_STORE_CONTEXT_TOKEN } from './users/user-groups/user-group.store';
import { UmbCurrentUserStore, UMB_CURRENT_USER_STORE_CONTEXT_TOKEN } from './users/current-user/current-user.store';
import {
	UmbCurrentUserHistoryStore,
	UMB_CURRENT_USER_HISTORY_STORE_CONTEXT_TOKEN,
} from './users/current-user/current-user-history.store';

import {
	UmbDocumentTypeStore,
	UMB_DOCUMENT_TYPE_STORE_CONTEXT_TOKEN,
} from './documents/document-types/document-type.store';
import { UmbMediaTypeStore, UMB_MEDIA_TYPE_STORE_CONTEXT_TOKEN } from './media/media-types/media-type.store';
import { UmbMemberTypeStore, UMB_MEMBER_TYPE_STORE_CONTEXT_TOKEN } from './members/member-types/member-type.store';
import { UmbDocumentStore, UMB_DOCUMENT_DETAIL_STORE_CONTEXT_TOKEN } from './documents/documents/document.detail.store';
import { UmbMediaStore, UMB_MEDIA_STORE_CONTEXT_TOKEN } from './media/media/media.store';
import { UmbMemberGroupStore, UMB_MEMBER_GROUP_STORE_CONTEXT_TOKEN } from './members/member-groups/member-group.store';
import { UmbDictionaryStore, UMB_DICTIONARY_STORE_CONTEXT_TOKEN } from './translation/dictionary/dictionary.store';
import {
	UmbDocumentBlueprintStore,
	UMB_DOCUMENT_BLUEPRINT_STORE_CONTEXT_TOKEN,
} from './documents/document-blueprints/document-blueprint.store';

import { UmbSectionStore, UMB_SECTION_STORE_CONTEXT_TOKEN } from './shared/components/section/section.store';
import { UmbDataTypeStore, UMB_DATA_TYPE_STORE_CONTEXT_TOKEN } from './settings/data-types/data-type.store';
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

		this.provideContext(UMB_MODAL_SERVICE_CONTEXT_TOKEN, new UmbModalService());
		this.provideContext(UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN, new UmbNotificationService());

		// TODO: find a way this is possible outside this element. It needs to be possible to register stores in extensions
		this.provideContext(UMB_CURRENT_USER_STORE_CONTEXT_TOKEN, new UmbCurrentUserStore());

		new UmbDocumentStore(this);

		this.provideContext(UMB_MEDIA_STORE_CONTEXT_TOKEN, new UmbMediaStore(this));
		this.provideContext(UMB_DATA_TYPE_STORE_CONTEXT_TOKEN, new UmbDataTypeStore(this));
		this.provideContext(UMB_DOCUMENT_TYPE_STORE_CONTEXT_TOKEN, new UmbDocumentTypeStore(this));
		this.provideContext(UMB_MEDIA_TYPE_STORE_CONTEXT_TOKEN, new UmbMediaTypeStore(this));
		this.provideContext(UMB_MEMBER_TYPE_STORE_CONTEXT_TOKEN, new UmbMemberTypeStore(this));
		this.provideContext(UMB_USER_STORE_CONTEXT_TOKEN, new UmbUserStore(this));
		this.provideContext(UMB_USER_GROUP_STORE_CONTEXT_TOKEN, new UmbUserGroupStore(this));
		this.provideContext(UMB_MEMBER_GROUP_STORE_CONTEXT_TOKEN, new UmbMemberGroupStore(this));
		this.provideContext(UMB_SECTION_STORE_CONTEXT_TOKEN, new UmbSectionStore());
		this.provideContext(UMB_CURRENT_USER_HISTORY_STORE_CONTEXT_TOKEN, new UmbCurrentUserHistoryStore());
		this.provideContext(UMB_DICTIONARY_STORE_CONTEXT_TOKEN, new UmbDictionaryStore(this));
		this.provideContext(UMB_DOCUMENT_BLUEPRINT_STORE_CONTEXT_TOKEN, new UmbDocumentBlueprintStore(this));
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
