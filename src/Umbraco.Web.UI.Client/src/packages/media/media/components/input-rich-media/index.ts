import type { UmbMediaCardItemModel } from '../../modals/index.js';

export * from './input-rich-media.element.js';

export interface UmbRichMediaItemModel extends UmbMediaCardItemModel {
	src: string;
}
