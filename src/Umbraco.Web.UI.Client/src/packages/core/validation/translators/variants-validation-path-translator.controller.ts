import { UmbDataPathVariantFilter } from '../utils/index.js';
import { UmbValidationPathTranslatorBase } from './validation-path-translator-base.controller.js';

const InitialPathToMatch = '$.variants[';
const InitialPathToMatchLength = InitialPathToMatch.length;

export class UmbVariantsValidationPathTranslator extends UmbValidationPathTranslatorBase {
	translate(path: string) {
		if (!this._context) return;
		if (path.indexOf(InitialPathToMatch) !== 0) {
			// We do not handle this path.
			return false;
		}
		const pathEnd = path.indexOf(']');
		if (pathEnd === -1) {
			// We do not handle this path.
			return false;
		}
		// retrieve the number from the message values index: [NL]
		const index = parseInt(path.substring(InitialPathToMatchLength, pathEnd));

		if (isNaN(index)) {
			// index is not a number, this means its not a path we want to translate. [NL]
			return false;
		}

		// Get the data from the validation request, the context holds that for us: [NL]
		const data = this._context.getTranslationData();

		const specificValue = data.values[index];
		// replace the values[ number ] with JSON-Path filter values[@.(...)], continues by the rest of the path:
		return InitialPathToMatch + UmbDataPathVariantFilter(specificValue) + path.substring(path.indexOf(']'));
	}
}
