import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import type { ManifestDashboardCollection } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-dashboard-collection')
export class UmbDashboardCollectionElement extends UmbLitElement {
	public manifest!: ManifestDashboardCollection;

	// TODO: figure out what collection to render
	render() {
		return html`<umb-collection></umb-collection>`;
	}

	static styles = [
		css`
			:host {
				display: flex;
				flex-direction: column;
				box-sizing: border-box;
				gap: var(--uui-size-space-5);
				height: 100%;
			}
		`,
	];
}

export default UmbDashboardCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-collection': UmbDashboardCollectionElement;
	}
}
