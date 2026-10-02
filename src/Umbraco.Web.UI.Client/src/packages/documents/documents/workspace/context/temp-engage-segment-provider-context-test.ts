import { UmbDocumentSegmentRepository } from '../../repository/index.js';
import type { UmbDocumentVariantOptionModel } from '../../types.js';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from './document-workspace.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { mergeObservables } from '@umbraco-cms/backoffice/observable-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbSegmentModel } from '@umbraco-cms/backoffice/segment';

export class UmbEngageSegmentProviderContextTest extends UmbControllerBase {
	#repository = new UmbDocumentSegmentRepository(this);
	#addedUniques: Array<string> = [];
	#requestId = 0;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (workspace) => {
			if (!workspace) return;
			const manager = workspace.variantOptionsManager;

			this.observe(
				mergeObservables(
					[workspace.unique, workspace.variesByCulture, workspace.variesBySegment, manager.languages],
					([unique, variesByCulture, variesBySegment, languages]) => ({
						unique,
						variesByCulture,
						variesBySegment,
						languages,
					}),
				),
				async ({ unique, variesByCulture, variesBySegment, languages }) => {
					const requestId = ++this.#requestId;

					manager.remove(this.#addedUniques);
					this.#addedUniques = [];

					if (!unique || variesByCulture === undefined || languages === undefined || !variesBySegment) return;

					const { data } = await this.#repository.getDocumentByIdSegmentOptions(unique, { skip: 0, take: 9999 });
					if (requestId !== this.#requestId || !data) return;

					const options = variesByCulture
						? languages.flatMap((language) =>
								this.#createOptions(
									data.items.filter((s) => !s.cultures || s.cultures.includes(language.unique)),
									language.unique,
								),
							)
						: this.#createOptions(
								data.items.filter((s) => !s.cultures),
								null,
							);

					this.#addedUniques = options.map((o) => o.unique);
					manager.add(options);
				},
				'_observeSegmentInputs',
			);
		});
	}

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
		this.#requestId++;
		super.destroy();
	}
}

export { UmbEngageSegmentProviderContextTest as api };
