import type { UmbMediaCardItemModel } from '../../modals/index.js';
import type { UmbMediaPickerPropertyValue } from '../../property-editors/index.js';

export * from './input-rich-media.element.js';

export interface UmbRichMediaItemModel extends UmbMediaCardItemModel, UmbMediaPickerPropertyValue {
	src: string;
}
