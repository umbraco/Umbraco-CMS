import { defineElement } from '@umbraco-ui/uui-base/lib/registration';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';

import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '../core/modal';
import { UmbNotificationService, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN } from '../core/notification';
import { UmbUserStore } from './users/users/user.store';
import { UmbUserGroupStore } from './users/user-groups/user-group.store';
import { UmbCurrentUserStore, UMB_CURRENT_USER_STORE_CONTEXT_TOKEN } from './users/current-user/current-user.store';
import {
	UmbCurrentUserHistoryStore,
	UMB_CURRENT_USER_HISTORY_STORE_CONTEXT_TOKEN,
} from './users/current-user/current-user-history.store';

import {UmbDocumentTypeDetailStore} from './documents/document-types/document-type.detail.store';
import {UmbDocumentTypeTreeStore} from './documents/document-types/document-type.tree.store';
import { UmbMediaTypeDetailStore } from './media/media-types/media-type.detail.store';
import { UmbMediaTypeTreeStore } from './media/media-types/media-type.tree.store';
import { UmbDocumentDetailStore } from './documents/documents/document.detail.store';
import { UmbDocumentTreeStore } from './documents/documents/document.tree.store';
import { UmbMediaDetailStore } from './media/media/media.detail.store';
import { UmbMediaTreeStore } from './media/media/media.tree.store';
import { UmbMemberTypeDetailStore } from './members/member-types/member-type.detail.store';
import { UmbMemberTypeTreeStore } from './members/member-types/member-type.tree.store';
import { UmbMemberGroupStore } from './members/member-groups/member-group.details.store';
import { UmbDictionaryDetailStore } from './translation/dictionary/dictionary.detail.store';
import { UmbDictionaryTreeStore } from './translation/dictionary/dictionary.tree.store';
import { UmbDocumentBlueprintDetailStore } from './documents/document-blueprints/document-blueprint.detail.store';
import { UmbDocumentBlueprintTreeStore } from './documents/document-blueprints/document-blueprint.tree.store';

import { UmbSectionStore, UMB_SECTION_STORE_CONTEXT_TOKEN } from './shared/components/section/section.store';
import { UmbDataTypeDetailStore } from './settings/data-types/data-type.detail.store';
import { UmbDataTypeTreeStore } from './settings/data-types/data-type.tree.store';
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

		new UmbDocumentDetailStore(this);
		new UmbDocumentTreeStore(this);
		new UmbMediaDetailStore(this);
		new UmbMediaTreeStore(this);
		new UmbDataTypeDetailStore(this);
		new UmbDataTypeTreeStore(this);
		new UmbUserStore(this);
		new UmbMediaTypeDetailStore(this);
		new UmbMediaTypeTreeStore(this);
		new UmbDocumentTypeDetailStore(this);
		new UmbDocumentTypeTreeStore(this);
		new UmbMemberTypeDetailStore(this);
		new UmbMemberTypeTreeStore(this);
		new UmbUserGroupStore(this);
		new UmbMemberGroupStore(this);
		new UmbDictionaryDetailStore(this);
		new UmbDictionaryTreeStore(this);
		new UmbDocumentBlueprintDetailStore(this);
		new UmbDocumentBlueprintTreeStore(this);

		this.provideContext(UMB_SECTION_STORE_CONTEXT_TOKEN, new UmbSectionStore());
		this.provideContext(UMB_CURRENT_USER_HISTORY_STORE_CONTEXT_TOKEN, new UmbCurrentUserHistoryStore());
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
