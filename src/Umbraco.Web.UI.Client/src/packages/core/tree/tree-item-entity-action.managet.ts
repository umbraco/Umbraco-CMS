import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { map } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export class UmbTreeItemEntityActionManager extends UmbControllerBase {
	#hasActions = new UmbBooleanState(false);
	readonly hasActions = this.#hasActions.asObservable();

	#entity?: UmbEntityModel;
	#observerController?: UmbObserverController;

	setEntity(entity: UmbEntityModel | undefined) {
		this.#entity = entity;

		if (entity && entity.entityType) {
			this.#observeActions();
		}
	}

	getEntity() {
		return this.#entity;
	}

	#observeActions() {
		const entityType = this.getEntity()?.entityType;

		if (!entityType) {
			this.#observerController?.destroy();
			return;
		}

		this.observe(
			umbExtensionsRegistry
				.byType('entityAction')
				.pipe(map((actions) => actions.filter((action) => action.forEntityTypes.includes(entityType)))),
			(actions) => {
				this.#hasActions.setValue(actions.length > 0);
			},
			'observeActions',
		);
	}
}
