export const name = 'Umbraco.Core.Telemetry';
export const extensions = [
	{
		name: 'Telemetry Bundle',
		alias: 'Umb.Bundle.Telemetry',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
