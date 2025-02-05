import type { ManifestTreeItemRecycleBinKind } from './types.js';
import { UmbDefaultTreeItemContext, type UmbTreeItemModel, type UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityTrashedEvent } from '@umbraco-cms/backoffice/recycle-bin';

export class UmbRecycleBinTreeItemContext<
	RecycleBinTreeItemModelType extends UmbTreeItemModel,
	RecycleBinTreeRootModelType extends UmbTreeRootModel,
> extends UmbDefaultTreeItemContext<
	RecycleBinTreeItemModelType,
	RecycleBinTreeRootModelType,
	ManifestTreeItemRecycleBinKind
> {
	#actionEventContext?: UmbActionEventContext;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (instance) => {
			this.#removeEventListener();
			this.#actionEventContext = instance;
			this.#actionEventContext?.addEventListener(UmbEntityTrashedEvent.TYPE, this.#onEntityTrashed as EventListener);
		});
	}

	#onEntityTrashed = (event: UmbEntityTrashedEvent) => {
		const entityType = event.getEntityType();
		if (!entityType) throw new Error('Entity type is required');

		const supportedEntityTypes = this.getManifest()?.meta.supportedEntityTypes;

		if (!supportedEntityTypes) {
			throw new Error('Supported entity types are missing from the manifest. (manifest.meta.supportedEntityTypes)');
		}

		if (supportedEntityTypes.includes(entityType)) {
			this.loadChildren();
		}
	};

	#removeEventListener = () => {
		this.#actionEventContext?.removeEventListener(UmbEntityTrashedEvent.TYPE, this.#onEntityTrashed as EventListener);
	};

	override destroy(): void {
		this.#removeEventListener();
		super.destroy();
	}
}

export { UmbRecycleBinTreeItemContext as api };
