import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export class UmbExpansionEntryExpandedEvent<EntryModelType extends UmbEntityModel = UmbEntityModel> extends Event {
	public static readonly TYPE = 'expansion-entry-expanded';
	entry: EntryModelType;

	public constructor(entry: EntryModelType) {
		// mimics the native change event
		super(UmbExpansionEntryExpandedEvent.TYPE, { bubbles: true, composed: false, cancelable: false });
		this.entry = entry;
	}
}
