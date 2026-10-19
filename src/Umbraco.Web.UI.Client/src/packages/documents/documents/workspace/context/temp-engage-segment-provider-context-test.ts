import { UmbEngageSegmentRepository } from './temp-engange.repository.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbHookMethod } from '@umbraco-cms/backoffice/hook-api';
import type { UmbEntityDetailIncomingDataHookMeta } from '@umbraco-cms/backoffice/workspace';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { mergeObservables, UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbVariantId, type UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import type { UmbSegmentModel } from '@umbraco-cms/backoffice/segment';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import { UMB_CONTENT_WORKSPACE_CONTEXT, type UmbContentDetailModel } from '@umbraco-cms/backoffice/content';

const observeCtrlAlias = Symbol();

export class UmbEngageSegmentProviderContextTest extends UmbControllerBase {
	#repository = new UmbEngageSegmentRepository(this);
	#segments = new UmbArrayState<UmbSegmentModel, string, undefined>(undefined, (x) => x.alias);
	#addedUniques: Array<string> = [];
	#workspace?: typeof UMB_CONTENT_WORKSPACE_CONTEXT.TYPE;
	#currentUnique?: string;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_CONTENT_WORKSPACE_CONTEXT, (workspace) => {
			this.#workspace?.incomingDataHook.remove(this.#onIncomingData);
			this.#workspace = workspace;
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
		return languages.flatMap((language) =>
			this.#createOptions(
				segments.filter((s) => !s.cultures || s.cultures.includes(language.unique)),
				language.unique,
			),
		);
	}

	#getNotCultureSegmentOptions(segments: UmbSegmentModel[]) {
		return this.#createOptions(
			segments.filter((s) => !s.cultures),
			null,
		);
	}

	#onIncomingData: UmbHookMethod<UmbContentDetailModel, UmbEntityDetailIncomingDataHookMeta> = async (data) => {
		if (this.#currentUnique === data.unique) {
			// we already have loaded segments, user must refresh for update.
			return data;
		}
		this.#currentUnique = data.unique;
		const { data: response } = await this.#repository.getDocumentByIdSegmentOptions(data.unique, {
			skip: 0,
			take: 9999,
		});
		this.#segments.setValue(response?.items ?? []);
		return data;
	};

	#createOptions(segments: Array<UmbSegmentModel>, culture: string | null) {
		return segments.map(
			(segment) =>
				({
					culture,
					segment: segment.alias,
					segmentInfo: { alias: segment.alias, name: segment.name },
					unique: new UmbVariantId(culture, segment.alias).toString(),
				}) as Omit<UmbEntityVariantOptionModel, 'language'>,
		);
	}

	override destroy() {
		this.#workspace?.variantOptionsManager.remove(this.#addedUniques);
		this.#workspace?.incomingDataHook.remove(this.#onIncomingData);
		super.destroy();
	}
}

export { UmbEngageSegmentProviderContextTest as api };
