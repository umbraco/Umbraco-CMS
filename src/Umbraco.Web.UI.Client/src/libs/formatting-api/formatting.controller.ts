import { DOMPurify } from '@umbraco-cms/backoffice/external/dompurify';
import { Marked } from '@umbraco-cms/backoffice/external/marked';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

const UmbMarked = new Marked({ gfm: true, breaks: true });
const UmbDomPurify = DOMPurify(window);
const UmbDomPurifyConfig: DOMPurify.Config = { USE_PROFILES: { html: true } };

UmbDomPurify.addHook('afterSanitizeAttributes', function (node) {
	// set all elements owning target to target=_blank
	if ('target' in node) {
		node.setAttribute('target', '_blank');
	}
});

/**
 * @description - Controller for formatting text.
 */
export class UmbFormattingController extends UmbControllerBase {
	#localize = new UmbLocalizationController(this._host);

	/**
	 * A method to localize the string input then transform any markdown to santized HTML.
	 */
	public transform(input?: string): string {
		if (!input) return '';
		const translated = this.#localize.string(input);
		const markup = UmbMarked.parse(translated) as string;
		const sanitized = UmbDomPurify.sanitize(markup, UmbDomPurifyConfig) as string;
		return sanitized;
	}
}
