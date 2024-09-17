import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-default-workspace')
export class UmbDefaultWorkspaceElement extends UmbLitElement {
	override render() {
		return html`This is a default workspace`;
	}

	static override styles = [UmbTextStyles, css``];
}

export { UmbDefaultWorkspaceElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-workspace': UmbDefaultWorkspaceElement;
	}
}
