import type { UmbBlockLayoutBaseModel } from '../types.js';

export interface UmbBlockCreatedEventDetail<
	TLayoutModel extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel,
	TOriginData = unknown,
> {
	originData: TOriginData;
	layout: TLayoutModel;
}

export class UmbBlockCreatedEvent<
	TLayoutModel extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel,
	TOriginData = unknown,
> extends CustomEvent<UmbBlockCreatedEventDetail<TLayoutModel, TOriginData>> {
	static readonly TYPE = 'umb-internal:blockCreated';

	constructor(detail: UmbBlockCreatedEventDetail<TLayoutModel, TOriginData>) {
		super(UmbBlockCreatedEvent.TYPE, { detail, bubbles: false, composed: false, cancelable: false });
	}
}

declare global {
	interface GlobalEventHandlersEventMap {
		[UmbBlockCreatedEvent.TYPE]: UmbBlockCreatedEvent;
	}
}
