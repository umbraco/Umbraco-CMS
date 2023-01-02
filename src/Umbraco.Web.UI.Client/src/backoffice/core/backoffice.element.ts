import { defineElement } from '@umbraco-ui/uui-base/lib/registration';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';

import { manifests as externalLoginProviderManifests } from '../../auth/external-login-providers/manifests';
import { UmbDocumentTypeStore } from '../documents/document-types/document-type.store';
import { UmbMediaTypeStore } from '../media/media-types/media-type.store';
import { UmbMemberTypeStore } from '../members/member-types/member-type.store';
import { UmbDocumentStore } from '../documents/documents/document.store';
import { UmbMediaStore } from '../media/media/media.store';
import { UmbMemberGroupStore } from '../members/member-groups/member-group.store';
import { UmbUserStore } from '../../auth/users/user.store';
import { UmbUserGroupStore } from '../../auth/user-groups/user-group.store';
import { UmbCurrentUserHistoryStore } from '../../auth/current-user/current-user-history.store';
import { UmbDictionaryStore } from '../translation/dictionary/dictionary.store';
import { UmbDocumentBlueprintStore } from '../documents/document-blueprints/document-blueprint.store';

import { manifests as sectionManifests } from '../sections.manifest';
import { manifests as editorManifests } from '../workspaces.manifest';
import { manifests as userDashboards } from '../../auth/current-user/user-dashboards/manifests';
import { manifests as propertyEditorModelManifests } from './property-editors/models/manifests';
import { manifests as propertyEditorUIManifests } from './property-editors/uis/manifests';
import { manifests as propertyActionManifests } from './property-actions/manifests';
import { UmbSectionStore } from './components/section/section.store';
import { UmbDataTypeStore } from './data-types/data-type.store';
import { UmbIconStore } from './stores/icon/icon.store';
import { UmbNotificationService } from './services/notification';
import { UmbModalService } from './services/modal';
import { manifests as collectionBulkActionManifests } from './components/collection/bulk-actions/manifests';
import { manifests as collectionViewManifests } from './components/collection/views/manifests';

import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';

import type { ManifestTypes } from '@umbraco-cms/models';

// Domains
import './components';
import '../core';
import '../documents';
import '../media';
import '../members';
import '../translation';

@defineElement('umb-backoffice')
export class UmbBackofficeElement extends UmbContextConsumerMixin(UmbContextProviderMixin(LitElement)) {
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

	private _umbIconRegistry = new UmbIconStore();

	constructor() {
		super();

		// TODO: this needs to happen in each domain
		this._registerExtensions(sectionManifests);
		this._registerExtensions(editorManifests);
		this._registerExtensions(propertyEditorModelManifests);
		this._registerExtensions(propertyEditorUIManifests);
		this._registerExtensions(propertyActionManifests);
		this._registerExtensions(externalLoginProviderManifests);
		this._registerExtensions(userDashboards);
		this._registerExtensions(collectionBulkActionManifests);
		this._registerExtensions(collectionViewManifests);

		this._umbIconRegistry.attach(this);

		// TODO: find a way this is possible outside this element. It needs to be possible to register stores in extensions
		this.provideContext('umbDocumentStore', new UmbDocumentStore());
		this.provideContext('umbMediaStore', new UmbMediaStore());
		this.provideContext('umbDataTypeStore', new UmbDataTypeStore());
		this.provideContext('umbDocumentTypeStore', new UmbDocumentTypeStore());
		this.provideContext('umbMediaTypeStore', new UmbMediaTypeStore());
		this.provideContext('umbMemberTypeStore', new UmbMemberTypeStore());
		this.provideContext('umbUserStore', new UmbUserStore());
		this.provideContext('umbUserGroupStore', new UmbUserGroupStore());
		this.provideContext('umbMemberGroupStore', new UmbMemberGroupStore());
		this.provideContext('umbNotificationService', new UmbNotificationService());
		this.provideContext('umbModalService', new UmbModalService());
		this.provideContext('umbSectionStore', new UmbSectionStore());
		this.provideContext('umbCurrentUserHistoryStore', new UmbCurrentUserHistoryStore());
		this.provideContext('umbDictionaryStore', new UmbDictionaryStore());
		this.provideContext('umbDocumentBlueprintStore', new UmbDocumentBlueprintStore());
	}

	private _registerExtensions(manifests: Array<ManifestTypes> | Array<ManifestTypes>) {
		manifests.forEach((manifest) => {
			if (umbExtensionsRegistry.isRegistered(manifest.alias)) return;
			umbExtensionsRegistry.register(manifest);
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
