import type { UmbCropModel, UmbFocalPointModel } from '../property-editors/types.js';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

/**
 * A single media reference with crop/focal-point fidelity, as stored on a `richMedia` clipboard entry.
 */
export interface UmbRichMediaClipboardEntryItemModel extends UmbReferenceByUnique {
	focalPoint: UmbFocalPointModel | null;
	crops: Array<UmbCropModel>;
}

/**
 * The value stored on a `richMedia` clipboard entry: media references with crops and focal point.
 */
export type UmbRichMediaClipboardEntryValueModel = Array<UmbRichMediaClipboardEntryItemModel>;

/**
 * The value stored on a `media` clipboard entry: bare media references, without crops or focal point.
 */
export type UmbMediaClipboardEntryValueModel = Array<UmbReferenceByUnique>;
