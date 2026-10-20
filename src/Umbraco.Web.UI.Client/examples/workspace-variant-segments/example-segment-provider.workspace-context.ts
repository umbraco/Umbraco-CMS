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
	 * - `'all'`: for each culture, and for content that does not vary by culture.
	 * - `'invariant'`: only for content that does not vary by culture.
	 * - An array of culture codes: only for those cultures.
	 */
	availableFor: 'all' | 'invariant' | Array<string>;
}

// The "Culture And Segment Variant Document Type" of the "documents" mock data set.
// Replace it with the unique of a Document Type in your own installation.
const EXAMPLE_NEWSLETTER_DOCUMENT_TYPE_UNIQUE = 'variant-documents-culture-and-segment-variant-document-type-id';

// Stands in for a request to a server or a third-party personalization service,
// which can answer with segments that depend on the content being opened.
const fetchExampleSegments = async (contentTypeUnique: string | undefined): Promise<Array<ExampleSegmentModel>> => {
	await new Promise((resolve) => setTimeout(resolve, 300));
	const segments: Array<ExampleSegmentModel> = [
		{ alias: 'example-returning-visitor', name: 'Returning visitor', availableFor: 'all' },
		{ alias: 'example-campaign', name: 'Campaign', availableFor: 'invariant' },
		{ alias: 'example-danish-visitor', name: 'Danish visitor', availableFor: ['da'] },
	];
	if (contentTypeUnique === EXAMPLE_NEWSLETTER_DOCUMENT_TYPE_UNIQUE) {
		segments.push({ alias: 'example-newsletter-subscriber', name: 'Newsletter subscriber', availableFor: 'all' });
	}
	return segments;
};

const isSegmentAvailableFor = (segment: ExampleSegmentModel, culture: string | null) => {
	if (segment.availableFor === 'all') return true;
	if (segment.availableFor === 'invariant') return culture === null;
	return culture !== null && segment.availableFor.includes(culture);
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

	#onIncomingData: UmbParallelHookMethod<UmbContentDetailIncomingDataHookMeta> = async (meta) => {
		// The meta describes the incoming content, so the segments are requested for its Document Type on every load.
		this.#segments.setValue(await fetchExampleSegments(meta.contentType?.unique));
	};

	override destroy() {
		this.#workspace?.variantOptionsManager.remove(this.#addedUniques);
		this.#workspace?.incomingDataHook.remove(this.#onIncomingData);
		super.destroy();
	}
}

export { ExampleSegmentProviderWorkspaceContext as api };
