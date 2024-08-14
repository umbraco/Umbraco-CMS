import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	GetPropertyNameFromPath,
	UmbDataPathPropertyValueFilter,
	UmbValidationPathTranslatorBase,
} from '@umbraco-cms/backoffice/validation';

export class UmbBlockElementDataValidationPathTranslator extends UmbValidationPathTranslatorBase {
	constructor(host: UmbControllerHost) {
		super(host);
	}

	translate(path: string) {
		if (!this._context) return;
		if (path.indexOf('$.') !== 0) {
			// We do not handle this path.
			return false;
		}

		const rest = path.substring(2);
		const key = GetPropertyNameFromPath(rest);

		const specificValue = { alias: key };
		// replace the values[ number ] with JSON-Path filter values[@.(...)], continues by the rest of the path:
		//return '$.values' + UmbVariantValuesValidationPathTranslator(specificValue) + path.substring(path.indexOf(']'));
		return '$.values[' + UmbDataPathPropertyValueFilter(specificValue) + '.value';
	}
}
