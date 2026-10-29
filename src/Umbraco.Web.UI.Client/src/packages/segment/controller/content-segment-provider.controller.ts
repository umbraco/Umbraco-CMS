import { UmbSegmentCollectionRepository } from '../collection/index.js';
import type { UmbSegmentModel } from '../types.js';
import { UMB_CONTENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/content';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbParallelHookMethod } from '@umbraco-cms/backoffice/hook-api';
import type { UmbEntityDetailIncomingDataHookMeta } from '@umbraco-cms/backoffice/workspace';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { mergeObservables, UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbVariantId, type UmbEntityVariantOptionSourceModel } from '@umbraco-cms/backoffice/variant';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';

const observeCtrlAlias = Symbol();

export class UmbDocumentSegmentProvider extends UmbControllerBase {
	// Use UmbSegmentCollectionRepository

	#repository = new UmbSegmentCollectionRepository(this);
	#segments = new UmbArrayState<UmbSegmentModel, string, undefined>(undefined, (x) => x.alias);
	#addedUniques: Array<string> = [];
	#workspace?: typeof UMB_CONTENT_WORKSPACE_CONTEXT.TYPE;
	request?: Promise<void>;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_CONTENT_WORKSPACE_CONTEXT, (workspace) => {
			this.#workspace?.incomingDataHook.remove(this.#onIncomingData);
			this.#workspace = workspace;

			this.observe(
				workspace?.variesBySegment,
				(variesBySegment) => {
					if (variesBySegment) {
						// If varies by segment changes while a workspace is present, then we load them when they are needed.
						this.#loadSegments();
					}
				},
				'observeVaryBySeg',
			);

			if (!workspace) {
				this.removeUmbControllerByAlias(observeCtrlAlias);
				return;
			}

			workspace.incomingDataHook.add(this.#onIncomingData);

			const manager = workspace.variantOptionsManager;
			this.observe(
				mergeObservables(
					[this.#segments.asObservable(), workspace.variesByCulture, workspace.variesBySegment, manager.languages],
					([segments, variesByCulture, variesBySegment, languages]) => ({
						segments,
						variesByCulture,
						variesBySegment,
						languages,
					}),
				),
				({ segments, variesByCulture, variesBySegment, languages }) => {
					const oldUniques = this.#addedUniques;
					this.#addedUniques = [];

					if (variesByCulture === undefined || languages === undefined || !variesBySegment || segments === undefined) {
						// Ensure old uniques are removed before adding new ones
						manager.remove(oldUniques);
						return;
					}

					const options = variesByCulture
						? this.#getVaryByCultureSegmentOptions(segments, languages)
						: this.#getNotCultureSegmentOptions(segments);

					this.#addedUniques = options.map((o) => o.unique);
					manager.replace(oldUniques, options);
				},
				observeCtrlAlias,
			);
		});
	}

	#getVaryByCultureSegmentOptions(segments: UmbSegmentModel[], languages: UmbLanguageDetailModel[]) {
		return languages.flatMap((language) => this.#createOptions(segments, language.unique));
	}

	#getNotCultureSegmentOptions(segments: UmbSegmentModel[]) {
		return this.#createOptions(segments, null);
	}

	#onIncomingData: UmbParallelHookMethod<UmbEntityDetailIncomingDataHookMeta> = async () => {
		const variesBySegment = await this.observe(this.#workspace?.variesBySegment)?.asPromise();
		// If we know that we vary by segment when data is incoming, then we should make the request of segments take part in the incoming-data flow.
		if (variesBySegment) {
			// Segments do not depend on the entity, so they are requested once and shared by concurrent loads. A failed request is retried on the next load.
			return await this.#loadSegments();
		}
	};

	#loadSegments() {
		this.request ??= this.#requestSegments();
		return this.request;
	}

	async #requestSegments() {
		// What happens when we leave out the skip/take? [NL]
		const { data: response } = await this.#repository.requestCollection({
			skip: 0,
			take: 9999,
		});
		if (!response) {
			this.request = undefined;
		}
		this.#segments.setValue(response?.items);
	}

	#createOptions(segments: Array<UmbSegmentModel>, culture: string | null) {
		return segments.map(
			(segment) =>
				({
					culture,
					segment: segment.alias,
					segmentInfo: { alias: segment.alias, name: segment.name },
					unique: new UmbVariantId(culture, segment.alias).toString(),
				}) as UmbEntityVariantOptionSourceModel,
		);
	}

	override destroy() {
		// TODO: Test that they are not removed twice in the case of an unregistration.. [NL]
		// Remove last added segment options:
		this.#workspace?.variantOptionsManager.remove(this.#addedUniques);
		this.#workspace?.incomingDataHook.remove(this.#onIncomingData);
		super.destroy();
	}
}

export { UmbDocumentSegmentProvider as api };
