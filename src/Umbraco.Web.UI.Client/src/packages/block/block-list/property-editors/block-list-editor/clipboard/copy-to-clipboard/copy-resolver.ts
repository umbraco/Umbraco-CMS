import { UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../constants.js';
import type { UmbBlockListLayoutModel } from '../../../../types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardCopyResolver } from '@umbraco-cms/backoffice/clipboard';

export class UmbBlockListClipboardCopyResolver extends UmbControllerBase implements UmbClipboardCopyResolver {
	resolve(propertyValue: unknown): Promise<unknown> {
		const contentData = structuredClone(propertyValue.contentData);
		const layout = propertyValue.layout?.[UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS]
			? structuredClone(propertyValue.layout?.[UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS])
			: undefined;
		const settingsData = structuredClone(propertyValue.settings);
		const expose = structuredClone(propertyValue.expose);

		layout.forEach((layoutItem: UmbBlockListLayoutModel) => {
			delete layoutItem.$type;
		});

		const entryValue = {
			contentData: contentData ?? [],
			layout: layout ?? [],
			settingsData: settingsData ?? [],
			expose: expose ?? [],
		};

		return entryValue;
	}
}

export { UmbBlockListClipboardCopyResolver as api };
