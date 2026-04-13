import type { UmbValueSummaryResolver } from '@umbraco-cms/backoffice/value-summary';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

export class UmbSectionAliasesValueSummaryResolver
	extends UmbControllerBase
	implements UmbValueSummaryResolver<string[], ReadonlyArray<string>>
{
	#localize = new UmbLocalizationController(this);

	async resolveValues(values: ReadonlyArray<string[]>): Promise<ReadonlyArray<ReadonlyArray<string>>> {
		const sections = umbExtensionsRegistry.getByType('section');
		const nameByAlias = new Map(
			sections.map((s) => [s.alias, s.meta.label ? this.#localize.string(s.meta.label) : s.name]),
		);
		return values.map((aliases) => aliases.map((alias) => nameByAlias.get(alias) ?? alias));
	}
}

// Named 'api' for ApiLoaderProperty convention
export { UmbSectionAliasesValueSummaryResolver as api };
