import { UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT } from '../entity-detail-workspace.context-token.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbPermissionRegistry, type UmbPermissionResult } from '@umbraco-cms/backoffice/permission';

const PERMISSION_UNIQUE = 'umbEntityDetailWorkspacePermission';

export interface UmbEntityDetailWorkspacePermissionBridgeControllerArgs {
	/** The permission verb required to create the entity, such as `Umb.DocumentType.Create`. */
	create?: string;
	/** The permission verb required to update the entity, such as `Umb.DocumentType.Update`. */
	update?: string;
}

/**
 * Bridges the permission registry's results to the entity-detail workspace's `readOnlyGuard` and `forbidden` state;
 * read those, not this controller. Adds a read-only rule while saving is denied (the create verb for a new entity, the
 * update verb for an existing one) and a forbidden state while creating a new entity is denied. Nothing changes while
 * a result is still undetermined.
 */
export class UmbEntityDetailWorkspacePermissionBridgeController extends UmbControllerBase {
	#workspaceContext?: typeof UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT.TYPE;
	#isNew?: boolean;
	#createResult?: UmbPermissionResult;
	#updateResult?: UmbPermissionResult;

	constructor(host: UmbControllerHost, args: UmbEntityDetailWorkspacePermissionBridgeControllerArgs) {
		super(host);

		if (args.create) {
			this.observe(
				umbPermissionRegistry.permissionResult(this, { allOf: [args.create] }),
				(result) => {
					this.#createResult = result;
					this.#update();
				},
				null,
			);
		}

		if (args.update) {
			this.observe(
				umbPermissionRegistry.permissionResult(this, { allOf: [args.update] }),
				(result) => {
					this.#updateResult = result;
					this.#update();
				},
				null,
			);
		}

		this.consumeContext(UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT, (context) => {
			this.#clear();
			this.#workspaceContext = context;
			this.#isNew = undefined;
			this.observe(
				context?.isNew,
				(isNew) => {
					this.#isNew = isNew;
					this.#update();
				},
				'_observeIsNew',
			);
		});
	}

	#update() {
		const context = this.#workspaceContext;
		if (!context) return;

		const saveResult = this.#isNew === undefined ? undefined : this.#isNew ? this.#createResult : this.#updateResult;
		if (saveResult?.permitted === false) {
			context.readOnlyGuard.addRule({
				unique: PERMISSION_UNIQUE,
				permitted: true,
				message: saveResult.denyReasons[0]?.title,
			});
		} else {
			context.readOnlyGuard.removeRule(PERMISSION_UNIQUE);
		}

		const existingState = context.forbidden.getStates().find((state) => state.unique === PERMISSION_UNIQUE);
		if (this.#isNew === true && this.#createResult?.permitted === false) {
			const reason = this.#createResult.denyReasons[0];
			if (existingState) {
				if (existingState.reason?.title === reason?.title && existingState.reason?.detail === reason?.detail) return;
				context.forbidden.removeState(PERMISSION_UNIQUE);
			}
			context.forbidden.addState({ unique: PERMISSION_UNIQUE, reason });
		} else if (existingState) {
			context.forbidden.removeState(PERMISSION_UNIQUE);
		}
	}

	#clear() {
		this.#workspaceContext?.readOnlyGuard.removeRule(PERMISSION_UNIQUE);
		this.#workspaceContext?.forbidden.removeState(PERMISSION_UNIQUE);
	}
}
