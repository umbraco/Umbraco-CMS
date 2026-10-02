import { UmbDocumentSegmentRepository } from '../../repository/index.js';
import type { UmbDocumentDetailModel, UmbDocumentVariantOptionModel } from '../../types.js';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from './document-workspace.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbHookMethod } from '@umbraco-cms/backoffice/hook-api';
import type { UmbEntityDetailIncomingDataHookMeta } from '@umbraco-cms/backoffice/workspace';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { mergeObservables, UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbSegmentModel } from '@umbraco-cms/backoffice/segment';

export class UmbEngageSegmentProviderContextTest extends UmbControllerBase {
	#repository = new UmbDocumentSegmentRepository(this);
	#segments = new UmbArrayState<UmbSegmentModel>([], (x) => x.alias);
	#addedUniques: Array<string> = [];
	#workspace?: typeof UMB_DOCUMENT_WORKSPACE_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (workspace) => {
			this.#workspace?.incomingDataHook.remove(this.#onIncomingData);
			this.#workspace = workspace;
			if (!workspace) return;
			const manager = workspace.variantOptionsManager;

			workspace.incomingDataHook.add(this.#onIncomingData);

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
					manager.remove(this.#addedUniques);
					this.#addedUniques = [];

					if (variesByCulture === undefined || languages === undefined || !variesBySegment) return;

					const options = variesByCulture
						? languages.flatMap((language) =>
								this.#createOptions(
									segments.filter((s) => !s.cultures || s.cultures.includes(language.unique)),
									language.unique,
								),
							)
						: this.#createOptions(
								segments.filter((s) => !s.cultures),
								null,
							);

					this.#addedUniques = options.map((o) => o.unique);
					manager.add(options);
				},
				'_observeSegmentInputs',
			);
		});
	}

	#onIncomingData: UmbHookMethod<UmbDocumentDetailModel, UmbEntityDetailIncomingDataHookMeta> = async (data) => {
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
					variant: undefined,
					culture,
					segment: segment.alias,
					segmentInfo: { alias: segment.alias, name: segment.name },
					unique: new UmbVariantId(culture, segment.alias).toString(),
				}) as Omit<UmbDocumentVariantOptionModel, 'language'>,
		);
	}

	override destroy() {
		this.#workspace?.incomingDataHook.remove(this.#onIncomingData);
		super.destroy();
	}
}

export { UmbEngageSegmentProviderContextTest as api };
