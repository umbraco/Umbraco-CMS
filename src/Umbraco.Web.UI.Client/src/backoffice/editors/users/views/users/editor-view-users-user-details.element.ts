import { css, html, LitElement, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../../../core/context';

@customElement('umb-editor-view-users-user-details')
export class UmbEditorViewUsersUserDetailsElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	render() {
		return html`USER DETAILS `;
	}
}

export default UmbEditorViewUsersUserDetailsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-users-user-details': UmbEditorViewUsersUserDetailsElement;
	}
}
