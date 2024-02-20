// import { UMB_COMPOSITION_PICKER_MODAL, type UmbCompositionPickerModalData } from '../../../modals/index.js';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-member-workspace-view-member')
export class UmbMemberWorkspaceViewMemberElement extends UmbLitElement implements UmbWorkspaceViewElement {
	render() {
		return html`member`;
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbMemberWorkspaceViewMemberElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-workspace-view-member': UmbMemberWorkspaceViewMemberElement;
	}
}
