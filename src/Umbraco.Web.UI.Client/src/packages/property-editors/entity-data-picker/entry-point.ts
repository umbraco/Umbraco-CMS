import { manifests as entityDataPickerManifests } from './manifests.js';
import type { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';
import { UMB_PICKER_DATA_SOURCE_TYPE } from '@umbraco-cms/backoffice/picker-data-source';
import type { ManifestPropertyEditorDataSource } from '@umbraco-cms/backoffice/property-editor-data-source';

// import of local component
import './input/input-entity-data.element.js';

export const onInit: UmbEntryPointOnInit = (host, extensionRegistry) => {
	let initialized = false;

	/* We register the Entity Data Picker only if any picker data sources have been registered.
  This prevents an unusable Property Editor UI from appearing in the list of Property Editors out of the box.
  We can remove this code when data sources become more common. */
	extensionRegistry
		.byTypeAndFilter<'propertyEditorDataSource', ManifestPropertyEditorDataSource>(
			'propertyEditorDataSource',
			(manifest) => manifest.dataSourceType === UMB_PICKER_DATA_SOURCE_TYPE,
		)
		.subscribe((pickerPropertyEditorDataSource) => {
			if (pickerPropertyEditorDataSource.length > 0 && !initialized) {
				extensionRegistry.registerMany(entityDataPickerManifests);
				initialized = true;
			}
		});
};
