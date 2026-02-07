import type { UmbBlockLayoutBaseModel } from '../types.js';

export interface UmbBlockInsertedEventDetail<
	TLayoutModel extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel,
	TOriginData = unknown,
> {
	originData: TOriginData;
	layout: TLayoutModel;
}

export class UmbBlockInsertedEvent<
	TLayoutModel extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel,
	TOriginData = unknown,
> extends CustomEvent<UmbBlockInsertedEventDetail<TLayoutModel, TOriginData>> {
	static readonly TYPE = 'umb-internal:blockInserted';

	constructor(detail: UmbBlockInsertedEventDetail<TLayoutModel, TOriginData>) {
		super(UmbBlockInsertedEvent.TYPE, { detail, bubbles: false, composed: false, cancelable: false });
	}
}

declare global {
	interface GlobalEventHandlersEventMap {
		[UmbBlockInsertedEvent.TYPE]: UmbBlockInsertedEvent;
	}
}
