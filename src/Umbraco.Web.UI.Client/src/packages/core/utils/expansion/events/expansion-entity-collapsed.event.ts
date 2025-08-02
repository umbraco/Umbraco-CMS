import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export class UmbExpansionEntityCollapsedEvent extends Event {
	public static readonly TYPE = 'expansion-entity-collapsed';
	entity: UmbEntityModel;

	public constructor(entity: UmbEntityModel) {
		// mimics the native change event
		super(UmbExpansionEntityCollapsedEvent.TYPE, { bubbles: true, composed: false, cancelable: false });
		this.entity = entity;
	}
}
