import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export class UmbExpansionEntityExpandedEvent extends Event {
	public static readonly TYPE = 'expansion-entity-expanded';
	entity: UmbEntityModel | undefined;

	public constructor(entity: UmbEntityModel) {
		// mimics the native change event
		super(UmbExpansionEntityExpandedEvent.TYPE, { bubbles: true, composed: false, cancelable: false });
		this.entity = entity;
	}
}
