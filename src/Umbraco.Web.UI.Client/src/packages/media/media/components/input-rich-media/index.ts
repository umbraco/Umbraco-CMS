import type { UmbMediaCardItemModel } from '../../modals/types.js';

export * from './input-rich-media.element.js';

export interface UmbRichMediaItemModel extends UmbMediaCardItemModel {
	src: string;
}
