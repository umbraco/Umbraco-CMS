import { UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../property-editors/constants.js';
import type { UmbBlockGridLayoutModel, UmbBlockGridValueModel } from '../../types.js';
import { UmbPropertyValueCloneController } from '@umbraco-cms/backoffice/property';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbClipboardEntryDetailRepository, type UmbClipboardPasteResolver } from '@umbraco-cms/backoffice/clipboard';
import type { UmbBlockLayoutBaseModel } from '@umbraco-cms/backoffice/block';

export class UmbBlockListClipboardPasteResolver extends UmbControllerBase implements UmbClipboardPasteResolver {
	#detailRepository = new UmbClipboardEntryDetailRepository(this);

	async getAcceptedTypes(): Promise<string[]> {
		return ['block'];
	}

	async resolve(unique: string) {
		const { data: entry } = await this.#detailRepository.requestByUnique(unique);

		if (!entry) {
			throw new Error(`Could not find clipboard entry with unique id: ${unique}`);
		}

		const acceptedTypes = await this.getAcceptedTypes();

		if (acceptedTypes.includes(entry.type) === false) {
			throw new Error(`Clipboard entry type "${entry.type}" is not supported by this resolver.`);
		}

		let propertyValue = undefined;

		if (entry.type === 'block') {
			propertyValue = await this.#transformBlockValue(entry.value);
		}

		const cloner = new UmbPropertyValueCloneController(this);
		const clonedValue = await cloner.clone({
			editorAlias: UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS,
			value: propertyValue,
		});

		propertyValue = clonedValue.value;

		return propertyValue;
	}

	async #transformBlockValue(value: any) {
		if (!value) {
			throw new Error('Clipboard entry value is missing.');
		}

		const clone = structuredClone(value);

		const propertyValue: UmbBlockGridValueModel = {
			contentData: clone.contentData,
			settingsData: clone.settingsData,
			expose: clone.expose,
			layout: {
				[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS]: clone.layout.map((baseLayout: UmbBlockLayoutBaseModel) => {
					const gridLayout: UmbBlockGridLayoutModel = {
						...baseLayout,
						columnSpan: 12,
						rowSpan: 1,
						areas: [],
					};

					return gridLayout;
				}),
			},
		};

		return propertyValue;
	}
}

export { UmbBlockListClipboardPasteResolver as api };
