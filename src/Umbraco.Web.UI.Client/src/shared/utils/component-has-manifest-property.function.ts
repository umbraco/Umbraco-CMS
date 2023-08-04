import { ManifestBase } from '@umbraco-cms/backoffice/extension-api';

export function componentHasManifestProperty(
	component: HTMLElement
): component is HTMLElement & { manifest: ManifestBase } {
	return component ? 'manifest' in component : false;
}
