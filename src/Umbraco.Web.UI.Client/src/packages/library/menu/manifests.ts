import { UMB_LIBRARY_MENU_ALIAS } from './constants.js';
import type { ManifestMenu } from '@umbraco-cms/backoffice/menu';

const menu: ManifestMenu = {
	type: 'menu',
	alias: UMB_LIBRARY_MENU_ALIAS,
	name: 'Library Menu',
};

export const manifests: Array<UmbExtensionManifest> = [menu];
