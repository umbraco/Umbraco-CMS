import { Path } from 'msw';

import type { paths } from '../../../schemas/generated-schema';

export default function umbracoPath(path: keyof paths): Path {
	return `/umbraco/backoffice${path}`;
}
