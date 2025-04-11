import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_CONTENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/content';
import {
	UMB_VALIDATION_CONTEXT,
	UmbDataPathPropertyValueQuery,
	type UmbValidator,
} from '@umbraco-cms/backoffice/validation';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbPropertyEditorRteValueType } from '@umbraco-cms/backoffice/rte';

// The Example Workspace Context Controller:
export class CustomValidationValidator extends UmbControllerBase implements UmbValidator {
	//
	#validationContext?: typeof UMB_VALIDATION_CONTEXT.TYPE;
	#workspaceContext?: typeof UMB_CONTENT_WORKSPACE_CONTEXT.TYPE;

	#propertyAlias: string;
	#variantId?: UmbVariantId;
	#dataPath: string;
	#isValid: boolean = false;
	#value?: UmbPropertyEditorRteValueType;

	constructor(host: UmbControllerHost, propertyAlias: string, variantId?: UmbVariantId) {
		super(host);
		this.#propertyAlias = propertyAlias;
		this.#variantId = variantId;
		this.#dataPath = `$.values[${UmbDataPathPropertyValueQuery({
			alias: this.#propertyAlias,
			culture: this.#variantId?.culture ?? null,
			segment: this.#variantId?.segment ?? null,
		})}].value`;

		this.consumeContext(UMB_VALIDATION_CONTEXT, (context) => {
			this.#validationContext = context;
			this.#checkValidation();
		});

		this.consumeContext(UMB_CONTENT_WORKSPACE_CONTEXT, async (context) => {
			this.#workspaceContext = context;
			this.observe(
				await context.propertyValueByAlias<UmbPropertyEditorRteValueType>(propertyAlias, this.#variantId),
				(value) => {
					this.#value = value;
					this.#checkValidation();
				},
			);
		});
	}

	#checkValidation() {
		// Check validation:
		if (this.#value?.markup) {
			const cleanedMarkup = this.#value.markup?.replace(/<\/?[^>]+(>|$)/g, '');
			// Check if markup contains words longer than 6 characters:
			const words = cleanedMarkup.split(/\s+/);
			const invalidWords = words.filter((word) => word.length > 10);
			if (invalidWords.length === 0) {
				this.#isValid = true;
			} else {
				this.#isValid = false;
			}
		} else {
			this.#isValid = false;
		}

		// Update validation message:
		if (this.#validationContext && this.#workspaceContext) {
			if (this.#isValid) {
				this.#validationContext.messages.removeMessagesByTypeAndPath('custom', this.#dataPath);
			} else {
				this.#validationContext.messages.addMessage('custom', this.#dataPath, 'Custom validation says this is invalid');
			}
		}
	}

	async validate(): Promise<void> {
		// validate is called when the validation state has to be fully resolved. Like when user clicks Save & Publish.
		// If you need to ask the server then it can be done here, instead of asking the server each time the value changes.
	}

	get isValid(): boolean {
		return this.#isValid;
	}

	reset(): void {
		this.#isValid = false;
	}

	/**
	 * Focus the first invalid element.
	 */
	focusFirstInvalidElement(): void {
		alert('custom validation is invalid, you should implement a feature to focus the problematic element');
	}
}

// Declare a api export, so Extension Registry can initialize this class:
export const api = CustomValidationValidator;

// Declare a Context Token, to other can request the context and to ensure typings.
export const EXAMPLE_CUSTOM_VALIDATION_CONTEXT = new UmbContextToken<CustomValidationValidator>(
	'UmbWorkspaceContext',
	'example.workspaceContext.counter',
);
