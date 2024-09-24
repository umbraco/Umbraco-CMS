import { UmbSubmittableWorkspaceContextBase } from '../submittable/index.js';
import { UmbEntityWorkspaceDataManager } from './entity-workspace-data-manager.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UMB_DISCARD_CHANGES_MODAL, UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export abstract class UmbEntityWorkspaceContextBase<
	EntityModelType extends UmbEntityModel,
> extends UmbSubmittableWorkspaceContextBase<EntityModelType> {
	/**
	 * @description Data manager for the workspace.
	 * @protected
	 * @memberof UmbEntityWorkspaceContextBase
	 */
	protected readonly _data = new UmbEntityWorkspaceDataManager<EntityModelType>(this);

	constructor(host: UmbControllerHost, workspaceAlias: string) {
		super(host, workspaceAlias);
		window.addEventListener('willchangestate', this.#onWillNavigate);
	}

	/**
	 * @description method to check if the workspace is about to navigate away.
	 * @protected
	 * @param {string} newUrl
	 * @returns {*}
	 * @memberof UmbEntityWorkspaceContextBase
	 */
	protected _checkWillNavigateAway(newUrl: string) {
		let willNavigateAway = false;

		if (this.getIsNew()) {
			willNavigateAway = !newUrl.includes(`${this.getEntityType()}/create`);
		} else {
			willNavigateAway = !newUrl.includes(this.getUnique()!);
		}

		return willNavigateAway;
	}

	#onWillNavigate = async (e: CustomEvent) => {
		const newUrl = e.detail.url;

		if (this._checkWillNavigateAway(newUrl) && this._data.hasUnpersistedChanges()) {
			e.preventDefault();
			const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
			const modal = modalManager.open(this, UMB_DISCARD_CHANGES_MODAL);

			try {
				// navigate to the new url when discarding changes
				await modal.onSubmit();
				// Reset the current data so we don't end in a endless loop of asking to discard changes.
				this._data.resetCurrentData();
				history.pushState({}, '', e.detail.url);
				return true;
			} catch {
				return false;
			}
		}

		return true;
	};

	public override destroy(): void {
		this._data.destroy();
		window.removeEventListener('willchangestate', this.#onWillNavigate);
		super.destroy();
	}
}
