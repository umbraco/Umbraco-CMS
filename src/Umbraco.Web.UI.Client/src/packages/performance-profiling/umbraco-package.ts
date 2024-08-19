export const name = 'Umbraco.Core.PerformanceProfiling';
export const extensions = [
	{
		name: 'Performance Profiling Bundle',
		alias: 'Umb.Bundle.PerformanceProfiling',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
