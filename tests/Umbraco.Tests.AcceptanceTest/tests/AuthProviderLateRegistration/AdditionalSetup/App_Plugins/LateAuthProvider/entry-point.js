// Test fixture: an appEntryPoint that registers an external auth provider LATE — i.e. after
// an async delay inside onInit — mirroring a real provider (e.g. Umbraco ID) whose onInit
// registers its authProvider after fetching/initialising on a slow connection.
//
// The backoffice boot must wait for app-entry-points to settle before deciding which login
// provider to use. If it doesn't, the login screen renders before this provider is registered
// and the late provider never appears (the v17.4+ regression). The delay makes that race
// deterministic.

const LATE_REGISTRATION_DELAY_MS = 1500;

export const onInit = async (_host, extensionRegistry) => {
	await new Promise((resolve) => setTimeout(resolve, LATE_REGISTRATION_DELAY_MS));

	extensionRegistry.register({
		type: 'authProvider',
		alias: 'Test.LateAuthProvider',
		name: 'Late External Login',
		forProviderName: 'Umbraco.LateTest',
		meta: {
			label: 'Late External Login',
			defaultView: {
				icon: 'icon-cloud',
			},
			behavior: {
				autoRedirect: false,
			},
		},
	});
};
