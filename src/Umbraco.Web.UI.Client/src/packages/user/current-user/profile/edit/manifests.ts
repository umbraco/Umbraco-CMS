const currentUserAction: UmbExtensionManifest = {
	type: 'currentUserAction',
	kind: 'default',
	alias: 'Umb.CurrentUser.Button.Edit',
	name: 'Current User Edit Profile Action',
	weight: 1000,
	api: () => import('./edit-profile.action.js'),
	meta: {
		label: '#general_edit',
		icon: 'edit',
	},
};

const modal: UmbExtensionManifest = {
	type: 'modal',
	alias: 'Umb.Modal.CurrentUserEditProfile',
	name: 'Current User Edit Profile Modal',
	element: () => import('./edit-profile-modal.element.js'),
};

export const manifests: Array<UmbExtensionManifest> = [currentUserAction, modal];
