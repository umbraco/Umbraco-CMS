export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Example.EntityAction.Hello',
		name: 'Example Hello Entity Action',
		forEntityTypes: ['example'],
		api: () => import('./hello.action.js'),
		meta: {
			icon: 'icon-handshake',
			label: 'Say Hello',
		},
	},
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Example.EntityAction.Goodbye',
		name: 'Example Goodbye Entity Action',
		forEntityTypes: ['example'],
		api: () => import('./goodbye.action.js'),
		meta: {
			icon: 'icon-chat',
			label: 'Say Goodbye',
		},
	},
];
