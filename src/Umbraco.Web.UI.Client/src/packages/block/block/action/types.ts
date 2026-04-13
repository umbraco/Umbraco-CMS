export type * from './block-action-element.interface.js';
export type * from './block-action.extension.js';
export type * from './block-action.interface.js';
export type * from './default/types.js';

export interface UmbBlockActionArgs<MetaArgsType> {
	unique: string;
	meta: MetaArgsType;
}
