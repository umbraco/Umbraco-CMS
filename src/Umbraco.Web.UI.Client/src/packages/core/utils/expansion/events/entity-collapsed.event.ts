import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export class UmbEntityCollapsedEvent extends Event {
	public static readonly TYPE = 'entity-collapsed';
	entity: UmbEntityModel | undefined;

	public constructor(entity?: UmbEntityModel) {
		// mimics the native change event
		super(UmbEntityCollapsedEvent.TYPE, { bubbles: true, composed: false, cancelable: false });
		this.entity = entity;
	}
}
