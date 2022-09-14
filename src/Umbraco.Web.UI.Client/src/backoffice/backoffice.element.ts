//TODO: we need to figure out what components should be available for extensions and load them upfront
import './components/ref-property-editor/ref-property-editor.element';
import './components/ref-property-editor-ui/ref-property-editor-ui.element';
import './components/backoffice-header.element';
import './components/backoffice-main.element';
import './components/backoffice-modal-container.element';
import './components/backoffice-notification-container.element';
import './components/editor-property-layout.element';
import './components/node-property.element';
import './sections/shared/section-layout.element';
import './sections/shared/section-main.element';
import './sections/shared/section-sidebar.element';
import './sections/shared/section.element';
import './trees/shared/tree-base.element';
import './trees/shared/tree.element';

import { defineElement } from '@umbraco-ui/uui-base/lib/registration';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import type { Subscription } from 'rxjs';

import { UmbContextConsumerMixin, UmbContextProviderMixin } from '../core/context';
import { UmbModalService } from '../core/services/modal';
import { UmbNotificationService } from '../core/services/notification';
import { UmbDataTypeStore } from '../core/stores/data-type.store';
import { UmbDocumentTypeStore } from '../core/stores/document-type.store';
import { UmbNodeStore } from '../core/stores/node.store';
import { UmbSectionStore } from '../core/stores/section.store';
import { UmbEntityStore } from '../core/stores/entity.store';
import { UmbPropertyEditorStore } from '../core/stores/property-editor.store';

@defineElement('umb-backoffice')
export default class UmbBackoffice extends UmbContextConsumerMixin(UmbContextProviderMixin(LitElement)) {
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

	private _umbSectionStore?: UmbSectionStore;
	private _umbEntityStore?: UmbEntityStore;
	private _currentSectionSubscription?: Subscription;

	constructor() {
		super();

		this._umbEntityStore = new UmbEntityStore();

		this.provideContext('umbEntityStore', this._umbEntityStore);
		this.provideContext('umbNodeStore', new UmbNodeStore(this._umbEntityStore));
		this.provideContext('umbDataTypeStore', new UmbDataTypeStore(this._umbEntityStore));
		this.provideContext('umbDocumentTypeStore', new UmbDocumentTypeStore(this._umbEntityStore));
		this.provideContext('umbPropertyEditorStore', new UmbPropertyEditorStore());
		this.provideContext('umbNotificationService', new UmbNotificationService());
		this.provideContext('umbModalService', new UmbModalService());

		// TODO: how do we want to handle context aware DI?
		this.consumeContext('umbExtensionRegistry', (extensionRegistry) => {
			this._umbSectionStore = new UmbSectionStore(extensionRegistry);
			this.provideContext('umbSectionStore', this._umbSectionStore);
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._currentSectionSubscription?.unsubscribe();
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

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice': UmbBackoffice;
	}
}
