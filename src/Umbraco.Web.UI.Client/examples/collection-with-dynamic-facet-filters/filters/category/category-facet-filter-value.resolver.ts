import type { UmbFacetFilterValueResolver } from '@umbraco-cms/backoffice/facet-filter';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

const categoryNames: Record<string, string> = {
	'cat-hoodie': 'Hoodies',
	'cat-jacket': 'Jackets',
	'cat-shorts': 'Shorts',
	'cat-trousers': 'Trousers',
	'cat-tshirt': 'T-shirts',
};

/**
 * Resolves the active values of the category filter: `cat-tshirt` is displayed as "T-shirts".
 * Registered on the category filter only.
 */
export class ExampleCategoryFacetFilterValueResolver
	extends UmbControllerBase
	implements UmbFacetFilterValueResolver<string>
{
	async resolveValues(values: ReadonlyArray<string>) {
		return { data: values.map((value) => ({ name: categoryNames[value] ?? value })) };
	}
}

export { ExampleCategoryFacetFilterValueResolver as valueResolver };
