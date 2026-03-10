import { onInit as entityDataPickerOnInit } from './entity-data-picker/entry-point.js';
import type { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';
import './checkbox-list/components/index.js';
import './content-picker/components/index.js';

export const onInit: UmbEntryPointOnInit = (host, extensionRegistry) => {
	// We do not have a package for the Entity Data Picker, so we proxy the init call here
	entityDataPickerOnInit(host, extensionRegistry);
};
