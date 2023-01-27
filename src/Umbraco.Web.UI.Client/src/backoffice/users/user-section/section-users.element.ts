import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-api';
import type { ManifestSectionView } from '@umbraco-cms/models';

@customElement('umb-users-section')
export class UmbUsersSectionElement extends LitElement {
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

export default UmbUsersSectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-users-section': UmbUsersSectionElement;
	}
}
