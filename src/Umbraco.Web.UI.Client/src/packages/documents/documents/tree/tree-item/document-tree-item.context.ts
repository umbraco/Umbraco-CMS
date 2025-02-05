import type { UmbDocumentTreeItemModel, UmbDocumentTreeRootModel } from '../types.js';
import { UmbDefaultTreeItemContext } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbIsTrashedEntityContext } from '@umbraco-cms/backoffice/recycle-bin';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityUpdatedEvent } from '@umbraco-cms/backoffice/entity-action';

export class UmbDocumentTreeItemContext extends UmbDefaultTreeItemContext<
	UmbDocumentTreeItemModel,
	UmbDocumentTreeRootModel
> {
	// TODO: Provide this together with the EntityContext, ideally this takes part via a extension-type [NL]
	#isTrashedContext = new UmbIsTrashedEntityContext(this);
	#actionEventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;

	readonly isTrashed = this._treeItem.asObservablePart((item) => item?.isTrashed ?? false);

	constructor(host: UmbControllerHost) {
		super(host);

		this.observe(this.isTrashed, (isTrashed) => {
			this.#isTrashedContext.setIsTrashed(isTrashed);
		});

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (actionEvent) => {
			this.#removeEventListeners();

			this.#actionEventContext = actionEvent;

			this.#actionEventContext?.addEventListener(
				UmbEntityUpdatedEvent.TYPE,
				this.#onEntityUpdated as unknown as EventListener,
			);
		});
	}

	#onEntityUpdated = (event: UmbEntityUpdatedEvent) => {
		const entityType = event.getEntityType();

		// we don't want to update unless it's a document type
		if (entityType !== 'document-type') return;

		const treeItem = this.getTreeItem();
		if (!treeItem) return;

		// we don't wont to update if the document type is not the same
		if (treeItem.documentType.unique !== event.getUnique()) return;

		if (this.parentTreeItemContext) {
			this.parentTreeItemContext?.loadChildren();
		} else {
			this.treeContext?.loadTree();
		}
	};

	#removeEventListeners() {
		this.#actionEventContext?.removeEventListener(
			UmbEntityUpdatedEvent.TYPE,
			this.#onEntityUpdated as unknown as EventListener,
		);
	}

	override destroy(): void {
		this.#removeEventListeners();
		super.destroy();
	}
}

export { UmbDocumentTreeItemContext as api };
