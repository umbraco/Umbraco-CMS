import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export class UmbExpansionEntryCollapsedEvent<EntryModelType extends UmbEntityModel = UmbEntityModel> extends Event {
	public static readonly TYPE = 'expansion-entry-collapsed';
	entry: EntryModelType;

	public constructor(entry: EntryModelType) {
		// mimics the native change event
		super(UmbExpansionEntryCollapsedEvent.TYPE, { bubbles: true, composed: false, cancelable: false });
		this.entry = entry;
	}
}
