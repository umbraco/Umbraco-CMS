import type { UmbDocumentRecycleBinTreeItemModel, UmbDocumentRecycleBinTreeRootModel } from '../types.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../../entity.js';
import { UmbDefaultTreeItemContext } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityTrashedEvent } from '@umbraco-cms/backoffice/recycle-bin';

export class UmbDocumentRecycleBinRootTreeItemContext extends UmbDefaultTreeItemContext<
	UmbDocumentRecycleBinTreeItemModel,
	UmbDocumentRecycleBinTreeRootModel
> {
	#actionEventContext?: UmbActionEventContext;
	#entityTypes = [UMB_DOCUMENT_ENTITY_TYPE];

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (instance) => {
			this.#actionEventContext = instance;
			this.#actionEventContext.addEventListener(UmbEntityTrashedEvent.TYPE, this.#onEntityTrashed as EventListener);
		});
	}

	#onEntityTrashed = (event: UmbEntityTrashedEvent) => {
		const entityType = event.getEntityType();

		if (this.#entityTypes.includes(entityType)) {
			this.loadChildren();
		}
	};

	override destroy(): void {
		this.#actionEventContext?.removeEventListener(UmbEntityTrashedEvent.TYPE, this.#onEntityTrashed as EventListener);
		super.destroy();
	}
}

export { UmbDocumentRecycleBinRootTreeItemContext as api };
