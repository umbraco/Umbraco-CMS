import type { ManifestWorkspaceDefaultKind } from './types.js';
import { customElement, html, ifDefined, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-default-workspace')
export class UmbDefaultWorkspaceElement extends UmbLitElement {
	@property({ type: Object, attribute: false })
	manifest?: ManifestWorkspaceDefaultKind;

	override render() {
		const headline = this.manifest?.meta.headline;
		return html`<umb-body-layout headline=${ifDefined(headline ? this.localize.string(headline) : undefined)}>
		</umb-body-layout>`;
	}

	static override styles = [UmbTextStyles];
}

export { UmbDefaultWorkspaceElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-workspace': UmbDefaultWorkspaceElement;
	}
}
