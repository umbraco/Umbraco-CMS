import { UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../property-editors/block-list-editor/constants.js';
import type { UmbBlockListLayoutModel, UmbBlockListValueModel } from '../../types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbClipboardEntryDetailRepository, type UmbClipboardCopyResolver } from '@umbraco-cms/backoffice/clipboard';

export class UmbBlockListClipboardCopyResolver extends UmbControllerBase implements UmbClipboardCopyResolver {
	#entryType = 'block';
	#clipboardDetailRepository = new UmbClipboardEntryDetailRepository(this);

	async copy(propertyValue: UmbBlockListValueModel, name: string, meta: Record<string, unknown>) {
		const entryValue = this.#constructEntryValue(propertyValue);

		// TODO: Add correct meta data
		const { data } = await this.#clipboardDetailRepository.createScaffold({
			type: this.#entryType,
			name: name,
			meta: meta,
			value: entryValue,
		});

		if (data) {
			await this.#clipboardDetailRepository.create(data);
		}
	}

	#constructEntryValue(propertyValue: UmbBlockListValueModel) {
		const contentData = structuredClone(propertyValue.contentData);
		const layout = propertyValue.layout?.[UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS]
			? structuredClone(propertyValue.layout?.[UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS])
			: undefined;
		const settingsData = structuredClone(propertyValue.settingsData);
		const expose = structuredClone(propertyValue.expose);

		layout?.forEach((layoutItem: UmbBlockListLayoutModel) => {
			// @ts-expect-error - We are removing the $type property from the layout item
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
