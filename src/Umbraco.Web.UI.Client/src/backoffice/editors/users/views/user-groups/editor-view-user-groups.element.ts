import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../../../core/context';
import type { DocumentTypeEntity } from '../../../../../mocks/data/document-type.data';
import { Subscription, distinctUntilChanged } from 'rxjs';
import { UmbDocumentTypeContext } from '../../document-type.context';

@customElement('umb-editor-view-user-groups')
export class UmbEditorViewUserGroupsElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	render() {
		return html`<div>USER GROUPS</div>`;
	}
}

export default UmbEditorViewUserGroupsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-user-groups': UmbEditorViewUserGroupsElement;
	}
}
