import { UMB_SEARCH_CONTEXT } from '../global-context/search.global-context.js';
import {
	UMB_SEARCH_DETAIL_REPOSITORY_ALIAS,
	UMB_SEARCH_INDEX_ENTITY_TYPE,
	UMB_SEARCH_WORKSPACE_ALIAS,
} from '../constants.js';
import { UmbSearchDetailRepository } from '../detail/search-detail.repository.js';
import type { UmbSearchIndex, UmbSearchIndexState } from '../types.js';
import { UmbSearchWorkspaceEditorElement } from './search-workspace-editor.element.js';

import {
	UmbEntityNamedDetailWorkspaceContextBase,
	type UmbRoutableWorkspaceContext,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { mergeObservables, UmbBasicState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';

export class UmbSearchWorkspaceContext
	extends UmbEntityNamedDetailWorkspaceContextBase<UmbSearchIndex, UmbSearchDetailRepository>
	implements UmbRoutableWorkspaceContext
{
	public readonly repository = new UmbSearchDetailRepository(this);
	public readonly documentCount = this._data.createObservablePartOfPersisted((x) => x?.documentCount);
	public readonly healthStatus = this._data.createObservablePartOfPersisted((x) => x?.healthStatus);
	public readonly providerName = this._data.createObservablePartOfPersisted((x) => x?.providerName);
	#pendingState = new UmbBasicState<UmbSearchIndexState | undefined>(undefined);

	/**
	 * The state of a rebuild the user just triggered, falling back to the state the server reports.
	 * The pending part is deliberately kept out of the workspace data: writing it there diverges
	 * current from persisted, which makes the workspace look edited and prompts to discard on exit.
	 */
	public readonly state = mergeObservables(
		[this.#pendingState.asObservable(), this._data.createObservablePartOfPersisted((x) => x?.state)],
		([pending, persisted]) => pending ?? persisted ?? 'idle',
	);

	#selectedCulture = new UmbStringState(undefined);
	public readonly selectedCulture = this.#selectedCulture.asObservable();

	getSelectedCulture(): string | undefined {
		return this.#selectedCulture.getValue();
	}

	setSelectedCulture(culture: string | undefined) {
		this.#selectedCulture.setValue(culture);
	}

	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_SEARCH_WORKSPACE_ALIAS,
			entityType: UMB_SEARCH_INDEX_ENTITY_TYPE,
			detailRepositoryAlias: UMB_SEARCH_DETAIL_REPOSITORY_ALIAS,
		});

		this.routes.setRoutes([
			{
				path: 'edit/:unique',
				component: UmbSearchWorkspaceEditorElement,
				setup: (_component, info) => {
					void this.load(info.match.params.unique);
				},
			},
		]);

		this.consumeContext(UMB_SEARCH_CONTEXT, (searchContext) => {
			this.observe(
				searchContext?.indexRebuilt,
				(indexAlias) => {
					if (!indexAlias) return;
					if (indexAlias !== this.getUnique()) return;
					this.#pendingState.setValue(undefined);
					void this.reload();
				},
				'index-rebuild-completed-detail-observer',
			);
		});
	}

	setState(state: UmbSearchIndexState) {
		this.#pendingState.setValue(state);
	}
}

export default UmbSearchWorkspaceContext;
