export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'globalContext',
		alias: 'Umb.GlobalContext.ManagementApi.ServerEvent',
		name: 'Management Api Server Event Global Context',
		api: () => import('./server-event.context.js'),
	},
];
