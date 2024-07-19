import { html, customElement, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-current-user-profile-user-profile-app')
export class UmbCurrentUserProfileUserProfileAppElement extends UmbLitElement {
	override render() {
		return html`
			<uui-box .headline=${this.localize.term('user_yourProfile')}>
				<umb-extension-with-api-slot id="actions" type="currentUserAction"></umb-extension-with-api-slot>
			</uui-box>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#actions {
				display: flex;
				flex-wrap: wrap;
				flex-direction: row;
				gap: var(--uui-size-space-2);
			}
		`,
	];
}

export default UmbCurrentUserProfileUserProfileAppElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-profile-user-profile-app': UmbCurrentUserProfileUserProfileAppElement;
	}
}
