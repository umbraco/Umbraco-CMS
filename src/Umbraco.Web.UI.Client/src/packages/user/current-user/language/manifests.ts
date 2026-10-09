export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'userProfileApp',
		alias: 'Umb.UserProfileApp.CurrentUser.Language',
		name: 'Current User Language User Profile App',
		element: () => import('./current-user-language-user-profile-app.element.js'),
		weight: 300,
		meta: {
			label: 'Current User Language User Profile App',
			pathname: 'language',
		},
	},
];
