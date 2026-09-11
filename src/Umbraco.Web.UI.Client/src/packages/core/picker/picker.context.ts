import { UMB_PICKER_CONTEXT } from './picker.context.token.js';
import { UmbPickerSearchManager } from './search/manager/picker-search.manager.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbInteractionMemoryManager } from '@umbraco-cms/backoffice/interaction-memory';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbContextMinimal } from '@umbraco-cms/backoffice/context-api';

export class UmbPickerContext extends UmbContextBase {
	public readonly interactionMemory = new UmbInteractionMemoryManager(this);
	public readonly selection = new UmbSelectionManager(this);
	public readonly search = new UmbPickerSearchManager(this);

	public dataType?: { unique: string };

	constructor(host: UmbControllerHost) {
		super(host, UMB_PICKER_CONTEXT);

		/* TODO: Move this implementation to another place. The generic picker context shouldn't be aware of property and data types.

		HACK: The alias is deliberately a hardcoded string rather than an import of
		`UMB_PROPERTY_TYPE_BASED_PROPERTY_CONTEXT` from `@umbraco-cms/backoffice/content`. Importing it makes the picker
		module depend on the content module, which closes an import cycle and — because the tree module also depends on
		picker — blocks any module that legitimately needs to depend on picker from being able to.

		This removes the *coding* dependency but not the *functional* one: the alias still resolves a context that the
		content module owns, so picker remains functionally coupled to content, and a rename there breaks this at runtime
		rather than at compile time. It is done this way only because the cycle is blocking other work. */
		this.consumeContext<UmbContextMinimal & { dataType: Observable<{ unique: string } | undefined> }>(
			'UmbPropertyTypeBasedPropertyContext',
			(context) => {
				this.observe(context?.dataType, (dataType) => {
					this.dataType = dataType;
				});
			},
		);
	}
}
