import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import type { ManifestSectionView } from '@umbraco-cms/models';

@customElement('umb-section-users')
export class UmbSectionUsersElement extends LitElement {
	constructor() {
		super();

		this._registerSectionViews();
	}

	private _registerSectionViews() {
		const manifests: Array<ManifestSectionView> = [];

		manifests.forEach((manifest) => {
			if (umbExtensionsRegistry.isRegistered(manifest.alias)) return;
			umbExtensionsRegistry.register(manifest);
		});
	}

	render() {
		return html` <umb-section></umb-section> `;
	}
}

export default UmbSectionUsersElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-users': UmbSectionUsersElement;
	}
}
