//TODO: we need to figure out what components should be available for extensions and load them upfront
import './workspaces/shared/workspace-entity-layout/workspace-entity-layout.element';
import './components/ref-property-editor-ui/ref-property-editor-ui.element';
import './components/backoffice-frame/backoffice-header.element';
import './components/backoffice-frame/backoffice-main.element';
import './components/backoffice-frame/backoffice-modal-container.element';
import './components/backoffice-frame/backoffice-notification-container.element';
import './components/content-property/content-property.element';
import './components/table/table.element';
import './components/shared/code-block.element';
import './components/extension-slot/extension-slot.element';
import './sections/shared/section-main/section-main.element';
import './sections/shared/section-sidebar/section-sidebar.element';
import './sections/shared/section.element';
import './trees/shared/tree-base.element';
import './trees/shared/tree.element';

import { defineElement } from '@umbraco-ui/uui-base/lib/registration';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';

import { UmbModalService } from '../core/services/modal';
import { UmbNotificationService } from '../core/services/notification';
import { UmbDataTypesStore } from '../core/stores/data-types/data-types.store';
import { UmbDocumentTypeStore } from '../core/stores/document-type/document-type.store';
import { UmbMediaTypeStore } from '../core/stores/media-type/media-type.store';
import { UmbMemberTypeStore } from '../core/stores/member-type/member-type.store';
import { UmbDocumentStore } from '../core/stores/document/document.store';
import { UmbMediaStore } from '../core/stores/media/media.store';
import { UmbMemberGroupStore } from '../core/stores/member-group/member-group.store';
import { UmbSectionStore } from '../core/stores/section.store';
import { UmbUserStore } from '../core/stores/user/user.store';
import { UmbIconStore } from '../core/stores/icon/icon.store';
import { UmbUserGroupStore } from '../core/stores/user/user-group.store';
import { UmbCurrentUserHistoryStore } from '../core/stores/current-user-history/current-user-history.store';
import { manifests as sectionManifests } from './sections/manifests';
import { manifests as propertyEditorModelManifests } from './property-editor-models/manifests';
import { manifests as propertyEditorUIManifests } from './property-editor-uis/manifests';
import { manifests as treeManifests } from './trees/manifests';
import { manifests as editorManifests } from './workspaces/manifests';
import { manifests as propertyActionManifests } from './property-actions/manifests';
import { manifests as externalLoginProviderManifests } from './external-login-providers/manifests';
import { manifests as userDashboards } from './user-dashboards/manifests';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import type { ManifestTypes } from '@umbraco-cms/models';

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

		this._registerExtensions(sectionManifests);
		this._registerExtensions(treeManifests);
		this._registerExtensions(editorManifests);
		this._registerExtensions(propertyEditorModelManifests);
		this._registerExtensions(propertyEditorUIManifests);
		this._registerExtensions(propertyActionManifests);
		this._registerExtensions(externalLoginProviderManifests);
		this._registerExtensions(userDashboards);

		this._umbIconRegistry.attach(this);

		this.provideContext('umbDocumentStore', new UmbDocumentStore());
		this.provideContext('umbMediaStore', new UmbMediaStore());
		this.provideContext('umbDataTypeStore', new UmbDataTypesStore());
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
