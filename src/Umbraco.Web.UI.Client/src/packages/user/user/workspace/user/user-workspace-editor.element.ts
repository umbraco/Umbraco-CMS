import { UMB_USER_ROOT_WORKSPACE_PATH } from '../../paths.js';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-user-workspace-editor')
export class UmbUserWorkspaceEditorElement extends UmbLitElement {
	override render() {
		return html`
			<umb-entity-detail-workspace-editor .backPath=${UMB_USER_ROOT_WORKSPACE_PATH}>
				<umb-workspace-header-name-editable slot="header"></umb-workspace-header-name-editable>
			</umb-entity-detail-workspace-editor>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}
		`,
	];
}

export default UmbUserWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-workspace-editor': UmbUserWorkspaceEditorElement;
	}
}
