import { manifest as defaultSectionManifest } from './default/default.section.kind.js';
import { manifests as sectionUserPermissionConditionManifests } from './conditions/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.SectionPicker',
		name: 'Section Picker Modal',
		element: () => import('./section-picker-modal/section-picker-modal.element.js'),
	},
	defaultSectionManifest,
	...sectionUserPermissionConditionManifests,
	...repositoryManifests,
];
