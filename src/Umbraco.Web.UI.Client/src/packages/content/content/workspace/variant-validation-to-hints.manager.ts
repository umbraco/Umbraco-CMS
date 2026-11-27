import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbHintController, UmbVariantHint } from '@umbraco-cms/backoffice/hint';
import { extractFirstJsonQueryContaining, type UmbValidationController } from '@umbraco-cms/backoffice/validation';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

/*
 * @internal
 * @module UmbVariantValidationToHintsManager
 * @description
 * Observes validation messages of the variants data, such as a missing variant name, and turns them into hints on the variant they belong to.
 * The hints are not bound to a workspace view, as variant data is not presented within one.
 */
export class UmbVariantValidationToHintsManager extends UmbControllerBase {
	#hintedMsgs: Set<string> = new Set();

	constructor(host: UmbControllerHost, validation: UmbValidationController, hints: UmbHintController<UmbVariantHint>) {
		super(host);

		this.observe(
			validation.messages.messagesOfPathAndDescendant('$.variants'),
			(messages) => {
				messages.forEach((message) => {
					if (this.#hintedMsgs.has(message.key)) return;

					const queryWithCulture = extractFirstJsonQueryContaining(
						message.path,
						(props) => props.culture !== undefined,
					);
					const queryWithSegment = extractFirstJsonQueryContaining(
						message.path,
						(props) => props.segment !== undefined,
					);
					const variantId = UmbVariantId.CreateFromPartial({
						culture: queryWithCulture?.culture ?? null,
						segment: queryWithSegment?.segment ?? null,
					});

					hints.addOne({
						unique: message.key,
						path: [],
						text: '!',
						color: 'invalid',
						weight: 1000,
						variantId,
					});
					this.#hintedMsgs.add(message.key);
				});

				const removeKeys: Array<string> = [];
				this.#hintedMsgs.forEach((key) => {
					if (!messages.some((msg) => msg.key === key)) {
						this.#hintedMsgs.delete(key);
						removeKeys.push(key);
					}
				});
				hints.remove(removeKeys);
			},
			null,
		);
	}
}
