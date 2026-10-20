import type { UmbParallelHookMethod } from '@umbraco-cms/backoffice/hook-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UMB_CONTENT_WORKSPACE_CONTEXT,
	type UmbContentDetailIncomingDataHookMeta,
} from '@umbraco-cms/backoffice/content';
import { mergeObservables, UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbVariantId, type UmbEntityVariantOptionSourceModel } from '@umbraco-cms/backoffice/variant';

interface ExampleSegmentModel {
	alias: string;
	name: string;
	/**
	 * Where the segment is available:
	 * - `undefined`: everywhere, for each culture and for invariant content.
	 * - `null`: only for content that does not vary by culture.
	 * - An array of culture codes: only for those cultures.
	 */
	cultures?: Array<string> | null;
}

// Stands in for a request to a server or a third-party personalization service.
const fetchExampleSegments = async (): Promise<Array<ExampleSegmentModel>> => {
	await new Promise((resolve) => setTimeout(resolve, 300));
	return [
		{ alias: 'example-returning-visitor', name: 'Returning visitor' },
		{ alias: 'example-campaign', name: 'Campaign', cultures: null },
		{ alias: 'example-danish-visitor', name: 'Danish visitor', cultures: ['da'] },
	];
};

const isSegmentAvailableFor = (segment: ExampleSegmentModel, culture: string | null) => {
	if (segment.cultures === undefined) return true;
	if (segment.cultures === null) return culture === null;
	return culture !== null && segment.cultures.includes(culture);
};

const observeCtrlAlias = Symbol();

export class ExampleSegmentProviderWorkspaceContext extends UmbControllerBase {
	#workspace?: typeof UMB_CONTENT_WORKSPACE_CONTEXT.TYPE;
	#segments = new UmbArrayState<ExampleSegmentModel, string, undefined>(undefined, (x) => x.alias);
	#addedUniques: Array<string> = [];

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_CONTENT_WORKSPACE_CONTEXT, (workspace) => {
			this.#workspace?.incomingDataHook.remove(this.#onIncomingData);
			this.#workspace = workspace;
			if (!workspace) {
				this.removeUmbControllerByAlias(observeCtrlAlias);
				return;
			}

			// The workspace awaits every method of this hook before it presents the incoming data,
			// so the segments are in place when the variant selector renders.
			workspace.incomingDataHook.add(this.#onIncomingData);

			const manager = workspace.variantOptionsManager;
			this.observe(
				mergeObservables(
					[this.#segments.asObservable(), workspace.variesByCulture, workspace.variesBySegment, manager.languages],
					(sources) => sources,
				),
				([segments, variesByCulture, variesBySegment, languages]) => {
					const oldUniques = this.#addedUniques;

					if (!segments || !variesBySegment || variesByCulture === undefined || !languages) {
						this.#addedUniques = [];
						manager.remove(oldUniques);
						return;
					}

					// Options are added per culture, or with a null culture when the content does not vary by culture.
					const cultures = variesByCulture ? languages.map((language) => language.unique) : [null];
					const options = cultures.flatMap((culture) =>
						segments
							.filter((segment) => isSegmentAvailableFor(segment, culture))
							.map(
								(segment): UmbEntityVariantOptionSourceModel => ({
									culture,
									segment: segment.alias,
									segmentInfo: { alias: segment.alias, name: segment.name },
									unique: new UmbVariantId(culture, segment.alias).toString(),
								}),
							),
					);

					this.#addedUniques = options.map((option) => option.unique);
					manager.replace(oldUniques, options);
				},
				observeCtrlAlias,
			);
		});
	}

	#onIncomingData: UmbParallelHookMethod<UmbContentDetailIncomingDataHookMeta> = async () => {
		// The segments do not depend on the entity, so they are only requested once.
		if (this.#segments.getValue()) return;
		this.#segments.setValue(await fetchExampleSegments());
	};

	override destroy() {
		this.#workspace?.variantOptionsManager.remove(this.#addedUniques);
		this.#workspace?.incomingDataHook.remove(this.#onIncomingData);
		super.destroy();
	}
}

export { ExampleSegmentProviderWorkspaceContext as api };
