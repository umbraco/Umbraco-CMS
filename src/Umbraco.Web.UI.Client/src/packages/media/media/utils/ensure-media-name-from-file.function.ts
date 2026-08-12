import { UMB_MEDIA_ENTITY_TYPE } from '../entity.js';
import { toFriendlyName } from './to-friendly-name.function.js';
import type { UmbClassInterface } from '@umbraco-cms/backoffice/class-api';
import { UMB_NAMEABLE_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';

/**
 * Populates the media workspace name from an uploaded file when the user has not
 * already provided one. The filename is converted to a friendly name (extension
 * stripped, dashes/underscores replaced, title-cased) before being applied.
 *
 * Does nothing if the consumed dataset context is not a media item, or if a
 * non-empty name is already set.
 * @param {UmbClassInterface} host - The context-consuming host (typically the calling element).
 * @param {File} file - The uploaded file whose name should seed the media item name.
 */
export async function ensureMediaNameFromFile(host: UmbClassInterface, file: File): Promise<void> {
	const datasetContext = await host.getContext(UMB_NAMEABLE_PROPERTY_DATASET_CONTEXT).catch(() => undefined);
	if (!datasetContext || datasetContext.getEntityType() !== UMB_MEDIA_ENTITY_TYPE) return;

	const currentName = datasetContext.getName();
	if (currentName && currentName.trim() !== '') return;

	datasetContext.setName(toFriendlyName(file.name));
}
