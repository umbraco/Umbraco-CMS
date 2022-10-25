import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import type { ManifestSectionView, ManifestWithLoader } from '@umbraco-cms/models';

@customElement('umb-section-users')
export class UmbSectionUsersElement extends LitElement {
	constructor() {
		super();

		this._registerSectionViews();
	}

	private _registerSectionViews() {
		const manifests: Array<ManifestWithLoader<ManifestSectionView>> = [
			{
				type: 'sectionView',
				alias: 'Umb.SectionView.Users.Users',
				name: 'Users Section View',
				loader: () => import('./views/users/section-view-users.element'),
				meta: {
					sections: ['Umb.Section.Users'],
					label: 'Users',
					pathname: 'users',
					weight: 200,
					icon: 'umb:user',
				},
			},
			{
				type: 'sectionView',
				alias: 'Umb.SectionView.Users.UserGroups',
				name: 'User Groups Section View',
				loader: () => import('./views/user-groups/section-view-user-groups.element'),
				meta: {
					sections: ['Umb.Section.Users'],
					label: 'User Groups',
					pathname: 'user-groups',
					weight: 100,
					icon: 'umb:users',
				},
			},
		];

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
