import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import type { ManifestSectionView } from '@umbraco-cms/models';

@customElement('umb-section-packages')
export class UmbSectionPackages extends UmbContextConsumerMixin(LitElement) {
	constructor() {
		super();

		this._registerSectionViews();
	}

	private _registerSectionViews() {
		const manifests: Array<ManifestSectionView> = [
			{
				type: 'sectionView',
				alias: 'Umb.SectionView.Packages.Repo',
				name: 'Packages Repo Section View',
				loader: () => import('./views/repo/section-view-packages-repo.element'),
				meta: {
					sections: ['Umb.Section.Packages'],
					label: 'Packages',
					pathname: 'packages',
					weight: 300,
					icon: 'umb:cloud',
				},
			},
			{
				type: 'sectionView',
				alias: 'Umb.SectionView.Packages.Installed',
				name: 'Installed Packages Section View',
				loader: () => import('./views/installed/section-view-packages-installed.element'),
				meta: {
					sections: ['Umb.Section.Packages'],
					label: 'Installed',
					pathname: 'installed',
					weight: 200,
					icon: 'umb:box',
				},
			},
			{
				type: 'sectionView',
				alias: 'Umb.SectionView.Packages.Builder',
				name: 'Packages Builder Section View',
				loader: () => import('./views/created/section-view-packages-created.element'),
				meta: {
					sections: ['Umb.Section.Packages'],
					label: 'Created',
					pathname: 'created',
					weight: 100,
					icon: 'umb:files',
				},
			},
		];

		manifests.forEach((manifest) => {
			if (umbExtensionsRegistry.isRegistered(manifest.alias)) return;
			umbExtensionsRegistry.register(manifest);
		});
	}

	render() {
		return html`<umb-section></umb-section>`;
	}
}

export default UmbSectionPackages;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-packages': UmbSectionPackages;
	}
}
