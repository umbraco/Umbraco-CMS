import type { UmbScriptDetailModel } from '../types.js';
import { UMB_SCRIPT_ENTITY_TYPE } from '../entity.js';
import { UMB_SCRIPT_DETAIL_REPOSITORY_ALIAS, type UmbScriptDetailRepository } from '../repository/index.js';
import { UMB_SCRIPT_WORKSPACE_ALIAS } from './manifests.js';
import { UmbScriptWorkspaceEditorElement } from './script-workspace-editor.element.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbEntityDetailWorkspaceContextBase,
	type UmbRoutableWorkspaceContext,
	type UmbSubmittableWorkspaceContext,
	UmbWorkspaceIsNewRedirectController,
} from '@umbraco-cms/backoffice/workspace';
import type { IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbServerFileRenamedEntityEvent } from '@umbraco-cms/backoffice/server-file-system';
import { ensurePathEndsWithSlash, umbUrlPatternToString } from '@umbraco-cms/backoffice/utils';

export class UmbScriptWorkspaceContext
	extends UmbEntityDetailWorkspaceContextBase<UmbScriptDetailModel, UmbScriptDetailRepository>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	public readonly name = this._data.createObservablePartOfCurrent((data) => data?.name);
	public readonly content = this._data.createObservablePartOfCurrent((data) => data?.content);

	#actionEventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_SCRIPT_WORKSPACE_ALIAS,
			entityType: UMB_SCRIPT_ENTITY_TYPE,
			detailRepositoryAlias: UMB_SCRIPT_DETAIL_REPOSITORY_ALIAS,
		});

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (context) => {
			this.#actionEventContext = context;

			if (this.#actionEventContext) {
				this.#actionEventContext.removeEventListener(UmbServerFileRenamedEntityEvent.TYPE, this.#onFileRenamed);
				this.#actionEventContext.addEventListener(UmbServerFileRenamedEntityEvent.TYPE, this.#onFileRenamed);
			}
		});

		this.routes.setRoutes([
			{
				path: 'create/parent/:entityType/:parentUnique',
				component: UmbScriptWorkspaceEditorElement,
				setup: async (component: PageComponent, info: IRoutingInfo) => {
					const parentEntityType = info.match.params.entityType;
					const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
					await this.createScaffold({ parent: { entityType: parentEntityType, unique: parentUnique } });

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: 'edit/:unique',
				component: UmbScriptWorkspaceEditorElement,
				setup: (component: PageComponent, info: IRoutingInfo) => {
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	/**
	 * @description Set the name of the script
	 * @param {string} value
	 * @memberof UmbScriptWorkspaceContext
	 */
	public setName(value: string) {
		this._data.updateCurrent({ name: value });
	}

	/**
	 * @description Set the content of the script
	 * @param {string} value
	 * @memberof UmbScriptWorkspaceContext
	 */
	public setContent(value: string) {
		this._data.updateCurrent({ content: value });
	}

	#onFileRenamed = ((event: UmbServerFileRenamedEntityEvent) => {
		const currentUnique = this.getUnique();
		const eventUnique = event.getUnique();
		if (currentUnique !== eventUnique) return;

		const newUnique = event.getNewUnique();
		if (!newUnique) throw new Error('New unique is required for this event.');

		const router = this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!;

		if (router) {
			const routerPath = router.absoluteRouterPath;
			if (routerPath) {
				const newPath: string = umbUrlPatternToString(ensurePathEndsWithSlash(routerPath) + 'edit/:unique', {
					unique: newUnique,
				});
				window.history.pushState({}, '', newPath);
			}
		}
	}) as EventListener;

	public override destroy(): void {
		super.destroy();
		this.#actionEventContext?.removeEventListener(UmbServerFileRenamedEntityEvent.TYPE, this.#onFileRenamed);
	}
}

export { UmbScriptWorkspaceContext as api };
