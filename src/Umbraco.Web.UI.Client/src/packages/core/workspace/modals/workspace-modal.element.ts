import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import { css, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import type { UmbWorkspaceModalData } from '@umbraco-cms/backoffice/workspace';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-workspace-modal')
export class UmbWorkspaceModalElement extends UmbLitElement {
	@property({ attribute: false })
	data?: UmbWorkspaceModalData;

	/**
	 * TODO: Consider if this binding and events integration is the right for communicating back the modal handler. Or if we should go with some Context API. like a Modal Context API.
	 *
	 */
	override render() {
		return this.data ? html`<umb-workspace .entityType=${this.data.entityType}></umb-workspace>` : '';
	}

	static override styles: CSSResultGroup = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				height: 100%;
			}
		`,
	];
}

export default UmbWorkspaceModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-modal': UmbWorkspaceModalElement;
	}
}
