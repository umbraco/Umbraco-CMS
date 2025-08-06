import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export class UmbExpansionEntityExpandedEvent extends Event {
	public static readonly TYPE = 'expansion-entity-expanded';
	entry: UmbEntityModel;

	public constructor(entry: UmbEntityModel) {
		// mimics the native change event
		super(UmbExpansionEntityExpandedEvent.TYPE, { bubbles: true, composed: false, cancelable: false });
		this.entry = entry;
	}
}
