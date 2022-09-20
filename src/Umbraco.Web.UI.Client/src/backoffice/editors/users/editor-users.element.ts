import { UUIButtonState, UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { distinctUntilChanged, Subscription } from 'rxjs';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '../../../core/context';
import { UmbNotificationService } from '../../../core/services/notification';
import { UmbDocumentTypeStore } from '../../../core/stores/document-type.store';
import { DocumentTypeEntity } from '../../../mocks/data/document-type.data';
import { UmbDocumentTypeContext } from './document-type.context';

import '../shared/editor-entity-layout/editor-entity-layout.element';

// Lazy load
// TODO: Make this dynamic, use load-extensions method to loop over extensions for this node.
import './views/user-groups/editor-view-user-groups.element';
import { UmbNotificationDefaultData } from '../../../core/services/notification/layouts/default';

@customElement('umb-editor-users')
export class UmbEditorUsersElement extends UmbContextProviderMixin(UmbContextConsumerMixin(LitElement)) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			#name {
				width: 100%;
			}

			#alias {
				padding: 0 var(--uui-size-space-3);
			}
		`,
	];

	render() {
		return html` <umb-editor-entity-layout alias="Umb.Editor.Users"></umb-editor-entity-layout> `;
	}
}

export default UmbEditorUsersElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-users': UmbEditorUsersElement;
	}
}
