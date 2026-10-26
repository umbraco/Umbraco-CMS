import { UmbSchemaOperationAllowedController } from '../schema-operation-allowed.controller.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

const SCHEMA_LOCKDOWN_UNIQUE = 'UMB_SCHEMA_LOCKDOWN';

/**
 * Makes the workspace entity read-only while schema lockdown doesn't allow saving it: creating it while it is new,
 * and updating it otherwise. A new entity that can't be created is also marked forbidden, so the workspace shows
 * that access is denied. Workspaces whose entity type isn't governed by schema lockdown are left untouched.
 */
export class UmbSchemaLockdownWorkspaceController extends UmbControllerBase {
	#saveAllowed = new UmbSchemaOperationAllowedController(this, { operation: 'createOrUpdate' });
	#createAllowed = new UmbSchemaOperationAllowedController(this, { operation: 'create' });
	#workspaceContext?: typeof UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT.TYPE;
	#isNew?: boolean;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT, (context) => {
			this.#clear();
			this.#workspaceContext = context;
			this.#isNew = undefined;
			const entityType = context?.getEntityType();
			this.#saveAllowed.setEntityType(entityType);
			this.#createAllowed.setEntityType(entityType);
			this.observe(
				context?.isNew,
				(isNew) => {
					this.#isNew = isNew;
					this.#update();
				},
				'_observeIsNew',
			);
			// Loading or creating an entity clears the workspace's read-only rules and forbidden state, so they are applied
			// again for each entity.
			this.observe(context?.unique, () => this.#update(), '_observeUnique');
			this.observe(context?.forbidden.isOn, () => this.#update(), '_observeIsForbidden');
		});

		this.observe(this.#saveAllowed.allowed, () => this.#update(), null);
		this.observe(this.#createAllowed.allowed, () => this.#update(), null);
	}

	#update() {
		const context = this.#workspaceContext;
		if (!context) return;

		if (this.#saveAllowed.getAllowed()) {
			context.readOnlyGuard.removeRule(SCHEMA_LOCKDOWN_UNIQUE);
		} else {
			context.readOnlyGuard.addRule({ unique: SCHEMA_LOCKDOWN_UNIQUE, permitted: true });
		}

		const isForbidden = this.#isNew === true && !this.#createAllowed.getAllowed();
		const hasForbiddenState = context.forbidden.getStates().some((state) => state.unique === SCHEMA_LOCKDOWN_UNIQUE);
		if (isForbidden && !hasForbiddenState) {
			context.forbidden.addState({ unique: SCHEMA_LOCKDOWN_UNIQUE });
		} else if (!isForbidden && hasForbiddenState) {
			context.forbidden.removeState(SCHEMA_LOCKDOWN_UNIQUE);
		}
	}

	#clear() {
		this.#workspaceContext?.readOnlyGuard.removeRule(SCHEMA_LOCKDOWN_UNIQUE);
		this.#workspaceContext?.forbidden.removeState(SCHEMA_LOCKDOWN_UNIQUE);
	}

	override destroy() {
		this.#clear();
		super.destroy();
	}
}

export { UmbSchemaLockdownWorkspaceController as api };
