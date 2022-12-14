//TODO: we need to figure out what components should be available for extensions and load them upfront
import './editors/shared/editor-entity-layout/editor-entity-layout.element';
import './components/ref-property-editor-ui/ref-property-editor-ui.element';
import './components/backoffice-frame/backoffice-header.element';
import './components/backoffice-frame/backoffice-main.element';
import './components/backoffice-frame/backoffice-modal-container.element';
import './components/backoffice-frame/backoffice-notification-container.element';
import './components/node-property/node-property.element';
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
import { UmbDataTypeStore } from '../core/stores/data-type/data-type.store';
import { UmbDocumentTypeStore } from '../core/stores/document-type.store';
import { UmbNodeStore } from '../core/stores/node.store';
import { UmbSectionStore } from '../core/stores/section.store';
import { UmbEntityStore } from '../core/stores/entity.store';
import { UmbUserStore } from '../core/stores/user/user.store';
import { UmbIconStore } from '../core/stores/icon/icon.store';
import { UmbUserGroupStore } from '../core/stores/user/user-group.store';
import { manifests as sectionManifests } from './sections/manifests';
import { manifests as propertyEditorModelManifests } from './property-editor-models/manifests';
import { manifests as propertyEditorUIManifests } from './property-editor-uis/manifests';
import { manifests as treeManifests } from './trees/manifests';
import { manifests as editorManifests } from './editors/manifests';
import { manifests as propertyActionManifests } from './property-actions/manifests';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import type { ManifestTypes, ManifestWithLoader } from '@umbraco-cms/models';

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
	private _umbEntityStore = new UmbEntityStore();

	constructor() {
		super();

		this._registerExtensions(sectionManifests);
		this._registerExtensions(treeManifests);
		this._registerExtensions(editorManifests);
		this._registerExtensions(propertyEditorModelManifests);
		this._registerExtensions(propertyEditorUIManifests);
		this._registerExtensions(propertyActionManifests);

		this._umbIconRegistry.attach(this);

		this.provideContext('umbEntityStore', this._umbEntityStore);
		this.provideContext('umbNodeStore', new UmbNodeStore(this._umbEntityStore));
		this.provideContext('umbDataTypeStore', new UmbDataTypeStore(this._umbEntityStore));
		this.provideContext('umbDocumentTypeStore', new UmbDocumentTypeStore(this._umbEntityStore));
		this.provideContext('umbUserStore', new UmbUserStore(this._umbEntityStore));
		this.provideContext('umbUserGroupStore', new UmbUserGroupStore(this._umbEntityStore));
		this.provideContext('umbNotificationService', new UmbNotificationService());
		this.provideContext('umbModalService', new UmbModalService());
		this.provideContext('umbSectionStore', new UmbSectionStore());
	}

	private _registerExtensions(manifests: Array<ManifestWithLoader<ManifestTypes>> | Array<ManifestTypes>) {
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
