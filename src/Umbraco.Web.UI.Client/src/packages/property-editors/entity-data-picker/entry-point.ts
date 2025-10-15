import { manifests as entityDataPickerManifests } from './manifests.js';
import type { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';
import type { ManifestPropertyEditorDataSource } from '@umbraco-cms/backoffice/property-editor-data-source';

export const onInit: UmbEntryPointOnInit = (host, extensionRegistry) => {
	let initialized = false;

	/* We register the Entity Data Picker only if any picker data sources have been registered. 
  This prevents an unusable Property Editor UI from appearing in the list of Property Editors out of the box. 
  We can remove this code when data sources become more common. */
	extensionRegistry
		.byTypeAndFilter<'propertyEditorDataSource', ManifestPropertyEditorDataSource>(
			'propertyEditorDataSource',
			(manifest) => manifest.dataSourceType === 'picker',
		)
		.subscribe((pickerPropertyEditorDataSource) => {
			if (pickerPropertyEditorDataSource.length > 0 && !initialized) {
				extensionRegistry.registerMany(entityDataPickerManifests);
				initialized = true;
			}
		});
};
